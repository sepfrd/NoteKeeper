namespace NoteKeeper.Business.Constants;

public static class GoogleOAuth2Constants
{
    public const string ClientIdParameterName = "client_id";
    public const string AccessTypeParameterName = "access_type";
    public const string IncludeGrantedScopesParameterName = "include_granted_scopes";
    public const string ResponseTypeParameterName = "response_type";
    public const string StateParameterName = "state";
    public const string ScopeParameterName = "scope";
    public const string RedirectUriParameterName = "redirect_uri";
    public const string ClientSecretParameterName = "client_secret";
    public const string GrantTypeParameterName = "grant_type";
    public const string PromptParameterName = "prompt";
    public const string TokenParameterName = "token";
    public const string CodeParameterName = CodeResponseType;
    public const string RefreshTokenParameterName = RefreshTokenJsonPropertyName;

    public const string ConsentPrompt = "consent";
    public const string OfflineAccessType = "offline";
    public const string CodeResponseType = "code";
    public const string AuthorizationCodeGrantType = "authorization_code";
    public const string RefreshTokenGrantType = "refresh_token";

    public const string AccessTokenJsonPropertyName = "access_token";
    public const string ExpiresInJsonPropertyName = "expires_in";
    public const string RefreshTokenJsonPropertyName = "refresh_token";
    public const string ScopeJsonPropertyName = ScopeParameterName;
    public const string TokenTypeJsonPropertyName = "token_type";
    public const string IdTokenJsonPropertyName = "id_token";
    public const string ErrorDescriptionJsonPropertyName = "error_description";
}