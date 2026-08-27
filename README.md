# InvestFlow

InvestFlow is a calm, practical personal finance MVP for tracking income, expenses, monthly budgets, investments, net worth, and financial goals.

## Included

- Identity account registration and sign-in
- User-scoped transactions and custom categories
- Monthly budgets and remaining-budget calculations
- Manual investments and contributions
- Accounts, debts, net worth, and financial goals
- Monthly dashboard and Monthly X-Ray
- Responsive layouts, validation, empty states, and calculation tests

## Stack

React, TypeScript, Vite, Tailwind CSS, .NET 10, PostgreSQL, Entity Framework Core, ASP.NET Core Identity, FluentValidation, and xUnit.

## Backend architecture

The API uses vertical slices under `src/InvestFlow.Api/Features`. Each feature owns its endpoints, request contracts, validators, and response models. Business entities and financial calculations live under `Domain`; PostgreSQL and Entity Framework configuration live under `Infrastructure/Persistence`. Fast unit tests live under `src/InvestFlow.Api/Tests`; database integration and end-to-end API tests live under `src/InvestFlow.Api.IntegrationTests`.

## Frontend architecture

The React app is organised by feature under `src/InvestFlow.Web/src/features`. HTTP calls live in typed feature services, API responses are validated at runtime with Zod, and reusable components, hooks, schemas, and formatting utilities live under `shared`. Frontend tests are colocated with the code they verify.

## Run locally

1. Make sure your PostgreSQL container is listening on `localhost:5432` with the `investflow` database and the credentials configured in `src/InvestFlow.Api/appsettings.Development.json`.
2. Run `dotnet run --project src/InvestFlow.Api`.
3. In `src/InvestFlow.Web`, run `pnpm install` and then `pnpm dev`.
4. Open `http://localhost:5173` and sign in or create an account.

The development schema is created on first API startup. A local administrator is also created when it does not already exist:

- Username: `admin`
- Password: `123`

This short password bypass is restricted to the development seed; registered accounts still follow the normal password policy. Override the local connection string with `ConnectionStrings__Postgres` and seed settings with the `DevelopmentSeed__*` environment variables when needed.

## Run with Docker

1. Copy `.env.example` to `.env` and replace `POSTGRES_PASSWORD` with a private value.
2. Run `docker compose up --build` from the repository root.
3. Open `http://localhost:5173` and create an account.

The Compose stack starts the React frontend on port `5173`, the API on port `5017`, PostgreSQL on port `5432`, and Redis on port `6379`. Override these ports in `.env` if necessary. PostgreSQL data, Redis data, and ASP.NET Core data-protection keys are stored in named volumes and survive container recreation.

The API runs in Production mode and applies pending database migrations when the container starts. Use `docker compose down` to stop the stack without removing its data.

## Dashboard cache

The dashboard and Monthly X-Ray use a five-minute cache keyed by user and month. Configure Redis with the `ConnectionStrings__Redis` environment variable, for example `localhost:6379`. When that connection string is not configured, the API uses an in-memory cache, keeping local development and tests self-contained. If Redis becomes unavailable while the API is running, dashboard reads fall back to PostgreSQL.

Changes to transactions, categories, accounts, investments, and contributions invalidate the affected user's dashboard cache. Net-worth snapshots are updated when accounts or investments change instead of during dashboard reads.

## Database migrations

The API applies pending Entity Framework migrations automatically when it starts in Development. To manage them manually from the repository root:

1. Run `dotnet tool restore`.
2. Create a migration with `dotnet tool run dotnet-ef migrations add MigrationName --project src/InvestFlow.Api --startup-project src/InvestFlow.Api --output-dir Infrastructure/Persistence/Migrations`.
3. Apply pending migrations with `dotnet tool run dotnet-ef database update --project src/InvestFlow.Api --startup-project src/InvestFlow.Api`.

## Backend tests

The backend uses xUnit v3 and Microsoft Testing Platform. Run the fast, Docker-independent unit suite with:

```powershell
dotnet test src/InvestFlow.Api/Tests/InvestFlow.Api.Tests.csproj
```

Integration and end-to-end tests exercise the real HTTP pipeline, authentication, Entity Framework migrations, PostgreSQL constraints, and user isolation. They use Testcontainers to create a disposable PostgreSQL instance, so Docker Desktop (or another compatible Docker engine) must be running; the Compose stack does not need to be started first.

```powershell
dotnet test src/InvestFlow.Api.IntegrationTests/InvestFlow.Api.IntegrationTests.csproj
```

Run every backend suite with `dotnet test InvestFlow.slnx`. Test data uses unique users, and the PostgreSQL container is shared only within the integration test run to keep execution isolated and reasonably fast.

## Verify

Run the backend suites above, then run `pnpm lint`, `pnpm test`, and `pnpm build` from `src/InvestFlow.Web`.
