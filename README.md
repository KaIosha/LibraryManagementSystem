# Library Management System API

RESTful API for managing a small library: books, authors, categories, members, borrowing, and payments (cash + Stripe). Built with ASP.NET Core, EF Core Code First, SQL Server, and a clean-architecture layout.

## Stack

- .NET 10, ASP.NET Core Web API
- Entity Framework Core + SQL Server
- ASP.NET Core Identity + JWT (access + refresh rotation)
- Stripe.net (Checkout)
- Hangfire (background email jobs) + MailKit
- Swagger / OpenAPI

## Solution layout

- `LibraryManagementSystemAPI` — controllers, `Program.cs`, `appsettings`
- `LibraryManagementSystem.Application` — DTOs, service/repository interfaces
- `LibraryManagementSystem.Domain` — entities, enums
- `LibraryManagementSystem.Infrastructure` — DbContext, migrations, repositories, services

## Roles

There is no separate "Admin" role in code. The admin account seeds into the `Librarian` role, so treat `Librarian` as admin:

- `Librarian` — full access (add staff, delete/reactivate members, manage catalog)
- `Staff` — borrowing desk work
- `Member` — borrows books, pays

Seeded on first run: `admin@library.local` (Librarian), `staff@library.local` (Staff). Override via config, see below.

## Getting started

Prerequisites: .NET 10 SDK, SQL Server running locally.

```powershell
cd LibraryManagementSystemAPI

dotnet restore

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=LibraryDb;Trusted_Connection=True;TrustServerCertificate=True"

dotnet user-secrets set "JWT:Key" "your-long-random-key-min-32-chars"
dotnet user-secrets set "JWT:Issuer" "LibraryApi"
dotnet user-secrets set "JWT:Audience" "LibraryClient"
dotnet user-secrets set "JWT:AccessTokenExpiryMinutes" "60"
dotnet user-secrets set "JWT:RefreshTokenExpiryDays" "7"

dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
dotnet user-secrets set "Stripe:SuccessUrl" "https://localhost:7221/api/payment/success?session_id={CHECKOUT_SESSION_ID}"
dotnet user-secrets set "Stripe:CancelUrl" "https://localhost:7221/api/payment/cancel"
dotnet user-secrets set "Stripe:WebhookSecret" "temp_placeholder"

dotnet user-secrets set "EMAIL_CONFIGURATION:EMAIL" "you@gmail.com"
dotnet user-secrets set "EMAIL_CONFIGURATION:PASSWORD" "app-password"
dotnet user-secrets set "EMAIL_CONFIGURATION:HOST" "smtp.gmail.com"
dotnet user-secrets set "EMAIL_CONFIGURATION:PORT" "587"

dotnet user-secrets set "SeedUsers:Admin:Email" "admin@library.local"
dotnet user-secrets set "SeedUsers:Admin:Password" "Admin123!"
dotnet user-secrets set "SeedUsers:Staff:Email" "staff@library.local"
dotnet user-secrets set "SeedUsers:Staff:Password" "Staff123!"
```

Apply migrations and run:

```powershell
dotnet ef database update --project ..\LibraryManagementSystem.Infrastructure --startup-project .
dotnet run
```

URLs (see `Properties/launchSettings.json`):

- HTTPS: `https://localhost:7221`
- HTTP: `http://localhost:5209`
- Swagger: `https://localhost:7221/swagger`
- Hangfire: `https://localhost:7221/hangfire`

## What is implemented

- Books: create, update, soft-delete, list, single, search by title/ISBN, filter by category/author, availability check
- Authors: create, edit, soft-delete, list, details, books by author
- Categories: create, update, soft-delete, list, books in category
- Members: register, update, soft-delete (`IsActive=false`, history kept, refresh tokens revoked), reactivate (Librarian only), profile, member borrows
- Borrowing: borrow (blocks unavailable + double-borrow via unique index), return, due date (default 14 days), overdue list, history, member history
- Fees: base borrow fee 5 on borrow, late fee 2/day computed on return into `LateFee`. Payable = `TotalAmount + LateFee`
- Payments: cash (immediate `Paid`) and Stripe Checkout (`Pending` then `Paid` after success redirect verification), payment history per borrow
- Auth: register + email confirm code, login, refresh rotation, logout (single token), forgot/reset password, add staff (Librarian only)

A ready Postman list lives in `testEnpoint.txt` (endpoint order + sample bodies).

## Testing payments with Stripe

1. Borrow a book, copy `borrowId`.
2. `POST /api/payment/checkout` with that `borrowId`.
3. Open `sessionUrl` from the response, pay with `4242 4242 4242 4242`, any future expiry, any CVC.
4. You land on `/api/payment/success?session_id=...`, which flips the row from `Pending` to `Paid`.
5. Confirm with `GET /api/payment/borrow/{borrowId}`.

## Notes and limits

- Success verification is redirect-based (`GET /payment/success`), not a Stripe webhook. Good enough for local testing; a webhook with signature verification is the production follow-up.
- Logout revokes the one refresh token sent (single device). Access tokens stay valid until expiry, which is standard for stateless JWT.
- Most `[Authorize]` attributes on catalog/borrow controllers are commented out for local testing. Enable them before any shared deployment.
