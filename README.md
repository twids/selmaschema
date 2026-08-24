# Selma

Selma är en flerfamiljstjänst för boendescheman. React/TypeScript-gränssnittet levereras av ett ASP.NET Core 8-API och data lagras i PostgreSQL 16.

## Selma v2

- Ett konto kan tillhöra flera familjer som Owner, Editor eller Viewer.
- Varje familj har egna barn, kalendrar, Hem A/Hem B-etiketter, tidszon och detaljnivå.
- Scheman är versionshanterade och stödjer Varannan vecka, 2-2-3, 2-2-5-5, 3-4-4-3 och Primärt boende + varannan helg.
- Inbjudningar innehåller både engångslänk och kort engångskod.
- Vanlig inloggning och `/admin` använder separata Authentik-klienter, sessioner och cookies.
- Plattformsadmin styrs av Authentik-gruppen `selma-platform-admins`; supportåtgärder kräver orsak och auditeras.

## Teknik och struktur

- `frontend/`: React 18, TypeScript, Vite, Material UI och Vitest.
- `backend/CoParenting.Core/`: domänmodellen.
- `backend/CoParenting.Application/`: DTO:er, tjänster och schemamotor.
- `backend/CoParenting.Infrastructure/`: EF Core, PostgreSQL och migrationer.
- `backend/CoParenting.API/`: Minimal API, OIDC, sessioner, CSRF och SPA-leverans.
- `backend/CoParenting.Tests/`: enhets- och integrationstester.
- `e2e-tests/`: sekventiella Playwright-scenarier.

## Lokal utveckling

Förutsättningar är .NET 8, Node 20, PostgreSQL 16 och Docker Compose V2.

```powershell
dotnet restore selmaschema.sln
Push-Location frontend; npm ci; Pop-Location
Push-Location e2e-tests; npm ci; Pop-Location
```

API och frontend kan köras separat:

```powershell
dotnet run --project backend/CoParenting.API/CoParenting.API.csproj
Push-Location frontend; npm run dev; Pop-Location
```

Eller via Compose efter att nödvändiga OIDC- och databasvariabler har satts:

```powershell
docker compose up --build
```

En tom databas migreras automatiskt. Om en äldre EF-databas innehåller Selma v1-data stoppar appen den destruktiva v2-migrationen tills `ALLOW_DESTRUCTIVE_V2_RESET=true` uttryckligen sätts. Använd aldrig flaggan i produktion innan Dockhand pausats, den slutliga körningen bekräftats och antingen en backup verifierats eller ägaren uttryckligen dokumenterat att all befintlig data får förloras.

## Verifiering

```powershell
dotnet test selmaschema.sln --configuration Release
Push-Location frontend; npm run test:run; npm run lint; npm run build; Pop-Location
docker compose config
docker compose -f docker-compose.yml -f docker-compose.prod.yml config
docker compose -f docker-compose.e2e.yml config
```

E2E körs mot den isolerade E2E-stacken och får aldrig riktas mot delad data eller produktion. Se [e2e-tests/README.md](e2e-tests/README.md).

## Autentisering och drift

Authentik-konfiguration, claims, callbacks och hemligheter beskrivs i [docs/authentication-oidc.md](docs/authentication-oidc.md). Den kontrollerade v2-lanseringen och rollbackförfarandet finns i [docs/selma-v2-rollout.md](docs/selma-v2-rollout.md).

Produktionsimagen publiceras till `ghcr.io/twids/selmaschema/coparenting-app`. Dockhand får uppdatera appcontainern automatiskt efter den kontrollerade v2-lanseringen, men aldrig PostgreSQL-containern.
