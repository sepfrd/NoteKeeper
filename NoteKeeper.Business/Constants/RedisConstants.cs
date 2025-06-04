namespace NoteKeeper.Business.Constants;

public static class RedisConstants
{
    public const string NotesSubscriptionSetKey = "subscriptions:notes";
    public const string ValidRefreshTokenStringKeyTemplate = "refresh_token:{0}";
    public const string ReverseValidRefreshTokenStringKeyTemplate = "refresh_token_reverse:{0}";
    public const string InvalidatedRefreshTokenStringKeyTemplate = "invalidated_refresh_token:{0}";
}