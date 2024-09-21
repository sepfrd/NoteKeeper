using NoteKeeper.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpContextAccessor()
    .AddSwagger()
    .AddApiControllers()
    .AddRepositories()
    .AddServices()
    .AddNoteKeeperDbContext(builder.Configuration)
    .AddAuth()
    .AddJsonSerializerOptions();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();