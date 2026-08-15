namespace CoParenting.API.Authentication;

public static class AuthSchemes
{
    public const string Session = "SessionAuth";
    public const string Oidc = "oidc";
    public const string OidcTemporary = "OidcTemporary";
    public const string SessionCookie = "__Host-selma-session";
    public const string OidcCookie = "__Host-selma-oidc";
}
