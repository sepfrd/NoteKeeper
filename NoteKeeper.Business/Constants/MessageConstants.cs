namespace NoteKeeper.Business.Constants;

public static class MessageConstants
{
    // ---------------------------- Validation Error Messages ----------------------------
    public const string UsernameValidationErrorMessage = "Username must be 8-32 characters long and cannot start with a digit or underscore.";
    public const string PasswordValidationErrorMessage = "Password must be 8-32 characters long, contain at least one uppercase letter, one digit, and one special character.";
    public const string EmailValidationErrorMessage = "Email is not in a valid format.";

    // ---------------------------- Bad Request (400) Messages ----------------------------
    public const string AlreadySignedInMessage = "You are already signed in.";
    public const string PropertyNotUniqueMessage = "{0} is already taken.\nChoose another {1}.";
    public const string InvalidCredentialsMessage = "Username/Email and/or password not correct.";

    // ---------------------------- OK (200) Messages ----------------------------
    public const string SuccessfulLoginMessage = "Successfully authenticated.";

    // ---------------------------- Exception Messages ----------------------------

    public const string SwaggerAuthorizationMessage = "Please enter only the token (without Bearer)";
}