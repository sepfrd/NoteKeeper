using Microsoft.EntityFrameworkCore;
using NoteKeeper.Infrastructure.Persistence;

namespace NoteKeeper.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication webApplication)
    {
        await using var scope = webApplication.Services.CreateAsyncScope();

        await using var context = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

        await context.Database.MigrateAsync();
    }
}