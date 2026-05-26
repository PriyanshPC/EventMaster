# EventMaster

EventMaster is a full-stack event booking platform built with ASP.NET Core. It includes:

- **EventMaster.Api**: REST API for authentication, events, bookings, reviews, venues, and payments.
- **EventMaster.Web**: MVC frontend that consumes the API and provides the user interface.
- **MySQL** database with schema and seed data bootstrapped from SQL scripts.

## Architecture

- **Backend API**: ASP.NET Core Web API + Entity Framework Core + MySQL
- **Frontend**: ASP.NET Core MVC (Razor Views)
- **Auth**:
  - JWT in API
  - Cookie authentication in Web app
- **API docs**: Swagger UI served by the API at `/swagger`

## Repository Structure

```text
.
├── EventMaster.Api/        # API project
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Entities/
│   ├── Security/
│   └── db/                 # SQL schema + seed scripts
├── EventMaster.Web/        # MVC frontend project
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   └── wwwroot/
├── docker-compose.yml      # Local multi-container setup
└── EventMaster.sln
```

## Quick Start (Docker)

### Prerequisites

- Docker + Docker Compose

### Run

```bash
docker compose up --build
```

This starts:

- **Web app**: `http://localhost:8080`
- **API**: `http://localhost:8081` (Swagger at `http://localhost:8081/swagger`)
- **MySQL**: `localhost:3306`

Database schema and seed data are initialized automatically from `EventMaster.Api/db` on first startup.

## Run Locally (without Docker)

### Prerequisites

- .NET SDK 8+
- MySQL 8+

### 1) Create database and apply SQL scripts

Run scripts in order:

1. `EventMaster.Api/db/01_schema.sql`
2. `EventMaster.Api/db/02_seed.sql`

### 2) Configure connection string

Set `ConnectionStrings:Default` in `EventMaster.Api/appsettings.Development.json` (or environment variable) to your MySQL instance.

### 3) Start API

```bash
dotnet run --project EventMaster.Api
```

### 4) Start Web

In a new terminal:

```bash
dotnet run --project EventMaster.Web
```

Default local URLs:

- Web: `http://localhost:5260` (or configured launch URL)
- API: `http://localhost:5000/5001` or configured launch URL

> If needed, set `Api:BaseUrl` in `EventMaster.Web/appsettings.Development.json` to match your running API URL.

## Main Functional Areas

- User registration, login, and profile management
- Event discovery and details
- Booking flow and payment handling
- User/organizer dashboards
- Review and reply system

## Useful Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build EventMaster.sln

# Run tests (if present)
dotnet test EventMaster.sln
```

## Notes

- API root (`/`) redirects to Swagger.
- API CORS policy is configured to allow browser-based frontend access.
- Static files are served in both API and Web projects.
