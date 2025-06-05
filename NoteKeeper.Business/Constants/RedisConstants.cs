namespace NoteKeeper.Business.Constants;

public static class RedisConstants
{
    public const string NotesSubscriptionSetKey = "subscriptions:notes";
    public const string RefreshTokenStringKeyTemplate = "refresh_token:{0}";
    public const string ReverseRefreshTokenStringKeyTemplate = "refresh_token_reverse:{0}";
}