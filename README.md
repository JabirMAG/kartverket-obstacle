# Semesterproject for Kartverket re. Aviation Obstacles Grp. 18

## Application Setup

The technologies used in this application are as follows:

- **Docker**: For building and running the application and the database.
- **MariaDB**: Relational database used for data storage.
- **ASP.NET Core MVC**: Application built with the .NET 9 MVC framework.

## How to Run the Application

Docker Compose fungerer likt på alle operativsystemer (Windows, Mac, Linux). Følg disse stegene for å starte applikasjonen:

1. **Installer Docker Desktop** på maskinen som skal kjøre applikasjonen.
2. **Klon repositoryet** til ditt lokale system.
3. **Naviger til prosjektet** i terminalen.
4. **Start database og applikasjon** ved å kjøre følgende kommandoer i prosjektmappen i terminalen:

```bash
docker compose down -v
docker compose build
docker compose up -d
```

5. **Åpne applikasjonen i nettleseren**:
   - Vent til containere er startet (sjekk i Docker Desktop at begge containere kjører).
   - I Docker Desktop, finn containeren `kartverket-obstacle-firstwebapplication-1`.
   - Klikk på porten `8080:8080` for å åpne applikasjonen i nettleseren.
   - Alternativt kan du åpne nettleseren og gå til `http://localhost:8080`.

## Hvordan logge inn

For å logge inn i applikasjonen, bruk følgende innloggingsinformasjon:

- **Brukernavn**: `Admin@kartverket.com`
- **Passord**: `Admin123!`

Dette er en forhåndsdefinert administratorbruker som har full tilgang til alle funksjoner i systemet.

## Opprette nye brukere

Den mest effektive måten å opprette en ny bruker på er gjennom administratorpanelet:

1. **Logg inn** som administrator med innloggingsinformasjonen over.
2. Klikk på **"Brukerhåndtering"** i navigasjonsmenyen.
3. Klikk på **"Opprett ny bruker"**.
4. Fyll ut skjemaet med brukerens informasjon.

**Hvorfor denne metoden?** Hvis du prøver å registrere en bruker utenfor administratorpanelet (via registreringsskjemaet), må personen uansett få godkjenning fra en administrator før de kan bruke systemet. Ved å opprette brukeren direkte gjennom administratorpanelet unngår du dette ekstra godkjenningssteget og sparer tid.

## Application Features

This is an obstacle reporting and validation system for airspace safety, with role-based workflows for pilots, registrars, and administrators.


### User Administration & Authentication

- User registration with approval workflow
- Login/Logout
- Password reset (forgot password)
- User approval/rejection by administrators
- Role-based access control (Admin, Registerfører, Pilot)

### User Management

- Create users
- View all users with roles and approval status
- Assign/remove roles (Pilot, Registerfører, Admin)
- Delete users
- View user details (email, organization, desired role)
- Default admin account protection

### Obstacle Reporting & Management

- Interactive map for obstacle reporting
- Quick save obstacle (geometry only)
- Full obstacle submission (name, height, description, geometry)
- View obstacle overview/details

### Obstacle Status Management

- Under treatment (1)
- Approved (2)
- Rejected (3) - automatically archived
- Automatic report generation when obstacles are created

### Pilot Features

- View own obstacles
- View obstacle details and associated reports
- Update obstacles (only when status is "Under treatment")
- Receive notifications about comments on obstacles

### Registrar Features

- View all obstacles and reports
- Update obstacle status
- Add comments/reports to obstacles
- View detailed obstacle information
- View archived reports (rejected obstacles)

### Admin Features

- Admin dashboard
- View all obstacle reports
- Update obstacle status
- Archive/reject obstacles
- Restore archived obstacles with new status
- Add comments to obstacles
- View archived reports
- User management (see User Administration above)

### Notifications System

- View notifications (comments on user's obstacles)
- Real-time notification count (unread)
- Filter out auto-generated comments
- Group notifications by obstacle

### Feedback System

- Submit feedback form
- Thank you confirmation page

### General Features

- Home page with time-based greeting
- Privacy page
- About us page
- Error handling
- Responsive design
- CSRF protection
- Security headers
- Session management

### Data Management

- Archive rejected obstacles
- Restore archived obstacles
- Organization-based user categorization
- GeoJSON geometry support for obstacles

## MVC

MVC (Model-View-Controller) separates an application into three parts:

- **Model**: Data and business logic
- **View**: User interface and presentation
- **Controller**: Handles requests, coordinates Model and View, and returns responses

There is a clear separation of concerns, easier maintenance, and better testability. In ASP.NET Core MVC, Controllers process HTTP requests, Models represent data entities, and Views are Razor templates that render HTML.

## Entity Framework

Entity Framework Core is an ORM that lets the application work with the MySQL/MariaDB database using C# objects instead of SQL. It maps models like `ObstacleData`, `RapportData`, and `ApplicationUser` to database tables and handles queries, updates, and migrations.

**Database communication**: The application uses `ApplicationDBContext` (configured in `Program.cs`) to interact with the database. EF translates LINQ queries from repositories (e.g., `ObstacleRepository`, `UserRepository`) into SQL, executes them against MySQL/MariaDB, and maps results back to model objects. Database schema changes are managed through EF migrations (e.g., `InitialCreate`, `CombineArchivedTables`).

## Migrations

Migrations are version-controlled database schema changes managed by Entity Framework Core. They define how the database structure evolves over time, such as creating tables for obstacles, reports, and users. In this application, migrations like `InitialCreate` and `CombineArchivedTables` are applied to keep the MySQL/MariaDB database schema in sync with the model definitions.

## Domain Models

Domain models represent core business entities and map directly to database tables. In this application, models include:

- **ObstacleData**: Obstacles with geometry, height, status
- **RapportData**: Reports/comments on obstacles
- **ApplicationUser**: User accounts with roles
- **ArchivedReport**: Rejected obstacles

These models contain validation attributes, relationships, and business logic, and are defined in the `Models` folder.

## View Models

View models are data transfer objects used to pass data between controllers and views, separate from domain models. They handle form input, display formatting, and role-specific data combinations.

Examples include:
- **RegisterViewModel**: Registration form
- **LoginViewModel**: Login credentials
- **RegistrarViewModel**: Combines obstacles and reports for the registrar view
- **UserWithRolesVm**: User data with role information for admin management

## Repositories

Repositories abstract database operations and provide a clean interface for data access. They encapsulate Entity Framework queries and operations, making controllers independent of database implementation details.

This application uses repositories like:
- **ObstacleRepository**: CRUD for obstacles
- **UserRepository**: User management
- **RegistrarRepository**: Report handling
- **ArchiveRepository**: Archiving rejected obstacles

All repositories implement interfaces for testability and dependency injection.

## Database

The application uses MySQL/MariaDB as the database, managed through Entity Framework Core's `ApplicationDBContext`. The context defines `DbSet` properties for `ObstaclesData`, `Rapports`, `ArchivedReports`, and `Feedback`, and configures relationships (e.g., obstacles to reports, obstacles to users) with cascade delete rules.

The database connection is configured in `appsettings.json` and can run in Docker containers for easy deployment and portability.
