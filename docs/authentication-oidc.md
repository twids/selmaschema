# Widsell ID, OIDC och inbjudningar

Selma använder `id.widsell.nu` som en generell OpenID Connect-provider. Selma känner inte till Authentik-specifika användare, grupper eller sociala leverantörer. Authentik autentiserar identiteten; Selma äger lokala användare, roller, inbjudningar och sessioner.

## Authentik

1. Skapa en OAuth2/OIDC provider och en application för Selma i Authentik.
2. Använd Authorization Code-flödet. Selma skickar PKCE med `S256` och begär endast `openid profile email`.
3. Registrera exakt redirect URI `https://selma.widsell.nu/signin-oidc`. Använd inte wildcard eller regex för denna klient.
4. Sätt provider/application-sluggen och använd dess issuer/discovery-URL som `OIDC_AUTHORITY`, normalt `https://id.widsell.nu/application/o/<slug>/`.
5. Säkerställ att ID-token eller user-info innehåller stabila `sub`, `email`, `name` och `email_verified`.
6. `email_verified` måste vara boolean `true` och bygga på en faktiskt verifierad adress. Authentik-dokumentationen anger att claimen som standard är `false` från version 2025.10; använd därför en property mapping som läser ett verifieringsattribut eller annan betrodd källstatus, inte en mapping som blint returnerar `true`.
7. Skapa Google, Facebook och andra alternativ som Sources i Authentik. Lägg dem i identification stage under `Flows and Stages` → standard authentication flow → identification stage → `Selected sources`. Alternativen visas då hos Widsell ID, inte i Selmas kod.

Referenser: [OAuth2/OIDC provider](https://docs.goauthentik.io/add-secure-apps/providers/oauth2/), [federerade identitetsleverantörer](https://docs.goauthentik.io/users-sources/sources/social-logins/) och [Sources i login-flödet](https://docs.goauthentik.io/users-sources/sources/index.html#add-sources-to-default-login-page).

## Produktionskonfiguration

GitHub Actions bygger, testar och publicerar applikationsimagen till
`ghcr.io/twids/selmaschema/coparenting-app:latest` efter varje lyckad push till
`main`. Produktions-Compose använder denna flytande tagg och märker endast
applikationscontainern med `dockhand.update=true`. PostgreSQL är märkt med
`dockhand.update=false` och ska uppdateras separat efter backup och kontroll av
release notes.

Aktivera `Settings` → `Environments` → miljön → `Updates` →
`Enable scheduled update check` och `Automatically update containers` i
Dockhand. När digesten bakom `:latest` ändras hämtar Dockhand imagen och
återskapar applikationscontainern. `pull_policy: always` gör även manuella
Compose-deployer deterministiska. Uppdateringen innebär ett kort driftstopp.

Applikationen kör EF Core-migrationer innan HTTP-servern startar genom
`Database__ApplyMigrations=true`. Detta upplägg förutsätter en enda
applikationsinstans, vilket är Selmas nuvarande produktionsmodell. Om Selma
senare skalas horisontellt ska migrationerna flyttas till ett separat,
koordinerat deploysteg.

Dockhand ska tillföra följande konfiguration vid start:

- `OIDC_AUTHORITY`
- `OIDC_CLIENT_ID`
- `LOCAL_ADMIN_ENABLED` (`true` endast när reservvägen ska vara aktiv)
- `FORWARDED_HEADERS_KNOWN_NETWORK` (det betrodda proxy-/Docker-subnätet, i CIDR-form)

Följande värden är hemligheter och ska lagras i driftplattformens
hemlighetshantering, aldrig som värden i Git, Compose eller `appsettings`:

- `OIDC_CLIENT_SECRET`
- `LOCAL_ADMIN_PASSWORD_HASH` (BCrypt, separat från alla OIDC-konton)
- `POSTGRES_PASSWORD`

Callback-sökvägen är `/signin-oidc`. `X-Forwarded-Proto` och `X-Forwarded-For` accepteras bara från konfigurerade proxyadresser/nät; nginx måste skicka `X-Forwarded-Proto https`. Kontrollera efter deploy att redirect-parametern till Authentik är exakt `https://selma.widsell.nu/signin-oidc`.

## Säkerhets- och acceptanskontroll

- Kontrollera efter uppdatering att appcontainern är frisk och att startup-loggen visar en lyckad EF-migration. OIDC-migrationen tar bort gamla magic-link-rader och alla gamla sessioner men behåller användare och kalenderhistorik.
- Kontrollera att reservadmin kan logga in och skapa den första OIDC-admininbjudan.
- Kontrollera att ParentA/ParentB kan skapa föräldrainbjudningar men inte admininbjudningar.
- Kontrollera att Google/Facebook visas på Widsell ID-sidan.
- Lös in en inbjudan med annan verifierad adress än e-postledtråden och kontrollera att båda visas före den uttryckliga bekräftelsen.
- Försök använda samma länk igen; den ska nekas.
- Kontrollera att `__Host-selma-session` har `Secure`, `HttpOnly`, `SameSite=Lax`, `Path=/`, saknar `Domain` och har högst 30 dagars absolut giltighet.
- Kontrollera att utloggning återkallar Selma-sessionen utan att logga ut hela Widsell ID-sessionen.
