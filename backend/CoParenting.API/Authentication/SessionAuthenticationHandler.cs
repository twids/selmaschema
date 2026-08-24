using System.Security.Claims;
using System.Text.Encodings.Web;
using CoParenting.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CoParenting.API.Authentication;

public sealed class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthService authService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(AuthSchemes.AccountSessionCookie, out var token) || string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        var (success, account, session) = await authService.ValidateAccountSessionAsync(token);
        if (!success || account == null || session == null)
        {
            return AuthenticateResult.Fail("Invalid or expired account session");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Name, account.DisplayName),
            new Claim("selma_session_expires", session.ExpiresAt.ToString("O"))
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
