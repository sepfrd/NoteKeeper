using DotNetEnv;
using NoteKeeper.Api.Constants;
using NoteKeeper.Api.Extensions;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateBootstrapLogger();

try
{
    Env.TraversePath().Load();

    builder.Configuration.AddEnvironmentVariables();

    builder.Logging.ClearProviders();

    var appOptions = builder.Services.AddApplicationDependencies(
        builder.Configuration,
        builder.Environment);

    var app = builder.Build();

    await app.InitializeDatabaseAsync();

    app.UseExceptionHandler();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.DarkMode = true;
        options.Theme = ScalarTheme.BluePlanet;
        options.Title = appOptions.AppInformation.Name;
    });

    app.UseRouting();

    app.UseRateLimiter();

    app.UseCors(app.Environment.IsProduction()
        ? CorsConstants.RestrictedCorsPolicy
        : CorsConstants.AllowAnyOriginCorsPolicy);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Logger.Information("App started");

    app.Run();
}
catch (Exception exception)
{
    Console.WriteLine(exception);
}