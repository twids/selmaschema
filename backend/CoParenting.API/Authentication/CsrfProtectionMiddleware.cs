using CoParenting.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CoParenting.API.Authentication;

public sealed class CsrfProtectionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method) ||
            HttpMethods.IsOptions(context.Request.Method) || context.User.Identity?.IsAuthenticated != true ||
            context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await next(context);
            return;
        }

        var isAdmin = context.User.HasClaim("selma_platform_admin", "true");
        var sessionCookie = isAdmin ? AuthSchemes.AdminSessionCookie : AuthSchemes.AccountSessionCookie;
        var csrfCookie = isAdmin ? AuthSchemes.AdminCsrfCookie : AuthSchemes.AccountCsrfCookie;
        var sessionToken = context.Request.Cookies[sessionCookie];
        var csrfToken = context.Request.Cookies[csrfCookie];
        var csrfHeader = context.Request.Headers["X-CSRF-TOKEN"].FirstOrDefault();
        var valid = !string.IsNullOrWhiteSpace(sessionToken) &&
                    !string.IsNullOrWhiteSpace(csrfToken) &&
                    string.Equals(csrfToken, csrfHeader, StringComparison.Ordinal) &&
                    (isAdmin
                        ? await authService.ValidateAdminCsrfAsync(sessionToken, csrfToken)
                        : await authService.ValidateAccountCsrfAsync(sessionToken, csrfToken));
        if (!valid)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid CSRF token" });
            return;
        }

        await next(context);
    }
}
