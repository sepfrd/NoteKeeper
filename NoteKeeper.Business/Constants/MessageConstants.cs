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
    public const string NoTokensFound = "You have not authorized this app yet.";

    // ---------------------------- Not Found (404) Messages ----------------------------
    public const string EntityNotFoundByGuidMessage = "{0} with GUID of {1} not found.";

    // ---------------------------- Internal Server Error (500) Messages ----------------------------
    public const string GoogleOAuthFailureMessage = "Google OAuth authentication failed. Please try again or contact support.";
    public const string GoogleTokenRevocationFailureMessage = "Google token(s) revocation failed. Please try again or contact support.";

    // ---------------------------- OK (200) Messages ----------------------------
    public const string SuccessfulLoginMessage = "Successfully authenticated.";
    public const string SuccessfulUpdateMessage = "Successfully updated {0} entity.";
    public const string SuccessfulDeleteMessage = "Successfully deleted {0} entity.";
    public const string GoogleOAuthSuccessMessage = "Google OAuth authentication was successful. You have successfully authorized access.";
    public const string GoogleTokenRevocationSuccessMessage = "Google token(s) revocation was successful. You have successfully revoked access.";

    // ---------------------------- Exception Messages ----------------------------

    public const string SwaggerAuthorizationMessage = "Please enter only the token (without Bearer)";
}