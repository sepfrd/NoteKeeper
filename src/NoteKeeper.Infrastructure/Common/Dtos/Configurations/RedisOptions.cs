namespace NoteKeeper.Infrastructure.Common.Dtos.Configurations;

public record RedisOptions
{
    public required string Endpoint { get; set; }

    public required int Port { get; set; }

    public required string User { get; set; }

    public required string Password { get; set; }

    public required RedisDatabases Databases { get; set; }

    public required int ConnectTimeout { get; set; }

    public required bool UseSsl { get; set; }

    public required string ClientName { get; set; }

    public required int RetryAttempts { get; set; }

    public required int KeepAlive { get; set; }
}