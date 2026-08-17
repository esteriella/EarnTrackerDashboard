# Freelancer Earnings Tracker

EarnTracker is a full-stack dashboard for freelancers to monitor income from payment providers, review gross earnings and fees, and track progress toward financial goals.

## Features

- Registration, login, JWT access tokens, and rotating refresh tokens
- PostgreSQL persistence through Entity Framework Core
- PayPal Sandbox order creation, approval, capture, and capture lookup
- Automatic recording of completed PayPal captures as user earnings
- Paystack transaction verification
- Currency-grouped gross, fee, and net totals
- Active, achieved, and expired financial goals
- Responsive Next.js dashboard
- Scalar interactive API documentation
- Serilog logging and centralized problem responses
- GitHub Actions backend build and migration-model validation

> PayPal is configured for Sandbox testing. Do not use real PayPal accounts or real money in the development workflow.

## Documentation

- [Complete API reference](docs/API.md)
- [Backend](src/EarnTrackerApi)
- [Frontend](src/EarnTrackerWeb)
- [Backend CI](.github/workflows/backend-ci.yml)

## Technology stack

| Area | Technology |
| --- | --- |
| Frontend | Next.js 16.3, React 19, TypeScript, Tailwind CSS 4 |
| Backend | ASP.NET Core 10 and C# |
| Database | PostgreSQL |
| Data access | Entity Framework Core 10 with Npgsql |
| Authentication | JWT Bearer and PBKDF2 password hashing |
| API documentation | ASP.NET Core OpenAPI and Scalar |
| Integrations | PayPal REST API v2 Sandbox and Paystack |
| Logging | Serilog and SerilogTracing |
| Package managers | pnpm 11.5 and NuGet |
| CI | GitHub Actions |

## Architecture

```mermaid
flowchart LR
    Browser[Next.js dashboard] -->|JWT and JSON| API[ASP.NET Core API]
    API --> Auth[Authentication]
    API --> Library[Dashboard and goals]
    API --> Payments[Payment integrations]
    Auth --> DB[(PostgreSQL)]
    Library --> DB
    Payments --> PayPal[PayPal Sandbox]
    Payments --> Paystack[Paystack]
    Payments --> Recorder[Payment recorder]
    Recorder --> DB
```

The browser uses an EarnTracker JWT. PayPal OAuth is separate and happens only on the server with the PayPal client ID and secret. Provider credentials and OAuth tokens are never sent to the browser.

## Repository structure

```text
EarnTrackerDashboard/
├── .github/workflows/backend-ci.yml
├── docs/API.md
├── src/
│   ├── EarnTrackerApi/
│   │   ├── Controllers/
│   │   ├── Data/Repository/
│   │   ├── Data/UnitOfWork/
│   │   ├── Dtos/
│   │   ├── Exceptions/
│   │   ├── Extensions/
│   │   ├── Helpers/
│   │   ├── Interfaces/
│   │   ├── Middleware/
│   │   ├── Migration/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Startup/
│   │   └── Program.cs
│   └── EarnTrackerWeb/
│       ├── src/app/
│       ├── src/components/
│       ├── src/lib/
│       └── package.json
└── README.md
```

Backend responsibilities:

- `Controllers`: routes and authenticated-user boundaries.
- `Data`: EF context, repositories, and unit-of-work save boundary.
- `Dtos`: validated request and response contracts.
- `Models`: PostgreSQL entities.
- `Services`: auth, JWT, caching, provider clients, and payment recording.
- `Startup`: database, JWT, HTTP clients, logging, DI, and middleware pipeline.
- `Middleware`: consistent `application/problem+json` errors.

## Prerequisites

- .NET SDK 10
- Node.js 24 or a compatible current Node.js version
- pnpm 11.5
- PostgreSQL
- PayPal developer account with Sandbox personal and business accounts
- Paystack test secret when testing Paystack

