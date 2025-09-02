using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BCrypt.Net;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NoteKeeper.Api.Authentication;
using NoteKeeper.Api.Constants;
using NoteKeeper.Api.Transformers;
using NoteKeeper.Application.Features.Notes.Commands.CreateNote;
using NoteKeeper.Application.Features.Notes.Commands.DeleteByUuid;
using NoteKeeper.Application.Features.Notes.Commands.UpdateByUuid;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Features.Notes.Queries.GetAllNotes;
using NoteKeeper.Application.Features.Notes.Queries.GetAllNotesCount;
using NoteKeeper.Application.Features.Notes.Queries.GetNoteByUuid;
using NoteKeeper.Application.Features.Users.Commands.CreateUser;
using NoteKeeper.Application.Features.Users.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Domain.Enums;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations;
using NoteKeeper.Infrastructure.Common.Dtos.Configurations.RateLimiters;
using NoteKeeper.Infrastructure.ExternalServices;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Models;
using NoteKeeper.Infrastructure.Interfaces;
using NoteKeeper.Infrastructure.Persistence;
using NoteKeeper.Infrastructure.Services;
using NoteKeeper.Infrastructure.Validators;
using NoteKeeper.Shared.Utilities;
using StackExchange.Redis;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using CorsConstants = NoteKeeper.Api.Constants.CorsConstants;

