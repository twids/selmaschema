namespace CoParenting.API.Authentication;

public static class AuthSchemes
{
    public const string AccountSession = "AccountSession";
    public const string AdminSession = "AdminSession";
    public const string Oidc = "oidc";
    public const string AdminOidc = "oidc-admin";
    public const string OidcTemporary = "OidcTemporary";
    public const string AdminOidcTemporary = "AdminOidcTemporary";
    public const string OidcIssuerProperty = ".selma.oidc.issuer";
    public const string AccountSessionCookie = "__Host-selma-session";
    public const string AccountCsrfCookie = "__Host-selma-csrf";
    public const string AdminSessionCookie = "__Host-selma-admin-session";
    public const string AdminCsrfCookie = "__Host-selma-admin-csrf";
    public const string OidcCookie = "__Host-selma-oidc";
    public const string AdminOidcCookie = "__Host-selma-admin-oidc";
}
