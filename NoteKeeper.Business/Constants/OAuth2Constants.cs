namespace NoteKeeper.Business.Constants;

public static class OAuth2Constants
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
    public const string NotionOwnerParameterName = NotionOwnerJsonPropertyName;
    public const string NotionUserOwnerType = NotionUserJsonPropertyName;

    public const string ConsentPrompt = "consent";
    public const string OfflineAccessType = "offline";
    public const string CodeResponseType = "code";
    public const string AuthorizationCodeGrantType = "authorization_code";
    public const string RefreshTokenGrantType = "refresh_token";

    public const string AccessTokenJsonPropertyName = "access_token";
    public const string ExpiresInJsonPropertyName = "expires_in";
    public const string RefreshTokenJsonPropertyName = "refresh_token";
    public const string TokenTypeJsonPropertyName = "token_type";
    public const string IdTokenJsonPropertyName = "id_token";
    public const string ErrorDescriptionJsonPropertyName = "error_description";
    public const string NotionEmailJsonPropertyName = "email";
    public const string NotionTypeJsonPropertyName = "type";
    public const string NotionUserJsonPropertyName = "user";
    public const string NotionBotIdJsonPropertyName = "bot_id";
    public const string NotionWorkspaceNameJsonPropertyName = "workspace_name";
    public const string NotionWorkspaceIconJsonPropertyName = "workspace_icon";
    public const string NotionWorkspaceIdJsonPropertyName = "workspace_id";
    public const string NotionOwnerJsonPropertyName = "owner";
    public const string NotionDuplicatedTemplateIdJsonPropertyName = "duplicated_template_id";
    public const string NotionRequestIdJsonPropertyName = "request_id";
    public const string NotionObjectJsonPropertyName = "object";
    public const string NotionIdJsonPropertyName = "id";
    public const string NotionNameJsonPropertyName = "name";
    public const string NotionPersonJsonPropertyName = "person";
    public const string NotionAvatarUrlJsonPropertyName = "avatar_url";
    public const string ScopeJsonPropertyName = ScopeParameterName;
    public const string GrantTypeJsonPropertyName = GrantTypeParameterName;
    public const string CodeJsonPropertyName = CodeParameterName;
    public const string RedirectUriJsonPropertyName = RedirectUriParameterName;
}