```powershell
dotnet --version
node --version
pnpm --version
psql --version
```

## PostgreSQL setup

Create a database named exactly `EarnTracker`. The default local connection string is:

```text
Host=localhost;Port=5432;Database=EarnTracker;Username=postgres;Password=postgres
```

Override it without committing a password:

```powershell
cd src\EarnTrackerApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=EarnTracker;Username=YOUR_USER;Password=YOUR_PASSWORD"
```

## Backend secrets

Run from `src\EarnTrackerApi`:

```powershell
dotnet user-secrets set "Jwt:Key" "YOUR_RANDOM_JWT_KEY_AT_LEAST_32_BYTES"
dotnet user-secrets set "HashHelper:SecretKey" "YOUR_RANDOM_PASSWORD_PEPPER_AT_LEAST_32_BYTES"
dotnet user-secrets set "PayPal:ClientId" "YOUR_PAYPAL_SANDBOX_CLIENT_ID"
dotnet user-secrets set "PayPal:ClientSecret" "YOUR_PAYPAL_SANDBOX_CLIENT_SECRET"
dotnet user-secrets set "PayStack:ClientSecret" "YOUR_PAYSTACK_TEST_SECRET"
```

Generate a random 32-byte Base64 value in PowerShell:

```powershell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

Generate separate values for the JWT and hash-helper keys. Never commit provider secrets, database passwords, JWT signing keys, or password peppers. Rotate any credential exposed in source control, chat, screenshots, or logs.

The PayPal base URL is non-secret and configured as:

```text
https://api-m.sandbox.paypal.com/
```

Development startup rejects a different PayPal host. PayPal credentials must belong to a Sandbox REST app associated with the business sandbox account.

## Migrations

```powershell
cd src\EarnTrackerApi
dotnet tool restore
dotnet tool run dotnet-ef database update --context AppDbContext
dotnet tool run dotnet-ef migrations list --context AppDbContext
```

After changing EF models:

```powershell
dotnet tool run dotnet-ef migrations add MeaningfulName --context AppDbContext --output-dir Migration
```

Keep generated migration namespaces as `EarnTrackerApi.DataMigrations`; this avoids colliding with EF Core's `Migration` type while retaining the requested physical `Migration` folder.

To roll back every applied migration while keeping the database:

```powershell
dotnet tool run dotnet-ef database update 0 --context AppDbContext
```

The API calls `MigrateAsync()` at startup, so it automatically applies pending migrations when PostgreSQL is reachable.

## Run the backend

```powershell
cd src\EarnTrackerApi
dotnet restore
dotnet run
```

| Resource | URL |
| --- | --- |
| HTTP API | `http://localhost:5048` |
| HTTPS API | `https://localhost:7140` |
| Scalar | `http://localhost:5048/scalar/v1` |
| OpenAPI JSON | `http://localhost:5048/openapi/v1.json` |
| Health | `http://localhost:5048/health` |

Scalar and OpenAPI are Development-only.

## Run the frontend

```powershell
cd src\EarnTrackerWeb
pnpm install
pnpm dev
```

Open `http://localhost:3000`. The frontend uses `NEXT_PUBLIC_API_URL`, falling back to `http://localhost:5048`.

Optional `src/EarnTrackerWeb/.env.local`:

```dotenv
NEXT_PUBLIC_API_URL=http://localhost:5048
```

Never put private secrets in `NEXT_PUBLIC_*`; those values are bundled into browser code.

## Authentication workflow

1. Register at `POST /api/auth/register` or log in at `POST /api/auth/login`.
2. The API returns an access token and refresh token.
3. The frontend stores the session under `earntracker-session` in local storage.
4. Protected requests send `Authorization: Bearer ACCESS_TOKEN`.
5. `POST /api/auth/refresh` rotates a valid refresh token and returns a new token pair.

