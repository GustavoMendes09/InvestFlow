# AGENTS.md

## Product Context

We are building an MVP for a personal finance app that brings together spending, investments, and net worth tracking.

The product should help users understand:
- how much they earn
- how much they spend
- how much they can invest
- how their net worth evolves over time

The app should be simple, clear, and useful for people who are starting to organize their financial life and invest.

## Stack

- Frontend: React + TypeScript
- Backend: C# + .NET 10
- Database: PostgreSQL
- Styling: Tailwind CSS
- Backend architecture: Vertical Slice Architecture

## Backend Frameworks

- Authentication and user management: ASP.NET Core Identity
- Authorization: ASP.NET Core Authorization Policies
- ORM: Entity Framework Core
- Database provider: Npgsql for PostgreSQL
- Validation: FluentValidation
- API documentation: OpenAPI / Swagger
- Unit testing: xUnit
- Integration testing: xUnit + Testcontainers
- Logging: built-in .NET logging with structured logs

## Architecture Rules

- Use Clean Code principles.
- Prefer event-driven architecture where it improves decoupling between business workflows.
- Keep business rules out of UI components and infrastructure code.
- Keep functions small, explicit, and easy to test.
- Use clear names that express business intent.
- Avoid premature abstractions; introduce abstractions only when they reduce real duplication or complexity.
- Do not put API logic directly inside UI components.
- Use validation in forms and application boundaries.
- Follow existing project patterns before introducing new ones.

## MVP Scope

Prioritize only what is essential for the first version:

- income tracking
- expense tracking
- spending categories
- monthly budget by category
- manual investment tracking
- investment contributions
- current investment values
- total net worth
- financial goals
- monthly dashboard
- "Monthly X-Ray" screen

Avoid in the MVP:

- automatic bank integrations
- brokerage integrations
- advanced tax calculations
- personalized financial advice
- trading, technical analysis, or complex charts
- multiple countries or currencies unless explicitly requested

## Core User Flows

The user should be able to:

1. Create an account or sign in.
2. Register monthly income.
3. Register expenses by category.
4. Define a monthly budget.
5. Add investments manually.
6. Register investment contributions.
7. Track total net worth.
8. View a clear monthly summary.
9. Create and track financial goals.

## Main Screens

Build the MVP around these screens:

- Dashboard
- Transactions
- Categories
- Budget
- Investments
- Net Worth
- Goals
- Monthly X-Ray
- Basic Settings

## Product Principles

Clarity over complexity.

The interface should quickly answer:

- How much money came in?
- How much money went out?
- How much was left?
- How much was invested?
- Did my net worth grow?
- Which category had the biggest impact?
- Am I close to my goals?

Avoid financial jargon when simple language works better.

## UX Direction

The app should feel trustworthy, organized, and calm.

Prefer:

- scannable dashboards
- simple charts
- clean tables
- useful empty states
- fast forms
- clear feedback after actions

Avoid:

- overloaded screens
- a trading-app visual style
- excessive decorative cards
- long in-app text explaining features

## Data Model

Main entities:

- User
- Account
- Transaction
- Category
- Budget
- Investment
- InvestmentHolding
- InvestmentContribution
- Goal
- MonthlySnapshot

Transactions should have:

- type: income or expense
- amount
- date
- category
- optional description
- optional account

Investments should have:

- name
- asset class
- invested amount
- current value
- contributions
- update date

Goals should have:

- name
- target amount
- current amount
- optional deadline
- type: emergency fund, travel, debt, property, retirement, or other

## Technical Priorities

Implement the functional end-to-end experience first.

Priority order:

1. Simple data model
2. CRUD for the main entities
3. Correct dashboard calculations
4. Useful empty states
5. Basic responsiveness
6. Form validation
7. Tests for the main financial calculations

Do not create large abstractions before there is real repetition.

## Financial Calculations

Important calculations:

- monthly balance = income - expenses
- savings rate = invested amount / income
- net worth = accounts + investments - debts
- goal progress = current amount / target amount
- monthly net worth variation = current net worth - previous net worth
- remaining budget = category budget - category expenses

Prioritize accuracy and predictability. Do not hide or mask negative values.

## Testing Guidance

Add tests for:

- income and expense totals
- monthly balance calculation
- remaining budget calculation
- net worth calculation
- goal progress
- monthly net worth evolution
- transaction classification by category

UI tests should focus on the main MVP flows.

## Implementation Behavior

Before implementing, read the existing code and follow the project patterns.

When making changes:

- keep the scope small
- avoid unrelated refactors
- preserve user data
- validate forms
- handle empty states and errors
- ensure the layout is usable on desktop and mobile

## Definition of Done

A feature is only done when:

- the user can complete the main flow
- common errors are handled
- the interface works on mobile and desktop
- related calculations are tested
- there is no mocked data in final screens unless explicitly requested
- the code follows existing project patterns
