using System.Security.Claims;
using System.Text.Json;
using CoParenting.API.Authentication;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CoParenting.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Authentication");

        auth.MapGet("/login", (string? returnUrl) =>
        {
            var safeReturnUrl = SafeReturnUrl(returnUrl);
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/api/auth/oidc/complete?returnUrl={Uri.EscapeDataString(safeReturnUrl)}"
            };
            return Results.Challenge(properties, [AuthSchemes.Oidc]);
        }).AllowAnonymous().RequireRateLimiting("oidc-login");

        auth.MapGet("/oidc/complete", async (
            HttpContext context,
            string? returnUrl,
            IAuthService authService) =>
        {
            var external = await context.AuthenticateAsync(AuthSchemes.OidcTemporary);
            var login = ToExternalLogin(external.Principal, external.Properties);
            if (!external.Succeeded || login == null)
            {
                await context.SignOutAsync(AuthSchemes.OidcTemporary);
                return Results.Redirect("/login?error=oidc_failed");
            }

            var result = await authService.SignInExternalAsync(login);
            await context.SignOutAsync(AuthSchemes.OidcTemporary);
            if (!result.Success || result.RawToken == null || result.RawCsrfToken == null || result.Session == null)
            {
                return Results.Redirect("/login?error=oidc_failed");
            }

            SetAccountCookies(context, result.RawToken, result.RawCsrfToken, result.Session.ExpiresAt);
            return Results.Redirect(SafeReturnUrl(returnUrl));
        }).AllowAnonymous();

        auth.MapGet("/me", async (HttpContext context, IFamilyService families) =>
        {
            var accountId = AccountId(context.User);
            if (accountId == null)
            {
                return Results.Unauthorized();
            }

            var account = new AccountDto(
                accountId.Value,
                context.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty);
            return Results.Ok(new AuthMeDto(account, await families.GetMembershipsAsync(accountId.Value)));
        }).RequireAuthorization();

        auth.MapPost("/logout", async (HttpContext context, IAuthService authService) =>
        {
            if (context.Request.Cookies.TryGetValue(AuthSchemes.AccountSessionCookie, out var token))
            {
                await authService.RevokeAccountSessionAsync(token);
            }

            DeleteAccountCookies(context);
            return Results.NoContent();
        }).RequireAuthorization();

        if (app.Environment.IsEnvironment("EndToEnd") || app.Environment.IsEnvironment("Testing"))
        {
            auth.MapPost("/test-login", async (
                EndToEndLoginRequest request,
                HttpContext context,
                IConfiguration configuration,
                IAuthService authService) =>
            {
                var suppliedSecret = context.Request.Headers["X-Selma-E2E-Secret"].FirstOrDefault();
                var expectedSecret = configuration["EndToEnd:LoginSecret"];
                if (string.IsNullOrWhiteSpace(expectedSecret) || !string.Equals(suppliedSecret, expectedSecret, StringComparison.Ordinal))
                {
                    return Results.NotFound();
                }

                var result = await authService.SignInExternalAsync(new ExternalLoginInfo(
                    "https://e2e.selma.invalid", request.Email, request.Email, request.DisplayName, true));
                if (!result.Success || result.RawToken == null || result.RawCsrfToken == null || result.Session == null)
                {
                    return Results.BadRequest();
                }

                SetAccountCookies(context, result.RawToken, result.RawCsrfToken, result.Session.ExpiresAt);
                return Results.NoContent();
            }).AllowAnonymous();
        }

        var join = app.MapGroup("/api/join").WithTags("Joining");
        join.MapGet("/link/{token}", async (string token, IFamilyService families) =>
        {
            if (!await families.IsInvitationTokenValidAsync(token))
            {
                return Results.Redirect("/join?error=invalid_invitation");
            }

            var returnUrl = $"/join/{Uri.EscapeDataString(token)}";
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/api/auth/oidc/complete?returnUrl={Uri.EscapeDataString(returnUrl)}"
            };
            return Results.Challenge(properties, [AuthSchemes.Oidc]);
        }).AllowAnonymous().RequireRateLimiting("oidc-login");

        join.MapGet("/{token}", async (string token, HttpContext context, IFamilyService families) =>
        {
            var accountId = AccountId(context.User);
            var preview = accountId == null ? null : await families.GetJoinPreviewByTokenAsync(accountId.Value, token);
            return preview == null ? Results.NotFound() : Results.Ok(preview);
        }).RequireAuthorization();

        join.MapPost("/{token}/complete", async (string token, HttpContext context, IFamilyService families) =>
        {
            var accountId = AccountId(context.User);
            var result = accountId == null ? null : await families.RedeemTokenAsync(accountId.Value, token);
            return result == null ? Results.Conflict(new { error = "Invitation is invalid, expired, or already used" }) : Results.Ok(result);
        }).RequireAuthorization().RequireRateLimiting("join-code");

        join.MapPost("/code", async (JoinByCodeRequest request, HttpContext context, IFamilyService families) =>
        {
            var accountId = AccountId(context.User);
            var result = accountId == null ? null : await families.RedeemCodeAsync(accountId.Value, request.Code);
            return result == null ? Results.BadRequest(new { error = "Koden är ogiltig eller har gått ut" }) : Results.Ok(result);
        }).RequireAuthorization().RequireRateLimiting("join-code");

        join.MapPost("/code/preview", async (JoinByCodeRequest request, HttpContext context, IFamilyService families) =>
        {
            var accountId = AccountId(context.User);
            var preview = accountId == null ? null : await families.GetJoinPreviewByCodeAsync(accountId.Value, request.Code);
            return preview == null ? Results.BadRequest(new { error = "Koden är ogiltig eller har gått ut" }) : Results.Ok(preview);
        }).RequireAuthorization().RequireRateLimiting("join-code");
    }

    public static void MapAdminAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/admin/auth").WithTags("Platform admin authentication");

        auth.MapGet("/login", () =>
        {
            var properties = new AuthenticationProperties { RedirectUri = "/api/admin/auth/oidc/complete" };
            return Results.Challenge(properties, [AuthSchemes.AdminOidc]);
        }).AllowAnonymous().RequireRateLimiting("oidc-login");

        auth.MapGet("/oidc/complete", async (HttpContext context, IAuthService authService) =>
        {
            var external = await context.AuthenticateAsync(AuthSchemes.AdminOidcTemporary);
            var login = ToExternalLogin(external.Principal, external.Properties);
            if (!external.Succeeded || login == null)
            {
                await context.SignOutAsync(AuthSchemes.AdminOidcTemporary);
                return Results.Redirect("/admin/login?error=oidc_failed");
            }

            var result = await authService.SignInPlatformAdminAsync(login);
            await context.SignOutAsync(AuthSchemes.AdminOidcTemporary);
            if (!result.Success || result.RawToken == null || result.RawCsrfToken == null || result.Session == null)
            {
                return Results.Redirect("/admin/login?error=not_authorized");
            }

            SetAdminCookies(context, result.RawToken, result.RawCsrfToken, result.Session.ExpiresAt);
            return Results.Redirect("/admin");
        }).AllowAnonymous();

        auth.MapPost("/break-glass", async (BreakGlassLoginRequest request, HttpContext context, IAuthService authService) =>
        {
            var result = await authService.ValidateBreakGlassPasswordAsync(request.Password);
            if (!result.Success || result.RawToken == null || result.RawCsrfToken == null || result.Session == null)
            {
                return Results.Unauthorized();
            }

            SetAdminCookies(context, result.RawToken, result.RawCsrfToken, result.Session.ExpiresAt);
            return Results.Ok(ToAdminMe(result.Session));
        }).AllowAnonymous().RequireRateLimiting("break-glass-login");

        auth.MapGet("/me", (HttpContext context) =>
        {
            var expires = DateTime.TryParse(context.User.FindFirstValue("selma_session_expires"), out var parsed)
                ? parsed
                : DateTime.UtcNow;
            return Results.Ok(new AdminMeDto(
                context.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                context.User.FindFirstValue("selma_admin_auth_method") ?? "oidc",
                expires));
        }).RequireAuthorization(AdminPolicy());

        auth.MapPost("/logout", async (HttpContext context, IAuthService authService) =>
        {
            if (context.Request.Cookies.TryGetValue(AuthSchemes.AdminSessionCookie, out var token))
            {
                await authService.RevokeAdminSessionAsync(token);
            }

            DeleteAdminCookies(context);
            return Results.NoContent();
        }).RequireAuthorization(AdminPolicy());
    }

    internal static AuthorizationPolicy AdminPolicy() => new AuthorizationPolicyBuilder(AuthSchemes.AdminSession)
        .RequireAuthenticatedUser()
        .RequireClaim("selma_platform_admin", "true")
        .Build();

    internal static Guid? AccountId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId) ? accountId : null;

    internal static string SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') ||
            returnUrl.StartsWith("//", StringComparison.Ordinal) || returnUrl.StartsWith("/\\", StringComparison.Ordinal) ||
            returnUrl.Contains('\\') || returnUrl.Contains('\r') || returnUrl.Contains('\n') ||
            !Uri.TryCreate(returnUrl, UriKind.Relative, out _))
        {
            return "/";
        }

        return returnUrl;
    }

    internal static ExternalLoginInfo? ToExternalLogin(ClaimsPrincipal? principal, AuthenticationProperties? properties)
    {
        if (principal == null)
        {
            return null;
        }

        var issuer = properties?.Items.TryGetValue(AuthSchemes.OidcIssuerProperty, out var validatedIssuer) == true
            ? validatedIssuer ?? string.Empty
            : principal.FindFirstValue("iss") ?? string.Empty;
        var email = principal.FindFirstValue("email") ?? principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var verifiedValue = principal.FindFirstValue("email_verified");
        var groups = ParseGroups(principal.FindAll("groups").Select(x => x.Value));
        return new ExternalLoginInfo(
            issuer,
            principal.FindFirstValue("sub") ?? string.Empty,
            email,
            principal.FindFirstValue("name") ?? principal.FindFirstValue(ClaimTypes.Name) ?? email,
            bool.TryParse(verifiedValue, out var verified) && verified,
            groups);
    }

    private static IReadOnlyCollection<string> ParseGroups(IEnumerable<string> values)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            if (value.StartsWith('['))
            {
                try
                {
                    foreach (var group in JsonSerializer.Deserialize<string[]>(value) ?? [])
                    {
                        result.Add(group);
                    }
                    continue;
                }
                catch (JsonException)
                {
                    // Treat a malformed array claim as a single non-matching value.
                }
            }
            result.Add(value);
        }
        return result;
    }

    private static AdminMeDto ToAdminMe(CoParenting.Core.Entities.PlatformAdminSession session) =>
        new(session.Email, session.DisplayName, session.AuthenticationMethod, session.ExpiresAt);

    internal static void SetAccountCookies(HttpContext context, string token, string csrf, DateTime expiresAt)
    {
        AppendSessionCookie(context, AuthSchemes.AccountSessionCookie, token, expiresAt);
        AppendCsrfCookie(context, AuthSchemes.AccountCsrfCookie, csrf, expiresAt);
    }

    private static void SetAdminCookies(HttpContext context, string token, string csrf, DateTime expiresAt)
    {
        AppendSessionCookie(context, AuthSchemes.AdminSessionCookie, token, expiresAt);
        AppendCsrfCookie(context, AuthSchemes.AdminCsrfCookie, csrf, expiresAt);
    }

    private static void AppendSessionCookie(HttpContext context, string name, string value, DateTime expiresAt) =>
        context.Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = new DateTimeOffset(expiresAt)
        });

    private static void AppendCsrfCookie(HttpContext context, string name, string value, DateTime expiresAt) =>
        context.Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = new DateTimeOffset(expiresAt)
        });

    private static void DeleteAccountCookies(HttpContext context)
    {
        DeleteCookie(context, AuthSchemes.AccountSessionCookie, true);
        DeleteCookie(context, AuthSchemes.AccountCsrfCookie, false);
    }

    private static void DeleteAdminCookies(HttpContext context)
    {
        DeleteCookie(context, AuthSchemes.AdminSessionCookie, true);
        DeleteCookie(context, AuthSchemes.AdminCsrfCookie, false);
    }

    private static void DeleteCookie(HttpContext context, string name, bool httpOnly) =>
        context.Response.Cookies.Delete(name, new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = true,
            SameSite = httpOnly ? SameSiteMode.Lax : SameSiteMode.Strict,
            Path = "/"
        });
}
