using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;
using NoteKeeper.DataAccess.Repositories;

namespace NoteKeeper.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<IRepositoryBase<User>, RepositoryBase<User>>()
            .AddScoped<IRepositoryBase<Note>, RepositoryBase<Note>>();

        services.AddDbContext<NoteKeeperDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Postgres"));
            options.EnableSensitiveDataLogging();
        });

        return services;
    }
}