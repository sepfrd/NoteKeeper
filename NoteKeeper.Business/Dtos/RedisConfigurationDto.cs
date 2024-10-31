namespace NoteKeeper.Business.Dtos;

public record RedisConfigurationDto
{
    public required string Endpoint { get; set; }

    public required int Port { get; set; }

    public required string User { get; set; }

    public required string Password { get; set; }

    public required int Database { get; set; }

    public required int ConnectTimeout { get; set; }

    public required bool UseSsl { get; set; }

    public required string ClientName { get; set; }

    public required int RetryAttempts { get; set; }

    public required int KeepAlive { get; set; }
}