Default access and refresh lifetimes are 10 and 20 minutes. Passwords must be 8–12 characters with uppercase, lowercase, number, and special characters. They are stored using salted PBKDF2-SHA256 with 210,000 iterations plus a server-side pepper. Only HMAC-SHA256 hashes of refresh tokens are persisted.

## PayPal-to-dashboard workflow

```mermaid
sequenceDiagram
    participant U as User
    participant W as Next.js
    participant A as EarnTracker API
    participant P as PayPal Sandbox
    participant D as PostgreSQL
    U->>W: Create order
    W->>A: POST PayPal order
    A->>P: Orders v2 create
    P-->>W: Order ID and approval URL
    U->>P: Approve as sandbox buyer
    W->>A: Capture approved order
    A->>P: Capture order
    P-->>A: Completed capture
    A->>D: Upsert source and earning
    W->>A: Load overview
    A-->>W: Updated totals and goals
```

Completed PayPal captures are recorded for the authenticated EarnTracker user. The unique PayPal capture ID prevents repeated checks from duplicating a transaction. Older completed captures can be imported by checking their capture ID once.

### Sandbox test flow

1. Use a PayPal business sandbox account as seller/app owner.
2. Use a PayPal personal sandbox account as buyer.
3. Sign in to EarnTracker and open **Connections**.
4. Create a PayPal test order.
5. Open its `approve` link in a private browser.
6. Sign in with the personal sandbox buyer and approve.
7. Return to EarnTracker and confirm status `APPROVED`.
8. Capture the order.
9. Confirm the payment appears in Overview, Transactions, and PayPal Earnings.

The capture ID is at:

```text
purchase_units[0].payments.captures[0].id
```

It may also appear as the seller-side transaction ID in the business sandbox account's Activity page.

## Financial goals

Users set a name, target net amount, currency, start date, and target date. Only completed transactions with the same currency and an occurrence date inside the inclusive date range contribute.

```text
current amount = sum(amount - fee)
progress = min(100, current amount / target amount × 100)
```

| Status | Meaning |
| --- | --- |
| Active | Below target and target date has not passed |
| Achieved | Net earnings reached or exceeded the target |
| Expired | Target date passed before achievement |

Status is derived dynamically; no separate database status column is required.

## Data model

| Entity | Purpose |
| --- | --- |
| `User` | Account identity and password hash |
| `RefreshToken` | Hashed, expiring, revocable token |
| `IncomeSource` | User/provider/currency grouping, such as PayPal USD |
| `EarningTransaction` | Amount, fee, status, currency, date, and provider ID |
| `FinancialGoal` | User target, currency, and date range |

A user owns refresh tokens, sources, and goals. A source owns transactions. Deletes cascade from the user. `(IncomeSourceId, ExternalId)` is unique to prevent duplicate provider transactions.

## Error handling and logs

Application errors use `application/problem+json`; validation uses ASP.NET Core validation-problem responses. The frontend displays field errors, session expiration, missing endpoint guidance, and HTTP failures.

Serilog writes to the console and daily files under `src/EarnTrackerApi/logs`. Never log credentials or tokens. Low-level development tracing can report a failed connection attempt before a successful fallback; check the final request status.

## CI and build checks

The backend workflow runs on backend pushes to `main`, pull requests, and manual dispatch. It restores .NET tools and packages, builds Release, runs test projects when present, and checks for EF model changes missing a migration. Placeholder CI configuration never accesses providers or PostgreSQL.

Backend checks:

```powershell
cd src\EarnTrackerApi
dotnet restore
dotnet build --configuration Release --no-restore
dotnet tool run dotnet-ef migrations has-pending-model-changes --context AppDbContext --configuration Release --no-build
```

Frontend checks:

```powershell
cd src\EarnTrackerWeb
pnpm install
pnpm lint
pnpm build
```

## Docker

```powershell
cd src\EarnTrackerApi
docker build -t earntracker-api .
```

