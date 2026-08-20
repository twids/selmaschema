using System.Security.Claims;
using System.Text.Encodings.Web;
using CoParenting.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CoParenting.API.Authentication;

public sealed class AdminSessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthService authService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(AuthSchemes.AdminSessionCookie, out var token) || string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        var (success, session) = await authService.ValidateAdminSessionAsync(token);
        if (!success || session == null)
        {
            return AuthenticateResult.Fail("Invalid or expired platform admin session");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.AccountId?.ToString() ?? "break-glass"),
            new(ClaimTypes.Email, session.Email),
            new(ClaimTypes.Name, session.DisplayName),
            new("selma_platform_admin", "true"),
            new("selma_admin_auth_method", session.AuthenticationMethod),
            new("selma_session_expires", session.ExpiresAt.ToString("O"))
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
