# kartverket-obstacle
Applikasjon for rapportering og validering av hindring i luftrommet. Piloter kan sende inn rapporter, og registerfører hos NRL kan validere og godkjenne eller avslå dem. Formålet er å øke flysikkerheten ved å ha et oppdatert register over hindringer.

Bygget med ASP.NET Core MVC, Entity framwork core, MySQL/MariaDB, og kjørbart i Docker.

## Arkitekturmodell
Applikasjonen følger (Model-View-Controller) mønsteret:
**Model**
Domeneklasser og validering (Advice, ObstacleData, errorViewModel).
**View**
Razor Views for presentasjon (skjema for hindringer, feedback, oversiktssider).
**Controller**
Forretningslogikk og ruting (HomeController, AdviceController, ObstacleController, FormController.
**Database**
MySQL/MariaDB, håndtert via Entity Framwork Core (ApplicationDBContext + migrasjoner).

### Dataflyt
1. Bruker åpner nettsiden (Home -->Index).
2. Piloter kan sende inn hindringsdata (ObstacleController --> Dataform).
   -Skjema valideres (modellattributter som Required, Range).
   -Gyldige data sendes til (Overview-siden).
3. Tilbakemeldinger sendes via adviceController --> FeedbackForm, og lagres i database ( Feedback-tabellen).
4. Registerfører kan validere og behandle innkomne rapporter (fremtidig utvidelse).

##Drift
Applikasjonen kjøres i Docker for enkel drift og portabilitet.
Docker-compose kan brukes til å starte både web-applikasjon og database i egne containere.
-**Web-applikasjon** kjører på'mcr.microsoft.com/dotnet/aspnet:9.0'
-**Database** kjører i 'mariadb'
-Kommunikasjon skjer via Docker-nettverket






