# Widsell ID och plattformsadministration

Selma använder `id.widsell.nu` som en generell OpenID Connect-provider med Authorization Code och PKCE. Google, Facebook och andra inloggningssätt är Sources i Authentik och syns endast på Widsell ID-sidan. Normal inloggning accepterar varje identitet med verifierad e-post som Authentik släpper igenom och skapar automatiskt ett lokalt `Account`.

## Två separata OIDC-klienter

Skapa två OAuth2/OIDC providers/applications i Authentik:

| Användning | Callback | Lokal cookie | Giltighet |
|---|---|---|---|
| Vanliga Selma | `https://selma.widsell.nu/signin-oidc` | `__Host-selma-session` | 30 dagar absolut |
| Plattformsadmin | `https://selma.widsell.nu/signin-oidc-admin` | `__Host-selma-admin-session` | 1 timme absolut |

Båda klienterna använder Authorization Code, PKCE `S256` och scopes `openid profile email`. Redirect-URI:erna ska vara exakta utan wildcard eller regex. Claims ska innehålla stabila `sub`, `email`, `name` och boolean `email_verified=true`.

Adminprovidern ska dessutom:

1. Kräva MFA i Authentik.
2. Returnera standardclaimen `groups`.
3. Endast ge Selmas adminsession när claimen innehåller exakt `selma-platform-admins`.

Samma Authentik-identitet kan användas i båda flödena, men sessionsdatabaser, cookies, autentiseringsscheman, API-policyer och frontendskal är separata. Adminmedlemskap ger aldrig familjeåtkomst och familjemedlemskap ger aldrig adminåtkomst.

Authentik-referenser: [OAuth2/OIDC provider](https://docs.goauthentik.io/add-secure-apps/providers/oauth2/), [sociala Sources](https://docs.goauthentik.io/users-sources/sources/social-logins/) och [Sources i identification stage](https://docs.goauthentik.io/users-sources/sources/index.html#add-sources-to-default-login-page).

## Konfiguration

Dockhand tillför följande icke-hemliga värden:

- `OIDC_AUTHORITY`, `OIDC_CLIENT_ID`, `OIDC_CALLBACK_PATH=/signin-oidc`
- `ADMIN_OIDC_AUTHORITY`, `ADMIN_OIDC_CLIENT_ID`, `ADMIN_OIDC_CALLBACK_PATH=/signin-oidc-admin`
- `ADMIN_OIDC_REQUIRED_GROUP=selma-platform-admins`
- `BREAK_GLASS_ADMIN_ENABLED` (`true` endast när reservvägen ska vara tillgänglig)
- `FORWARDED_HEADERS_KNOWN_NETWORK`

Följande är hemligheter och får aldrig lagras i Git, Compose eller appsettings:

- `OIDC_CLIENT_SECRET`
- `ADMIN_OIDC_CLIENT_SECRET`
- `BREAK_GLASS_ADMIN_PASSWORD_HASH` (BCrypt och separat från Authentik-konton)
- `POSTGRES_PASSWORD`

Nginx måste skicka `X-Forwarded-Proto https`. Selma litar endast på konfigurerade proxyadresser/nät och applicerar forwarded headers före autentisering, så callback-URL:er genereras med `https`.

## Cookies och CSRF

Båda sessionsvärdena är slumpmässiga, lagras endast hashade och skickas som `Secure`, `HttpOnly`, `SameSite=Lax`, `Path=/` utan `Domain`. Varje session har ett separat, sessionsbundet CSRF-token i en läsbar `__Host-...-csrf`-cookie; alla autentiserade mutationer kräver samma värde i `X-CSRF-TOKEN`.

Utloggning återkallar endast aktuell Selma-session och rensar dess cookies. Den loggar inte ut hela Widsell ID-sessionen.

Break-glass finns endast på `/admin/login`, är rate-limitad, skapar aldrig ett vanligt `Account` eller familjemedlemskap och skriver en auditpost vid lyckad användning.
