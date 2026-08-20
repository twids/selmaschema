using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CoParenting.API.Authentication;
using CoParenting.API.Endpoints;
using CoParenting.Application.Interfaces;
using CoParenting.Application.Services;
using CoParenting.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v2", new() { Title = "Selma API", Version = "v2" }));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<CoParentingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IAdminService, AdminService>();

var authentication = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = AuthSchemes.AccountSession;
    options.DefaultChallengeScheme = AuthSchemes.AccountSession;
});
authentication.AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(AuthSchemes.AccountSession, null);
authentication.AddScheme<AuthenticationSchemeOptions, AdminSessionAuthenticationHandler>(AuthSchemes.AdminSession, null);
authentication.AddCookie(AuthSchemes.OidcTemporary, options => ConfigureTemporaryCookie(options, AuthSchemes.OidcCookie));
authentication.AddCookie(AuthSchemes.AdminOidcTemporary, options => ConfigureTemporaryCookie(options, AuthSchemes.AdminOidcCookie));
authentication.AddOpenIdConnect(AuthSchemes.Oidc, options =>
    ConfigureOidc(options, builder.Configuration.GetSection("Oidc"), AuthSchemes.OidcTemporary, "/signin-oidc", "/login?error=oidc_failed", false));
authentication.AddOpenIdConnect(AuthSchemes.AdminOidc, options =>
    ConfigureOidc(options, builder.Configuration.GetSection("AdminOidc"), AuthSchemes.AdminOidcTemporary, "/signin-oidc-admin", "/admin/login?error=oidc_failed", true));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdmin", policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.AdminSession);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("selma_platform_admin", "true");
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("oidc-login", context => FixedWindow(context, 20, TimeSpan.FromMinutes(1)));
    options.AddPolicy("join-code", context => FixedWindow(context, 10, TimeSpan.FromMinutes(5)));
    options.AddPolicy("break-glass-login", context => FixedWindow(context, 5, TimeSpan.FromMinutes(5)));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
    {
        if (IPAddress.TryParse(proxy, out var address))
        {
            options.KnownProxies.Add(address);
        }
    }

    foreach (var network in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [])
    {
        var parts = network.Split('/', 2);
        if (parts.Length == 2 && IPAddress.TryParse(parts[0], out var prefix) && int.TryParse(parts[1], out var length))
        {
            options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, length));
        }
    }
});

builder.Services.AddCors(options => options.AddPolicy("AllowFrontend", policy => policy
    .WithOrigins("http://localhost:3000", "http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    ValidateRequiredConfiguration(app.Configuration, "Oidc", "Authority", "ClientId", "ClientSecret");
    ValidateRequiredConfiguration(app.Configuration, "AdminOidc", "Authority", "ClientId", "ClientSecret", "RequiredGroup");
    if (app.Configuration.GetValue<bool>("Auth:BreakGlass:Enabled") &&
        string.IsNullOrWhiteSpace(app.Configuration["Auth:BreakGlass:PasswordHash"]))
    {
        throw new InvalidOperationException("Break-glass admin is enabled but its password hash is missing");
    }
}

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
    var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
    var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
    var hasDestructiveV2Reset = pending.Any(x => x.Contains("SelmaV2", StringComparison.OrdinalIgnoreCase));
    var hasExistingEfSchema = applied.Count > 0;
    if (hasDestructiveV2Reset && hasExistingEfSchema &&
        !app.Configuration.GetValue<bool>("Database:AllowDestructiveV2Reset"))
    {
        throw new InvalidOperationException(
            "The destructive Selma v2 migration is pending. Pause Dockhand, verify a database backup, then explicitly set Database:AllowDestructiveV2Reset=true for the controlled rollout.");
    }

    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = exception switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
        DbUpdateException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
    await context.Response.WriteAsJsonAsync(new
    {
        error = exception is ArgumentException ? exception.Message : "Request could not be completed"
    });
}));

app.UseForwardedHeaders();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CsrfProtectionMiddleware>();

app.MapAuthEndpoints();
app.MapAdminAuthEndpoints();
app.MapFamilyEndpoints();
app.MapPlatformAdminEndpoints();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

static void ConfigureTemporaryCookie(CookieAuthenticationOptions options, string name)
{
    options.Cookie.Name = name;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = false;
}

static void ConfigureOidc(
    OpenIdConnectOptions options,
    IConfigurationSection configuration,
    string signInScheme,
    string defaultCallback,
    string failureRedirect,
    bool includeGroups)
{
    options.Authority = configuration["Authority"];
    options.ClientId = configuration["ClientId"];
    options.ClientSecret = configuration["ClientSecret"];
    options.CallbackPath = configuration["CallbackPath"] ?? defaultCallback;
    options.SignInScheme = signInScheme;
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = false;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.MapInboundClaims = false;
    options.RequireHttpsMetadata = true;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.ClaimActions.MapUniqueJsonKey("email", "email");
    options.ClaimActions.MapUniqueJsonKey("name", "name");
    options.ClaimActions.MapUniqueJsonKey("email_verified", "email_verified");
    if (includeGroups)
    {
        options.ClaimActions.MapJsonKey("groups", "groups");
    }
    options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            var issuer = context.SecurityToken?.Issuer;
            if (!string.IsNullOrWhiteSpace(issuer) && context.Properties != null)
            {
                context.Properties.Items[AuthSchemes.OidcIssuerProperty] = issuer;
            }

            var verified = context.Principal?.FindFirstValue("email_verified");
            if (!bool.TryParse(verified, out var isVerified) || !isVerified)
            {
                context.Fail("A verified email address is required");
            }
            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            context.Response.Redirect(failureRedirect);
            return Task.CompletedTask;
        }
    };
}

static RateLimitPartition<string> FixedWindow(HttpContext context, int permits, TimeSpan window) =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permits,
            Window = window,
            QueueLimit = 0,
            AutoReplenishment = true
        });

static void ValidateRequiredConfiguration(IConfiguration configuration, string section, params string[] keys)
{
    foreach (var key in keys)
    {
        if (string.IsNullOrWhiteSpace(configuration[$"{section}:{key}"]))
        {
            throw new InvalidOperationException($"Required configuration is missing: {section}:{key}");
        }
    }
}

public partial class Program;
