namespace NoteKeeper.Infrastructure.Common.Dtos.Configurations;

public record VaultOptions(
    string ServerUri,
    string Token,
    string MountPoint,
    string SecretsPath);