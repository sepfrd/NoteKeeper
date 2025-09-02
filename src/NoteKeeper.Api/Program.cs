using DotNetEnv;
using NoteKeeper.Api.Constants;
using NoteKeeper.Api.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

try
{
    Env.TraversePath().Load();

    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddApplicationDependencies(builder.Configuration);

    var app = builder.Build();

    await app.InitializeDatabaseAsync();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.DarkMode = true;
        options.Theme = ScalarTheme.BluePlanet;
        options.Title = "Note Keeper";
    });

    app.UseRateLimiter();

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