namespace NoteKeeper.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication webApplication)
    {
        await using var scope = webApplication.Services.CreateAsyncScope();

        await using var context = scope.ServiceProvider.GetRequiredService<NoteKeeperDbContext>();

        await context.Database.MigrateAsync();
    }
}