# Selma v2 E2E

Playwright-sviten verifierar ett sammanhängande v2-flöde mot en isolerad PostgreSQL-databas:

- skapa en ny familj;
- lägga till barn och flera boendekalendrar;
- förhandsvisa och versionsaktivera ett framtida schema;
- bjuda in och ansluta en andra förälder med engångskod;
- logga in via separat break-glass-adminsession;
- utföra en orsakskrävande och auditerad supportåtgärd.

Testerna körs sekventiellt eftersom de delar databasstate. Testinloggningen är endast registrerad i ASP.NET-miljön `EndToEnd` och kräver `E2E_LOGIN_SECRET`.

## Lokalt

Installera beroenden och Chromium:

```powershell
Push-Location e2e-tests
npm ci
npx playwright install chromium
Pop-Location
```

Skapa en BCrypt-hash för ett lokalt testlösenord och sätt `BREAK_GLASS_ADMIN_PASSWORD_HASH`, `E2E_ADMIN_PASSWORD` och `E2E_LOGIN_SECRET`. Starta därefter den isolerade stacken:

```powershell
docker compose -f docker-compose.e2e.yml up --build -d
Push-Location e2e-tests; npm test; Pop-Location
docker compose -f docker-compose.e2e.yml down -v
```

Stacken sätter `Database__AllowDestructiveV2Reset=true` eftersom databasen är testunik och får raderas. Kör aldrig denna Compose-fil eller testsviten mot delad data eller produktion.

Vid fel finns skärmbilder, video och traces under `e2e-tests/test-results/`; de katalogerna är genererade och ska inte versionshanteras.
