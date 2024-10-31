using NoteKeeper.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpContextAccessor()
    .AddHttpClient()
    .AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromHours(1d))
    .AddMemoryCacheEntryOptions()
    .AddSwagger()
    .AddApiControllers()
    .AddRepositories()
    .AddServices()
    .AddNoteKeeperDbContext(builder.Configuration)
    .AddAuth()
    .AddJsonSerializerOptions()
    .AddRedisConnectionMultiplexer(builder.Configuration)
    .AddExternalServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();