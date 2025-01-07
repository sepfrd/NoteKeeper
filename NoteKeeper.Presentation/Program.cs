using Microsoft.EntityFrameworkCore;
using NoteKeeper.DataAccess;
using NoteKeeper.Presentation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services
        .AddHttpContextAccessor()
        .AddHttpClient()
        .AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromHours(1d))
        .AddMemoryCacheEntryOptions()
        .AddOpenApi()
        .AddApiControllers()
        .AddRepositories()
        .AddServices()
        .AddNoteKeeperDbContext(builder.Configuration)
        .AddAuth()
        .AddJsonSerializerOptions()
        .AddRedisConnectionMultiplexer(builder.Configuration)
        .AddExternalServices();

    var app = builder.Build();

    await using var scope = app.Services.CreateAsyncScope();

    await using var context = scope.ServiceProvider.GetRequiredService<NoteKeeperDbContext>();

    await context.Database.MigrateAsync();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Console.WriteLine(exception);
}