# kartverket-obstacle
Applikasjon for rapportering og validering av hindring i luftrommet. Piloter kan sende inn rapporter, og registerfører hos NRL kan validere og godkjenne eller avslå dem. Formålet er å øke flysikkerheten ved å ha et oppdatert register over hindringer.

Bygget med ASP.NET Core MVC, Entity framwork core, MySQL/MariaDB, og kjørbart i Docker.

## Arkitekturmodell
Applikasjonen følger (Model-View-Controller) mønsteret:
**Model**
Domeneklasser og validering (Advice, ObstacleData, errorViewModel).
**View**
Razor Views for presentasjon (skjema for hindringer, feedback, oversiktssider, kart).
**Controller**
Forretningslogikk og ruting (HomeController, AdviceController, ObstacleController, FormController, MapController).
**Database**
MySQL/MariaDB, håndtert via Entity Framwork Core (ApplicationDBContext + migrasjoner).

### Dataflyt
1. Bruker åpner nettsiden (Home -->Index).
2. Piloter kan sende inn hindringsdata (ObstacleController --> Dataform).
   -Skjema valideres (modellattributter som Required, Range).
   -Gyldige data sendes til (Overview-siden).
3. Tilbakemeldinger sendes via adviceController --> FeedbackForm, og lagres i database ( Feedback-tabellen).
4. Registerfører kan validere og behandle innkomne rapporter (fremtidig utvidelse).

### Drift 
Applikasjonen kjøres i Docker for enkel drift og portabilitet.
Docker-compose kan brukes til å starte både web-applikasjon og database i egne containere.
-**Web-applikasjon** kjører på'mcr.microsoft.com/dotnet/aspnet:9.0'
-**Database** kjører i 'mariadb'
-Kommunikasjon skjer via Docker-nettverket

## Kjøre applikasjonen på Mac

### Forutsetninger

Før du kan kjøre applikasjonen, må du ha installert:

1. **.NET 9.0 SDK**
   - Last ned fra: https://dotnet.microsoft.com/download/dotnet/9.0
   - Eller installer via Homebrew:
     ```bash
     brew install --cask dotnet-sdk
     ```
   - Verifiser installasjonen:
     ```bash
     dotnet --version
     ```

2. **Docker Desktop for Mac**
   - Last ned fra: https://www.docker.com/products/docker-desktop
   - Start Docker Desktop og sørg for at den kjører

3. **Git** (hvis du ikke allerede har det)
   - Installer via Homebrew:
     ```bash
     brew install git
     ```

### Metode 1: Kjøre med Docker (Anbefalt)

Dette er den enkleste metoden og krever minimal konfigurasjon.

1. **Klon prosjektet** (hvis du ikke allerede har gjort det):
   ```bash
   git clone <repository-url>
   cd kartverket-obstacle
   ```

2. **Bygg og start containere**:
   ```bash
   docker-compose up --build
   ```

3. **Åpne applikasjonen i nettleseren**:
   - Applikasjonen vil være tilgjengelig på: http://localhost:8080
   - Database er tilgjengelig på port 3307

4. **Stopp applikasjonen**:
   - Trykk `Ctrl+C` i terminalen
   - Eller kjør: `docker-compose down`

**Standard innloggingsinformasjon:**
- Email: `admin@kartverket.com`
- Passord: `Admin123!`

### Metode 2: Kjøre direkte med .NET (Uten Docker)

Hvis du foretrekker å kjøre applikasjonen direkte uten Docker:

1. **Installer MariaDB lokalt**:
   ```bash
   brew install mariadb
   brew services start mariadb
   ```

2. **Opprett database**:
   ```bash
   mysql -u root -p
   ```
   I MySQL-konsollen:
   ```sql
   CREATE DATABASE KartverketDB;
   EXIT;
   ```

3. **Oppdater connection string** i `FirstWebApplication/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DatabaseConnection": "Server=localhost;Port=3306;Database=KartverketDB;User=root;Password=ditt_passord"
     }
   }
   ```

