using NoteKeeper.Infrastructure.Common.Dtos.Configurations.RateLimiters;

namespace NoteKeeper.Infrastructure.Common.Dtos.Configurations;

public record AppOptions
{
    public required RedisOptions RedisOptions { get; set; }

    public required JwtOptions JwtOptions { get; set; }

    public required CorsOptions CorsOptions { get; set; }

    public required NotionOAuthOptions NotionOAuthOptions { get; set; }

    public required GoogleOAuthOptions GoogleOAuthOptions { get; set; }

    public required HttpClientOptions HttpClientOptions { get; set; }

    public required CustomRateLimitersOptions RateLimitersOptions { get; set; }

    public required string DatabaseConnectionString { get; set; }

    public required string BaseApiUrl { get; set; }
}