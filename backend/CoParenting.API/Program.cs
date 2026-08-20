using CoParenting.API.Authentication;
using CoParenting.API.Endpoints;
using CoParenting.Application.Interfaces;
using CoParenting.Application.Services;
using CoParenting.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Co-Parenting Calendar API", Version = "v1" });
});

// Add DbContext
builder.Services.AddDbContext<CoParentingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddScoped<IDayAssignmentService, DayAssignmentService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChangeRequestService, ChangeRequestService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = AuthSchemes.Session;
        options.DefaultChallengeScheme = AuthSchemes.Session;
    })
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(AuthSchemes.Session, null)
    .AddCookie(AuthSchemes.OidcTemporary, options =>
    {
        options.Cookie.Name = AuthSchemes.OidcCookie;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        options.SlidingExpiration = false;
    })
    .AddOpenIdConnect(AuthSchemes.Oidc, options =>
    {
        var oidc = builder.Configuration.GetSection("Oidc");
        options.Authority = oidc["Authority"];
        options.ClientId = oidc["ClientId"];
        options.ClientSecret = oidc["ClientSecret"];
        options.CallbackPath = oidc["CallbackPath"] ?? "/signin-oidc";
        options.SignInScheme = AuthSchemes.OidcTemporary;
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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name"
        };
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
                context.Response.Redirect("/login?error=oidc_failed");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireParentRole", policy => policy.RequireRole("ParentA", "ParentB"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("local-admin-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    foreach (var key in new[] { "Oidc:Authority", "Oidc:ClientId", "Oidc:ClientSecret" })
    {
        if (string.IsNullOrWhiteSpace(app.Configuration[key]))
        {
            throw new InvalidOperationException($"Required OIDC configuration is missing: {key}");
        }
    }

    if (app.Configuration.GetValue<bool>("Auth:LocalAdmin:Enabled") &&
        string.IsNullOrWhiteSpace(app.Configuration["Auth:LocalAdmin:PasswordHash"]))
    {
        throw new InvalidOperationException("Local admin is enabled but its password hash is missing");
    }
}

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseCors("AllowFrontend");

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints first
app.MapAuthEndpoints();
app.MapInvitationEndpoints();
app.MapAdminEndpoints();
app.MapDayAssignmentEndpoints();
app.MapCommentEndpoints();
app.MapConfigurationEndpoints();
app.MapStatisticsEndpoints();
app.MapChangeRequestEndpoints();

// Serve static files from wwwroot (frontend build)
app.UseDefaultFiles();
app.UseStaticFiles();

// Fallback to index.html for client-side routing (SPA)
// This should come after API endpoints to avoid catching API routes
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for testing
public partial class Program { }
