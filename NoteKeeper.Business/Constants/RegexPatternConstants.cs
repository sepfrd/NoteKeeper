namespace NoteKeeper.Business.Constants;

public static class RegexPatternConstants
{
    public const string UsernameRegexPattern = @"^(?![\d_])[\w\d_]{8,32}$";
    public const string PasswordRegexPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[.,!@#$%^&*()_+])[A-Za-z\d.,!@#$%^&*()_+]{8,32}$";
    public const string EmailRegexPattern = @"^[\w.-]{1,64}@[\w.-]{1,251}\.[\w]{2,4}$";
}