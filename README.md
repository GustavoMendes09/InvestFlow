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

The API uses vertical slices under `src/InvestFlow.Api/Features`. Each feature owns its endpoints, request contracts, validators, and response models. Business entities and financial calculations live under `Domain`; PostgreSQL and Entity Framework configuration live under `Infrastructure/Persistence`. Backend tests are kept with the API under `src/InvestFlow.Api/Tests`.

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

## Database migrations

The API applies pending Entity Framework migrations automatically when it starts in Development. To manage them manually from the repository root:

1. Run `dotnet tool restore`.
2. Create a migration with `dotnet tool run dotnet-ef migrations add MigrationName --project src/InvestFlow.Api --startup-project src/InvestFlow.Api --output-dir Infrastructure/Persistence/Migrations`.
3. Apply pending migrations with `dotnet tool run dotnet-ef database update --project src/InvestFlow.Api --startup-project src/InvestFlow.Api`.

## Verify

Run `dotnet test`, then `pnpm lint`, `pnpm test`, and `pnpm build` from `src/InvestFlow.Web`.