Pass configuration with environment variables, using double underscores for nested ASP.NET Core keys:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
HashHelper__SecretKey
PayPal__ClientId
PayPal__ClientSecret
PayStack__ClientSecret
```

Inside a container, PostgreSQL is normally not `localhost`; use its service hostname or `host.docker.internal` as appropriate.

## Deploying the backend to Render

Create the PostgreSQL database and backend web service in the same Render region. For the web service use:

| Render setting | Value |
| --- | --- |
| Runtime | Docker |
| Root Directory | `src/EarnTrackerApi` |
| Dockerfile Path | `Dockerfile` |
| Health Check Path | `/health` |

On the Render PostgreSQL **Connect** page, copy the **Internal Database URL** and add it to the web service:

```text
DATABASE_URL=postgresql://user:password@internal-host:5432/database
```

The API converts Render's URL format to an Npgsql connection string. `DATABASE_URL` takes priority over the local `ConnectionStrings:DefaultConnection` setting. Do not use the local `localhost` connection string on Render.

Add these web-service environment variables:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
DATABASE_URL=<Render Internal Database URL>
Jwt__Key=<random secret of at least 32 bytes>
Jwt__Issuer=EarnTrackerApi
Jwt__Audience=EarnTrackerWeb
HashHelper__SecretKey=<different random secret of at least 32 bytes>
PayPal__ClientId=<sandbox client ID>
PayPal__ClientSecret=<sandbox client secret>
PayStack__ClientSecret=<Paystack test secret>
AllowedOrigins__0=https://YOUR-FRONTEND.onrender.com
```

Render supplies `PORT`; the API automatically binds to `http://0.0.0.0:$PORT`. On deployment, startup applies pending EF migrations to the Render database before accepting requests.

Use Render's internal database URL for the Render-hosted API. The external URL is intended for clients outside Render and normally requires SSL.

## Troubleshooting

### `ERR_PNPM_NO_PKG_MANIFEST`

Run pnpm from `src\EarnTrackerWeb`, where `package.json` exists.

### DLL locked during `dotnet run`

Visual Studio or IIS Express is already hosting the API. Stop debugging with `Shift+F5`, stop the relevant IIS Express process if needed, then rerun.

### Goal creation returns 404

An old backend process is running. Stop it, rebuild, restart, and reload the frontend.

### `relation "__EFMigrationsHistory" does not exist`

EF probes for this table on a new database. If migration update finishes with `Done.` and exit code 0, it succeeded.

### Hash-helper key is too short

Store at least 32 UTF-8 bytes under `HashHelper:SecretKey`.

### `NU1900`

NuGet could not reach its vulnerability-data service. This is generally a network warning rather than a compiler failure.

### PayPal capture is missing from the dashboard

Confirm that the capture is `COMPLETED`, was captured or checked while signed in to the intended EarnTracker account, uses the Sandbox host, and the overview was refreshed. Use capture lookup once to import an older capture.

## Current limitations

- Paystack verification does not yet persist earnings.
- Crypto has an HTTP client but no public dashboard workflow.
- No automated backend test project exists yet; CI will run one when added.
- Scalar is Development-only.
- The browser session is stored in local storage; production should evaluate secure cookies and a stricter refresh strategy.
- Production still needs provider webhooks, webhook signature validation, rate limiting, managed secrets, deployment observability, and reconciliation.

## Production security checklist

- Rotate credentials ever exposed outside a secret manager.
- Replace all development keys and passwords.
- Enforce HTTPS.
- Use a least-privilege PostgreSQL account.
- Restrict CORS to the deployed frontend.
- Separate PayPal Sandbox and live credentials.
- Move to PayPal live only through an explicit production release.
- Verify PayPal webhooks before accepting asynchronous state.
- Rate-limit authentication and provider endpoints.
- Add authentication, authorization, repository, and integration tests.
- Store deployment secrets in the hosting platform's secret manager.
