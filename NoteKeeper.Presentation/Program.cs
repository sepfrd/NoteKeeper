using Microsoft.EntityFrameworkCore;
using NoteKeeper.DataAccess;
using NoteKeeper.Presentation;
using NoteKeeper.Presentation.Constants;
using NoteKeeper.Presentation.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services
        .AddHttpContextAccessor()
        .AddHttpClient()
        .AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromHours(1d))
        .AddMemoryCacheEntryOptions()
        .AddOpenApi(options =>
            options
                .AddDocumentTransformer<BearerSecuritySchemeTransformer>()
                .AddDocumentTransformer<DocumentInfoTransformer>())
        .AddApiControllers()
        .AddRepositories()
        .AddServices()
        .AddNoteKeeperDbContext(builder.Configuration)
        .AddAuth()
        .AddJsonSerializerOptions()
        .AddRedisConnectionMultiplexer(builder.Configuration)
        .AddExternalServices()
        .AddCors(builder.Configuration);

    var app = builder.Build();

    await using var scope = app.Services.CreateAsyncScope();

    await using var context = scope.ServiceProvider.GetRequiredService<NoteKeeperDbContext>();

    await context.Database.MigrateAsync();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.DarkMode = true;
        options.Theme = ScalarTheme.BluePlanet;
        options.Title = "Note Keeper";
    });

    app.UseCors(app.Environment.IsProduction()
        ? CorsConstants.RestrictedCorsPolicy
        : CorsConstants.AllowAnyOriginCorsPolicy);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Console.WriteLine(exception);
}