4. **Kjør database-migrasjoner**:
   ```bash
   cd FirstWebApplication
   dotnet ef database update
   ```

5. **Start applikasjonen**:
   ```bash
   dotnet run
   ```

6. **Åpne applikasjonen i nettleseren**:
   - Applikasjonen vil være tilgjengelig på: http://localhost:5193 eller https://localhost:7145

### Troubleshooting

**Problem: Docker Desktop kjører ikke**
- Sørg for at Docker Desktop er startet og kjører
- Sjekk at Docker er tilgjengelig: `docker --version`

**Problem: Port allerede i bruk**
- Hvis port 8080 er opptatt, endre port i `docker-compose.yml`:
  ```yaml
  ports:
    - "8081:8080"  # Endre 8080 til 8081
  ```

**Problem: Database-tilkobling feiler**
- Sjekk at MariaDB-containeren kjører: `docker ps`
- Sjekk connection string i `docker-compose.yml` og `appsettings.json`
- Vent noen sekunder etter at containeren starter (database trenger tid på å initialisere)

**Problem: Migrasjoner feiler**
- Sørg for at databasen eksisterer
- Sjekk at connection string er korrekt
- Prøv å kjøre migrasjoner manuelt:
  ```bash
  cd FirstWebApplication
  dotnet ef database update
  ```

**Problem: .NET SDK ikke funnet**
- Verifiser at .NET 9.0 SDK er installert: `dotnet --version`
- Sørg for at PATH er konfigurert riktig
- Prøv å starte terminalen på nytt etter installasjon

### Utviklingsmiljø

For utvikling anbefales det å bruke:
- **Visual Studio Code** med C#-utvidelsen
- **Rider** (JetBrains)
- **Visual Studio for Mac** (hvis tilgjengelig)

**Anbefalte VS Code-utvidelser:**
- C# (Microsoft)
- C# Dev Kit (Microsoft)
- Docker (Microsoft) 

