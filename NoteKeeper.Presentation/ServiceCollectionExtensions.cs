using System.Text.Json;
using System.Text.Json.Serialization;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Dtos.DomainEntities;
using NoteKeeper.Business.ExternalServices;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.Business.Services;
using NoteKeeper.Business.Services.OAuth.V2;
using NoteKeeper.DataAccess;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;
using NoteKeeper.DataAccess.Repositories;
using NoteKeeper.Presentation.Authentication;
using StackExchange.Redis;

namespace NoteKeeper.Presentation;

public static class ServiceCollectionExtensions
{
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

    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IRepositoryBase<Note>, RepositoryBase<Note>>()
            .AddScoped<IRepositoryBase<GoogleToken>, RepositoryBase<GoogleToken>>()
            .AddScoped<IRepositoryBase<NotionToken>, RepositoryBase<NotionToken>>();

    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddScoped<INoteService, NoteService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IGoogleOAuth2Service, GoogleOAuth2Service>()
            .AddScoped<INotionOAuth2Service, NotionOAuth2Service>();

    public static IServiceCollection AddNoteKeeperDbContext(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddDbContext<NoteKeeperDbContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString(ConfigurationConstants.PostgreSqlConnectionStringKey))
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

    public static IServiceCollection AddRedisConnectionMultiplexer(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfigurationDto = configuration
            .GetSection(ConfigurationConstants.RedisConfigurationSectionKey)
            .Get<RedisConfigurationDto>()!;

        var redisConnectionString = $"{redisConfigurationDto.Endpoint}:{redisConfigurationDto.Port}";

        var redisConfigurationOptions = new ConfigurationOptions
        {
            Ssl = redisConfigurationDto.UseSsl,
            User = redisConfigurationDto.User,
            Password = redisConfigurationDto.Password,
            KeepAlive = redisConfigurationDto.KeepAlive,
            ConnectTimeout = redisConfigurationDto.ConnectTimeout,
            ConnectRetry = redisConfigurationDto.RetryAttempts,
            ClientName = redisConfigurationDto.ClientName,
            DefaultDatabase = redisConfigurationDto.Database,
            EndPoints = { redisConnectionString },
            AbortOnConnectFail = false
        };

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfigurationOptions);

        connectionMultiplexer.GetDatabase().KeyDelete(ApplicationConstants.RedisNotesSubscriptionSetKey);

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        return services;
    }

    public static IServiceCollection AddExternalServices(this IServiceCollection services) =>
        services
            .AddScoped<IRedisPubSubService<NoteDto>, RedisPubSubService<NoteDto>>()
            .AddScoped<IRedisService, RedisService>();

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