using Microsoft.EntityFrameworkCore;
using NoteKeeper.Infrastructure.Persistence;

namespace NoteKeeper.Api.Extensions;

public static class WebApplicationExtensions
{
    extension(WebApplication webApplication)
    {
        public async Task InitializeDatabaseAsync()
        {
            await using var scope = webApplication.Services.CreateAsyncScope();

            await using var context = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            await context.Database.MigrateAsync();
        }
    }
}