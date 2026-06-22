# HikeCampPlatform

A platform for discovering and booking hiking and camping tours worldwide. Verified operators list tours (with route data for map display); users browse, book, and complete tours to climb a leaderboard, sharing photos along the way.

> **Status:** Early-stage backend MVP. Auth and tour listing are working; bookings, completions, leaderboard, and payments are not yet built. No frontend yet (backend-first approach).

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 10)
- **Database:** PostgreSQL (via EF Core + Npgsql)
- **Auth:** Custom JWT (BCrypt password hashing)
- **API docs:** Scalar (OpenAPI UI)
- **Local dev:** Docker Compose (Postgres container)

## What's Implemented

- **User auth** — register, login, JWT issuance, protected endpoints (`/api/auth/*`, `/api/users/me`)
- **Operator auth** — same pattern, separate table, `IsVerified` flag for future admin approval (`/api/operator-auth/*`)
- **Tours** — operators create tours with multi-point routes (lat/long, ordered); public browsing only shows published tours (`/api/tours/*`)
- **Role-based authorization** — Users and Operators carry distinct JWT role claims; operator-only endpoints reject User tokens with `403`

## What's Planned

- Stripe-based listing fee for operators publishing a tour
- Admin approval flow for operator verification + tour publishing
- Bookings + payments (users book tours, pay per person)
- Completions tied to fulfilled bookings (drives leaderboard integrity)
- Leaderboard (ranks users by verified completed tours)
- Photo sharing tied to completions
- Frontend (TBD)

## Project Structure

\`\`\`
HikeCampPlatform/
├── docker-compose.yml          # Local Postgres container
├── HikeCampPlatform.slnx       # Solution file
└── src/
    └── HikeCampPlatform.Api/
        ├── Controllers/        # HTTP endpoints
        ├── Models/              # EF Core entities
        ├── DTOs/                # Request/response shapes
        ├── Data/                # AppDbContext
        ├── Migrations/          # EF Core migrations
        ├── Program.cs           # App startup/config
        └── appsettings.json     # Config (connection string, JWT settings)
\`\`\`

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Setup

1. **Start Postgres:**
   \`\`\`bash
   docker compose up -d
   \`\`\`

2. **Apply migrations:**
   \`\`\`bash
   cd src/HikeCampPlatform.Api
   dotnet ef database update
   \`\`\`

3. **Run the API:**
   \`\`\`bash
   dotnet run
   \`\`\`

4. **Open the API explorer:**
   \`\`\`
   http://localhost:5261/scalar/v1
   \`\`\`

### Connection details (local dev)

The default `appsettings.json` connects to the Docker Postgres container at `localhost:5433` (mapped from the container's internal `5432` to avoid conflicts with any native Postgres install on the host). Credentials are dev-only placeholders — see `docker-compose.yml`.

> The JWT secret in `appsettings.json` is also a placeholder for local development only. Before any real deployment, replace it with a securely generated secret stored outside source control (environment variable or secrets manager).

## API Overview

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/auth/register` | POST | None | Register a new user |
| `/api/auth/login` | POST | None | Log in, get a JWT |
| `/api/users/me` | GET | User | Get the logged-in user's profile |
| `/api/operator-auth/register` | POST | None | Register a new operator |
| `/api/operator-auth/login` | POST | None | Log in, get a JWT |
| `/api/tours` | POST | Operator | Create a tour (with route points) |
| `/api/tours` | GET | None | Browse published tours |
| `/api/tours/{id}` | GET | None | Get a single published tour |

## License

Not yet decided — private development for now.