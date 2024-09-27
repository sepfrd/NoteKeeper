using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.Business.Services;
using NoteKeeper.DataAccess;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;
using NoteKeeper.DataAccess.Repositories;
using NoteKeeper.Presentation.Authentication;

namespace NoteKeeper.Presentation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services) =>
        services
            .AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = MessageConstants.SwaggerAuthorizationMessage,
                    Name = HeaderNames.Authorization,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = JwtConstants.TokenType,
                    Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant()
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        []
                    }
                });
            });

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
            .AddScoped<IRepositoryBase<User>, RepositoryBase<User>>()
            .AddScoped<IRepositoryBase<Note>, RepositoryBase<Note>>();

    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddScoped<INoteService, NoteService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IAuthService, AuthService>();

    public static IServiceCollection AddNoteKeeperDbContext(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddDbContext<NoteKeeperDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(ConfigurationConstants.PostgreSqlConfigurationKey));
                options.EnableSensitiveDataLogging();
            });

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
}