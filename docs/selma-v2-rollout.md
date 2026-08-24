# Selma v2 – kontrollerad produktionslansering

Migreringen `SelmaV2FamiliesAndPlatformAdministration` rensar avsiktligt all befintlig Selma-data: användare, externa identiteter, sessioner, kalenderhistorik, konfiguration, inbjudningar, kommentarer och bytesförfrågningar. Authentik-konton och deployhemligheter ligger utanför databasen och påverkas inte.

Applikationen vägrar köra migreringen om inte `Database__AllowDestructiveV2Reset=true`. Den spärren ersätter inte ett uttryckligt backup-/databorttagningsbeslut och en separat körbekräftelse.

För den initiala v2-lanseringen den 24 augusti 2026 har ägaren uttryckligen bedömt den befintliga Selma-databasen som förbrukningsbar och godkänt att allt innehåll förloras. Databasbackup och återläsningsprov utgår därför endast för denna körning. Beslutet ersätter inte den separata slutbekräftelsen om att starta migreringen och gäller inte framtida driftsättningar med riktig användardata.

## Före merge/deploy

1. Säkerställ att v2-PR:ens backend Release-svit, frontend `test:run`, `lint`, `build`, Compose-validering, migrationsscript och E2E är gröna.
2. Skapa Authentik-gruppen `selma-platform-admins` och lägg rätt administratör där.
3. Skapa separat adminprovider/application med MFA, callback `https://selma.widsell.nu/signin-oidc-admin`, scopes `openid profile email` och `groups`-claim.
4. Lägg admin-OIDC-värden och hemligheter i Dockhand enligt `authentication-oidc.md`.
5. Pausa Dockhands schemalagda automatiska uppdatering för Selmas appcontainer. Kontrollera i Dockhand att inga uppdateringar körs.
6. Verifiera att backupbeslutet ovan fortfarande gäller. Om databasen hunnit få värdefull data ska körningen stoppas tills backup och återläsningsprov har genomförts.
7. Notera aktuell image-digest och Compose-konfiguration.
8. Begär en uttrycklig slutlig körbekräftelse. Sätt inte reset-flaggan innan den har givits.

## Kontrollerad körning

1. Pinna först den nya imagen med digest i ett manuellt Dockhand-/Compose-steg.
2. Sätt tillfälligt `ALLOW_DESTRUCTIVE_V2_RESET=true` och återskapa appcontainern. Endast en appinstans får köra migreringen.
3. Följ startup-loggen tills EF-migreringen är klar och HTTP-servern har startat.
4. Sätt omedelbart `ALLOW_DESTRUCTIVE_V2_RESET=false` och återskapa containern en gång till. Flaggan ska inte ligga kvar.
5. Verifiera normal OIDC-login, onboardingens create/join, separat admin-OIDC, admincookie, forwarded HTTPS-callback och break-glass.
6. Genomför acceptansen: skapa familj, bjud in andra föräldern med länk och kod, lägg barn i kalender, skapa flera kalendrar, förhandsvisa/aktivera schema framåt och gör en auditerad supportåtgärd.
7. Aktivera Dockhands automatiska imageuppdatering igen när samtliga kontroller är godkända.

## Rollback

Stoppa appen och starta tidigare image med den noterade digesten mot en ny tom databas. Eftersom den initiala v2-lanseringen uttryckligen saknar backup kan den inte återställa tidigare Selma-data. Kör inte EF `Down` som produktionsrollback. För alla senare lanseringar med riktig användardata är en verifierad backup den auktoritativa återställningsvägen.
