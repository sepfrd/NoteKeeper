namespace NoteKeeper.Business.Dtos.Configurations;

public record AppOptions
{
    public required RedisOptions RedisOptions { get; set; }

    public required JwtOptions JwtOptions { get; set; }

    public required CorsOptions CorsOptions { get; set; }

    public required NotionOAuthOptions NotionOAuthOptions { get; set; }

    public required GoogleOAuthOptions GoogleOAuthOptions { get; set; }

    public required string DatabaseConnectionString { get; set; }

    public required string BaseApiUrl { get; set; }
}