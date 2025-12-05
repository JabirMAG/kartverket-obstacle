# Semesterproject for Kartverket re. Aviation Obstacles Grp. 18

## Application Setup

The technologies used in this application are as follows:

- **Docker**: For building and running the application and the database.
- **MariaDB**: Relational database used for data storage.
- **ASP.NET Core MVC**: Application built with the .NET 9 MVC framework.

## How to Run the Application

### Prerequisites

1. **Install Docker Desktop** on your machine ([Download Docker Desktop](https://www.docker.com/products/docker-desktop/))
2. **Clone the repository** to your local system:
   ```bash
   git clone <https://github.com/JabirMAG/kartverket-obstacle>
   cd kartverket-obstacle
   ```

### Starting the Application

1. **Open a terminal** and navigate to the project directory.

2. **Build and start the containers** using Docker Compose:
   ```bash
   docker compose up --build
   ```
   This command will build the Docker images and start both the application and database containers.

3. **Run in detached mode** (optional, if you want to run containers in the background):
   ```bash
   docker compose up -d
   ```

4. **Access the application** by opening your web browser and navigating to:
   ```
   http://localhost:8080
   ```

5. **Verify the containers are running** by checking Docker Desktop. You should see two containers running:
   - The FirstWebapplication container (ASP.NET Core)
   - The MariaDB container (MariaDB)

### Stopping the Application

To stop the application, press `Ctrl+C` in the terminal where it's running, or run:
```bash
docker compose down
```

To stop and remove all containers, volumes, and networks:
```bash
docker compose down -v
```

### Troubleshooting

**Application won't start:**
- Ensure Docker Desktop is running
- Check if port 8080 is already in use by another application
- Verify containers are running in Docker Desktop
- Check terminal for error messages

**Can't access the application:**
- Verify the URL is correct: `http://localhost:8080` (not `https://`)
- Check that both containers (app and database) are running in Docker Desktop
- Try restarting the containers: `docker compose restart`

**Database connection errors:**
- Wait a few seconds after starting containers for the database to fully initialize
- Restart containers: `docker compose restart`
- Check database container logs in Docker Desktop

**Port conflicts:**
- If port 8080 is in use, modify `docker-compose.yml` to use a different port
- Update the port mapping (e.g., `8081:8080`) and access via `http://localhost:8081`

## Application Features

This is an obstacle reporting and validation system for airspace safety, with role-based workflows for pilots, registrars, and administrators.

## Quick Start Guide

### Step 1: Access the Application

Once the application is running, open your browser and go to `http://localhost:8080`.

### Step 2: Login as Administrator

Use the following credentials to log in as an administrator:

- **Email:** `Admin@Kartverket.com`
- **Password:** `Admin123!`

This seeded admin user has full access to create users, approve registrations, and manage obstacles.

### Step 3: Create Test Users

**Recommended approach:** Create users directly through the admin interface:

1. After logging in as admin:
2. Select **"Brukerhåndtering"** (User Management)
3. Click **"Opprett ny bruker"** (Create New User)
4. Fill in the form:
   - Username
   - Email
   - Password (must meet requirements)
   - Role: Select either **"Pilot"** or **"Registerfører"**
   - Organization (optional)
5. Click **"Opprett"** (Create)

**Why this method?** Users created this way are immediately active and don't require approval, making it ideal for testing and demonstration.

**Alternative method:** Users can register through the front page registration form, but they will need admin approval before accessing the system.

### Step 4: Test Different Roles

1. **Log out** from the admin account
2. **Log in** with one of the newly created user accounts
3. **Explore the features** available to that role (see feature sections below)

### Navigation Guide

**Admin Menu:**
- Dashboard → Overview of system
- Brukerhåndtering → User management
- Rapporter → View all obstacle reports
- Arkiverte rapporter → View archived/rejected obstacles
- Tilbakemelding → View feedback submissions

**Pilot Menu:**
- Kart → Interactive map to report obstacles
- Mine rapporter → View own submitted obstacles
- Varsling → View notifications about comments on obstacles

**Registerfører Menu:**
- Rapporter → View and manage all obstacles
- Arkiverte rapporter → View archived obstacles

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
- Update obstacles (only when status is "Under behandling")
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
- Admin can view all feedback sent about the system

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

**Database communication**: The application uses `ApplicationDBContext` (configured in `Program.cs`) to interact with the database. EF translates LINQ queries from repositories (e.g., `ObstacleRepository`, `UserRepository`) into SQL, executes them against MySQL/MariaDB, and maps results back to model objects. Database schema changes are managed through EF migrations (e.g., `InitialDBContext`).

## Migrations

Migrations are version-controlled database schema changes managed by Entity Framework Core. They define how the database structure evolves over time, such as creating tables for obstacles, reports, and users. In this application, the `InitialDBContext` migration is applied to keep the MySQL/MariaDB database schema in sync with the model definitions.

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