[![.NET Tests](https://github.com/JabirMAG/kartverket-obstacle/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JabirMAG/kartverket-obstacle/actions/workflows/dotnet.yml)

## Testing

Applikasjonen har omfattende testing gjennomført på flere nivåer:

### 1. Enhetstesting (Unit Testing)

**Status:** ✅ Fullstendig implementert

- **Omfang:** 40+ testfiler med 284+ testmetoder
- **Dekning:**
  - Controllers (9 filer) - alle controller-aksjoner er testet
  - Repositories (5 filer) - alle CRUD-operasjoner er testet
  - Models (18 filer) - validering og datamodeller er testet
  - ViewModels (13 filer) - alle viewmodeller er testet
- **Verktøy:** XUnit, Moq, Entity Framework Core InMemory Database
- **Dokumentasjon:** Se `docs/testing/testing-scenarier-resultater.md`

**Eksempler på testet funksjonalitet:**
- Validering av input-data
- Controller-aksjoner (GET, POST)
- Repository-operasjoner (Create, Read, Update, Delete)
- Autentisering og autorisasjon
- Feilhåndtering

### 2. Systemstesting (System/Integration Testing)

**Status:** ✅ Implementert

- **Integrasjonstester:** Tester hele flyter fra Controller → Repository → Database
- **Testfiler:**
  - `Kartverket.Tests/Integration/ObstacleFlowIntegrationTest.cs`
    - Tester end-to-end flyt for opprettelse av hindringer
    - Tester oppdatering av hindringer gjennom hele systemet
    - Tester opprettelse av rapporter sammen med hindringer
- **Metode:** Bruker InMemory-database for isolerte integrasjonstester

**Testet flyter:**
- Opprettelse av hindring → Lagring i database → Verifisering
- Oppdatering av hindring → Endringer i database → Verifisering
- Opprettelse av hindring med rapport → Begge lagres → Verifisering

### 3. Sikkerhetstesting (Security Testing)

**Status:** ✅ Implementert

- **Testfil:** `Kartverket.Tests/Security/SecurityTest.cs`
- **Testet sikkerhetsaspekter:**

**Autentisering og autorisasjon:**
- ✅ AdminController krever Admin-rolle
- ✅ PilotController krever Pilot-rolle
- ✅ RegistrarController krever Admin eller Registerfører-rolle
- ✅ VarslingController krever autentisering
- ✅ Uautorisert tilgang blir blokkert

**CSRF-beskyttelse:**
- ✅ Alle POST-aksjoner har `ValidateAntiForgeryToken`-attributt
- ✅ Verifisert for alle controllers med POST-metoder

**Input-validering:**
- ✅ SQL injection-forsøk håndteres (Entity Framework parameteriserer queries)
- ✅ XSS-forsøk håndteres (Razor views encoderer output automatisk)
- ✅ Passordpolicy håndheves (sterke passordkrav)

**Sikkerhetsfunksjoner:**
- AutoValidateAntiforgeryToken for alle POST-forespørsler (konfigurert i Program.cs)
- Security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, etc.)
- HTTPS-redirect i produksjon

### 4. Brukervennlighetstesting (Usability Testing)

**Status:** ✅ Dokumentert

Brukervennlighetstesting er gjennomført gjennom manuell testing av applikasjonen. Følgende aspekter er testet:

**Navigasjon og brukergrensesnitt:**
- ✅ Intuitiv navigasjon mellom sider
- ✅ Tydelige knapper og lenker
- ✅ Responsivt design for ulike skjermstørrelser
- ✅ Konsistent layout og styling

**Skjema og input:**
- ✅ Tydelige feltetiketter og hjelpetekster
- ✅ Valideringsmeldinger vises tydelig
- ✅ Passordkrav vises under passordfeltet
- ✅ Feilmeldinger er forståelige og på norsk

**Brukerroller og tilgang:**
- ✅ Piloter kan enkelt sende inn hindringer
- ✅ Registerfører kan enkelt behandle rapporter
- ✅ Administrator kan enkelt administrere brukere
- ✅ Roller har tydelige og relevante funksjoner

**Feedback og bekreftelse:**
- ✅ Suksessmeldinger ved vellykkede operasjoner
- ✅ Bekreftelsessider etter registrering
- ✅ Tydelige feilmeldinger ved problemer
- ✅ Loading-indikatorer ved asynkrone operasjoner

**Tilgjengelighet:**
- ✅ Tydelige kontraster for lesbarhet
- ✅ Logisk tab-rekkefølge i skjemaer
- ✅ Tydelige feilmeldinger for skjermlesere
- ✅ Responsivt design for mobile enheter

**Testscenarier som er gjennomført:**
1. Ny bruker registrerer seg og venter på godkjenning
2. Godkjent bruker logger inn og navigerer til sin rolle-spesifikke side
3. Pilot sender inn hindring med alle felter utfylt
4. Pilot sender inn hindring med hurtiglagring (kun geometri)
5. Registerfører godkjenner/avviser hindringer
6. Administrator administrerer brukere og roller
7. Bruker sender tilbakemelding via feedback-skjema
8. Bruker resetter passord

**Resultat:** Applikasjonen er brukervennlig med tydelig navigasjon, forståelige meldinger og logisk flyt for alle brukerroller.

### Kjøre tester

For å kjøre alle tester:
```bash
dotnet test
```

For å kjøre spesifikke testkategorier:
```bash
# Enhetstester
dotnet test --filter "FullyQualifiedName~Controllers"

# Integrasjonstester
dotnet test --filter "FullyQualifiedName~Integration"

# Sikkerhetstester
dotnet test --filter "FullyQualifiedName~Security"
```

### Testdekning

- **Controllers:** 100% av alle controllers har tester
- **Repositories:** 100% av alle repositories har tester
- **Models:** 100% av alle modeller har tester
- **Sikkerhet:** Alle kritiske sikkerhetsaspekter er testet
- **Integrasjon:** Kritiske flyter er testet end-to-end