namespace NoteKeeper.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        var useVault = configuration.GetValue<bool>(KeyConstants.UseVaultEnvironmentVariableKey);

        var appOptions = useVault ? GetAppOptionsFromVault(configuration) : configuration.Get<AppOptions>()!;

        ConfigureMapster();

        services
            .AddAppOptions(appOptions)
            .AddHttpContextAccessor()
            .AddHttpClients(appOptions.HttpClientOptions)
            .AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromHours(1d))
            .AddMemoryCacheEntryOptions()
            .AddRateLimiters(appOptions.RateLimitersOptions)
            .AddOpenApi(options =>
                options
                    .AddDocumentTransformer<BearerSecuritySchemeTransformer>()
                    .AddDocumentTransformer<DocumentInfoTransformer>())
            .AddApiControllers()
            .AddServices()
            .AddCommandHandlers()
            .AddQueryHandlers()
            .AddDatabase(appOptions)
            .AddAuth()
            .AddJsonSerializerOptions()
            .AddRedisConnectionMultiplexer(appOptions.RedisOptions)
            .AddExternalServices()
            .AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>(ServiceLifetime.Singleton)
            .AddCors(appOptions.CorsOptions)
            .AddSingleton<IDbConnectionPool, DbConnectionPool>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AppOptions>>().Value;

                DbConnectionPool.Initialize(options.DatabaseConnectionString);

                return DbConnectionPool.Instance;
            });

        return services;
    }

    public static IServiceCollection AddApiControllers(this IServiceCollection services) =>
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            })
            .Services
            .AddEndpointsApiExplorer();

    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<ITokenService, TokenService>()
            .AddScoped<IGoogleOAuth2Service, GoogleOAuth2Service>()
            .AddScoped<INotionOAuth2Service, NotionOAuth2Service>()
            .AddScoped<IMappingService, MappingService>();

    public static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services
            .AddScoped<ICommandHandler<CreateUserCommand, DomainResult<UserDto>>, CreateUserCommandHandler>()
            .AddScoped<ICommandHandler<CreateNoteCommand, DomainResult<NoteDto>>, CreateNoteCommandHandler>()
            .AddScoped<ICommandHandler<DeleteNoteCommand, DomainResult>, DeleteNoteCommandHandler>()
            .AddScoped<ICommandHandler<UpdateNoteCommand, DomainResult<NoteDto>>, UpdateNoteCommandHandler>();

    public static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services
            .AddScoped<IQueryHandler<GetAllNotesByFilterQuery, PaginatedDomainResult<IEnumerable<NoteDto>>>, GetAllNotesByFilterQueryHandler>()
            .AddScoped<IQueryHandler<GetAllNotesCountQuery, DomainResult<long>>, GetAllNotesCountQueryHandler>()
            .AddScoped<IQueryHandler<GetSingleNoteQuery, DomainResult<NoteDto>>, GetSingleNoteQueryHandler>();

    public static IServiceCollection AddDatabase(this IServiceCollection services, AppOptions appOptions) =>
        services
            .AddDbContext<IUnitOfWork, UnitOfWork>(options =>
                options
                    .UseNpgsql(appOptions.DatabaseConnectionString)
                    .EnableSensitiveDataLogging()
                    .UseSeeding((dbContext, _) => dbContext.SeedDatabase())
                    .UseAsyncSeeding((dbContext, _, _) => Task.FromResult(dbContext.SeedDatabase())));

    public static IServiceCollection AddAuth(this IServiceCollection services) =>
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = Ed25519JwtAuthenticationSchemeOptions.DefaultScheme;
                options.DefaultChallengeScheme = Ed25519JwtAuthenticationSchemeOptions.DefaultScheme;
                options.DefaultForbidScheme = Ed25519JwtAuthenticationSchemeOptions.DefaultScheme;
                options.DefaultAuthenticateScheme = Ed25519JwtAuthenticationSchemeOptions.DefaultScheme;
            })
            .AddScheme<Ed25519JwtAuthenticationSchemeOptions, Ed25519JwtAuthenticationHandler>(
                Ed25519JwtAuthenticationSchemeOptions.DefaultScheme,
                _ => { })
            .Services
            .AddAuthorization();

    public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services) =>
        services
            .AddSingleton(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

    public static IServiceCollection AddMemoryCacheEntryOptions(this IServiceCollection services) =>
        services
            .AddSingleton(new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5d)
            });

    public static IServiceCollection AddRedisConnectionMultiplexer(this IServiceCollection services, RedisOptions redisOptions)
    {
        var redisConnectionString = $"{redisOptions.Endpoint}:{redisOptions.Port}";

        var redisConfigurationOptions = new ConfigurationOptions
        {
            Ssl = redisOptions.UseSsl,
            User = redisOptions.User,
            Password = redisOptions.Password,
            KeepAlive = redisOptions.KeepAlive,
            ConnectTimeout = redisOptions.ConnectTimeout,
            ConnectRetry = redisOptions.RetryAttempts,
            ClientName = redisOptions.ClientName,
            DefaultDatabase = redisOptions.Databases.Default,
            EndPoints = { redisConnectionString },
            AbortOnConnectFail = false
        };

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfigurationOptions);

        connectionMultiplexer.GetDatabase().KeyDelete(RedisConstants.NotesSubscriptionSetKey);

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        return services;
    }

    public static IServiceCollection AddExternalServices(this IServiceCollection services) =>
        services
            .AddScoped<IRedisService, RedisService>();

    public static IServiceCollection AddCors(this IServiceCollection services, CorsOptions corsOptions) =>
        services.AddCors(options =>
        {
            options.AddPolicy(CorsConstants.RestrictedCorsPolicy, builder =>
            {
                builder
                    .AllowAnyMethod()
                    .WithHeaders(
                        HeaderNames.Accept,
                        HeaderNames.ContentType,
                        HeaderNames.Authorization)
                    .AllowCredentials()
                    .SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrWhiteSpace(origin))
                        {
                            return false;
                        }

                        if (corsOptions.AllowedUrls.Contains(origin, StringComparer.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }

                        return false;
                    });
            });

            options
                .AddPolicy(CorsConstants.AllowAnyOriginCorsPolicy, builder =>
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
        });

    public static IServiceCollection AddHttpClients(this IServiceCollection services, HttpClientOptions httpClientOptions) =>
        services
            .AddHttpClient<GoogleOAuth2Service>(client =>
                client.Timeout = TimeSpan.FromMilliseconds(httpClientOptions.DefaultTimeoutInMilliseconds))
            .Services
            .AddHttpClient<NotionOAuth2Service>(client =>
                client.Timeout = TimeSpan.FromMilliseconds(httpClientOptions.DefaultTimeoutInMilliseconds))
            .Services;

    public static IServiceCollection AddRateLimiters(this IServiceCollection services, CustomRateLimitersOptions rateLimiterOptions) =>
        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var authService = context.RequestServices.GetRequiredService<IAuthService>();

                    var userUuid = authService.GetSignedInUserUuid();

                    var userKey = !string.IsNullOrEmpty(userUuid)
                        ? userUuid
                        : context.Connection.RemoteIpAddress?.ToString() ?? KeyConstants.UnknownIPAddressKey;

                    return RateLimitPartition.GetFixedWindowLimiter(
                        userKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimiterOptions.FixedWindowRateLimiterOptions!.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimiterOptions.FixedWindowRateLimiterOptions.WindowSeconds),
                            QueueLimit = rateLimiterOptions.FixedWindowRateLimiterOptions.QueueLimit,
                            QueueProcessingOrder = rateLimiterOptions.FixedWindowRateLimiterOptions.QueueProcessingOrder,
                            AutoReplenishment = rateLimiterOptions.FixedWindowRateLimiterOptions.AutoReplenishment
                        });
                }),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var authService = context.RequestServices.GetRequiredService<IAuthService>();

                    var userUuid = authService.GetSignedInUserUuid();

                    var userKey = !string.IsNullOrEmpty(userUuid)
                        ? userUuid
                        : context.Connection.RemoteIpAddress?.ToString() ?? KeyConstants.UnknownIPAddressKey;

                    return RateLimitPartition.GetConcurrencyLimiter(
                        userKey,
                        _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = rateLimiterOptions.ConcurrencyLimiterOptions!.PermitLimit,
                            QueueLimit = rateLimiterOptions.ConcurrencyLimiterOptions.QueueLimit,
                            QueueProcessingOrder = rateLimiterOptions.ConcurrencyLimiterOptions.QueueProcessingOrder
                        });
                }),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var authService = context.RequestServices.GetRequiredService<IAuthService>();

                    var userUuid = authService.GetSignedInUserUuid();

                    var userKey = !string.IsNullOrEmpty(userUuid)
                        ? userUuid
                        : context.Connection.RemoteIpAddress?.ToString() ?? KeyConstants.UnknownIPAddressKey;

                    return RateLimitPartition.GetTokenBucketLimiter(
                        userKey,
                        _ => new TokenBucketRateLimiterOptions
                        {
                            AutoReplenishment = rateLimiterOptions.TokenBucketRateLimiterOptions!.AutoReplenishment,
                            QueueLimit = rateLimiterOptions.TokenBucketRateLimiterOptions.QueueLimit,
                            QueueProcessingOrder = rateLimiterOptions.TokenBucketRateLimiterOptions.QueueProcessingOrder,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimiterOptions.TokenBucketRateLimiterOptions.ReplenishmentPeriodSeconds),
                            TokenLimit = rateLimiterOptions.TokenBucketRateLimiterOptions.TokenLimit,
                            TokensPerPeriod = rateLimiterOptions.TokenBucketRateLimiterOptions.TokensPerPeriod
                        });
                }));

            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

    private static IServiceCollection AddAppOptions(this IServiceCollection services, AppOptions appOptions) =>
        services.Configure<AppOptions>(options =>
        {
            options.BaseApiUrl = appOptions.BaseApiUrl;
            options.CorsOptions = appOptions.CorsOptions;
            options.DatabaseConnectionString = appOptions.DatabaseConnectionString;
            options.GoogleOAuthOptions = appOptions.GoogleOAuthOptions;
            options.HttpClientOptions = appOptions.HttpClientOptions;
            options.RateLimitersOptions = appOptions.RateLimitersOptions;
            options.JwtOptions = appOptions.JwtOptions;
            options.NotionOAuthOptions = appOptions.NotionOAuthOptions;
            options.RedisOptions = appOptions.RedisOptions;
        });

    private static AppOptions GetAppOptionsFromVault(IConfiguration configuration)
    {
        var vaultOptions = configuration.GetSection(nameof(VaultOptions)).Get<VaultOptions>()!;

        var authMethod = new TokenAuthMethodInfo(vaultOptions.Token);
        var vaultClientSettings = new VaultClientSettings(vaultOptions.ServerUri, authMethod);
        var vaultClient = new VaultClient(vaultClientSettings);

        var configs = vaultClient.V1.Secrets.KeyValue.V2
            .ReadSecretAsync<AppOptions>(path: vaultOptions.SecretsPath, mountPoint: vaultOptions.MountPoint)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult()
            .Data
            .Data;

        return configs;
    }

    private static void ConfigureMapster()
    {
        TypeAdapterConfig<CreateUserCommand, User>
            .ForType()
            .Map(
                user => user.PasswordHash,
                command => CryptographyHelper.HashPassword(command.Password));

        TypeAdapterConfig<UpdateNoteCommand, Note>
            .ForType()
            .Map(note => note.Title, command => command.NewTitle)
            .Map(note => note.Content, command => command.NewContent);

        TypeAdapterConfig<GoogleIdTokenPayloadDto, User>
            .ForType()
            .Map(user => user.Username, src => src.Subject)
            .Map(user => user.Email, src => src.Email)
            .Map(user => user.FirstName, src => src.GivenName)
            .Map(user => user.LastName, src => src.FamilyName)
            .Map(user => user.RegistrationType, src => RegistrationType.Google);

        TypeAdapterConfig<GoogleTokenResponseDto, GoogleToken>
            .ForType()
            .Map(googleToken => googleToken.AccessToken, src => src.AccessToken)
            .Map(googleToken => googleToken.ExpiresAt, src => DateTimeOffset.UtcNow.AddSeconds(src.ExpiresIn))
            .Map(googleToken => googleToken.RefreshToken, src => src.RefreshToken!)
            .Map(googleToken => googleToken.Scope, src => src.Scope)
            .Map(googleToken => googleToken.TokenType, src => src.TokenType)
            .Map(googleToken => googleToken.IdToken, src => src.IdToken);

        // TODO: handle null values of the src
        TypeAdapterConfig<NotionTokenResponseDto, NotionToken>
            .ForType()
            .Map(notionToken => notionToken.AccessToken, src => src.AccessToken)
            .Map(notionToken => notionToken.TokenType, src => src.TokenType)
            .Map(notionToken => notionToken.BotId, src => src.BotId)
            .Map(notionToken => notionToken.WorkspaceName, src => src.WorkspaceName)
            .Map(notionToken => notionToken.WorkspaceIconUrl, src => src.WorkspaceIcon)
            .Map(notionToken => notionToken.WorkspaceId, src => src.WorkspaceId)
            .Map(notionToken => notionToken.NotionId, src => src.Owner!.User!.Id)
            .Map(notionToken => notionToken.Name, src => src.Owner!.User!.Name)
            .Map(notionToken => notionToken.AvatarUrl, src => src.Owner!.User!.AvatarUrl)
            .Map(notionToken => notionToken.NotionEmail, src => src.Owner!.User!.Person!.Email);
    }

    private static DbContext SeedDatabase(this DbContext dbContext)
    {
        var usersDbSet = dbContext.Set<User>();

        if (usersDbSet.Any())
        {
            return dbContext;
        }

        usersDbSet.Add(new User
        {
            Username = "sepehr_frd",
            Email = "sepfrd@outlook.com",
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("Sfr1376.", HashType.SHA512, 12)
        });

        dbContext.SaveChanges();

        return dbContext;
    }
}