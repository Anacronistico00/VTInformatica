# VTInformatica – API (back end)

[🇮🇹 Italiano](README.md) · 🇬🇧 English

REST API for an e-commerce site selling PC tech products: catalogue, cart, orders, reviews, accounts and a role-based back office (`Admin`, `Seller`, `Customer`). The front end lives in [vtinformatica_fe](https://github.com/Anacronistico00/vtinformatica_fe).

It started as a course final project (tag `v1-epicode`), then went through an audit and was reworked milestone by milestone (`v2-m1` … `v2-m4-be`). The [Before / after](#before--after) and [How I worked with AI](#how-i-worked-with-ai) sections tell the story.

## Stack

- ASP.NET Core Web API (net8.0), Entity Framework Core 8, SQL Server
- ASP.NET Identity + JWT Bearer, Serilog, Swagger
- Tests: xUnit + `WebApplicationFactory`, in-memory SQLite (no secrets and no SQL Server needed)
- CI: GitHub Actions (restore, Release build, test)

## Architecture

```mermaid
flowchart LR
    B["Browser<br/>React SPA (vtinformatica_fe)"] -- "HTTPS + JWT Bearer<br/>JSON / ProblemDetails" --> A["ASP.NET Core API<br/>Controllers → Services → DbContext"]
    A -- "EF Core" --> D[("SQL Server")]
```

- `Controllers/`: thin, one per resource. They validate input, call the service and map the response.
- `Services/` + `Interfaces/`: business logic; operations touching several tables (order + stock + cart) run in a single transaction.
- `Mapping/`: one entity → DTO mapping per resource. Entities never leave the API.
- `Errors/`: global exception handler; every error is a `ProblemDetails` with a stable `code` and a `traceId`.
- The user's identity always comes from the JWT claims, never from the route or body (`/me` routes).

## Running locally

Prerequisites: .NET 8 SDK, SQL Server (Express is fine), `dotnet-ef` (`dotnet tool install -g dotnet-ef`).

1. **Clone both repos** side by side (the FE end-to-end tests expect `../VTInformatica`):

   ```bash
   git clone https://github.com/Anacronistico00/VTInformatica.git
   git clone https://github.com/Anacronistico00/vtinformatica_fe.git
   ```

2. **Database.** Run `DatabaseScript.sql` (roles and sample data) in SQL Server Management Studio, then set the connection string in `VTInformatica/appsettings.json` or, better, in User Secrets:

   ```bash
   cd VTInformatica
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=VTInformatica;Trusted_Connection=True;TrustServerCertificate=True;" --project VTInformatica
   ```

   Apply the migrations that came after the script (take a backup first if the DB holds data you care about):

   ```bash
   dotnet ef database update --project VTInformatica
   ```

3. **JWT key.** It is not in the repository: without `Jwt:SecurityKey` (at least 32 characters) the API refuses to start.

   ```bash
   dotnet user-secrets set "Jwt:SecurityKey" "<key of at least 32 characters>" --project VTInformatica
   ```

   In production use the `Jwt__SecurityKey` environment variable. The key used in the past is in this public repo's git history: treat it as compromised and never reuse it. `Jwt:Issuer`, `Jwt:Audience` and `Jwt:ExpiresInMinutes` stay in `appsettings.json`.

4. **Start the API:**

   ```bash
   dotnet run --project VTInformatica --launch-profile https
   ```

   It listens on `https://localhost:7286` (Swagger in Development). CORS allows `http://localhost:5173` by default; add other origins with environment variables, e.g. `Cors__AllowedOrigins__1=http://localhost:5174` (needed by the FE end-to-end tests).

5. **First admin, via the DB.** There is no endpoint to create admins (yet): every registration creates a `Customer`. Register the user from the FE (or Swagger), then grant the role:

   ```sql
   INSERT INTO AspNetUserRoles (UserId, RoleId)
   SELECT u.Id, r.Id
   FROM AspNetUsers u, AspNetRoles r
   WHERE u.Email = 'your@email.com' AND r.Name = 'Admin';
   ```

   Then log in again: the role is in the token.

6. **Front end:** follow the [FE README](https://github.com/Anacronistico00/vtinformatica_fe#readme) (`.env.local` with `VITE_API_URL=https://localhost:7286`).

## Tests

```bash
dotnet test VTInformatica.sln
```

Tests use in-memory SQLite and a test JWT key: no setup. The end-to-end tests (real browser + BE + SQL Server) live in the FE repo: see the E2E section of its README.

## Before / after

Numbers are **recomputed from the tags** (clean worktree per tag, `dotnet build --no-incremental`, `dotnet test`, `git diff --shortstat v1-epicode <tag>`; on the FE side `npm ci`, `npm run lint`, `npm test`, `npm run build`, `npm audit`), not copied from reports. The FE only has the tags `v1-epicode` and `v2-m4` (all its changes belong to M4), so the middle columns have no FE measurement.

| | `v1-epicode` | `v2-m1` | `v2-m2` | `v2-m3` | `v2-m4-be` / FE `v2-m4` |
|---|---:|---:|---:|---:|---:|
| BE tests (xUnit) | 0 | 42 | 54 | 240 | **243** |
| FE tests (Vitest) | 0 | – | – | – | **93** (+ 8 E2E flows) |
| `dotnet build` warnings (Release, clean) | 114 | 112 | 110 | **0** | **0** |
| FE lint (`npm run lint`) | 5 problems (2 errors, 3 warnings) | – | – | – | **0** |
| FE build warnings | 1 (chunk > 500 kB) | – | – | – | 1 (unchanged) |
| FE `npm audit` | 16 (12 high, 2 moderate, 2 low) | – | – | – | **0** |
| CI | none | BE build + test | same | same | BE: build + test; FE: lint + test + build |
| BE `.cs` files (tracked) | 96 | 108 | 117 | 174 | 176 |
| FE `.js/.jsx` files in `src/` | 52 | – | – | – | 86 |
| Diff vs `v1-epicode` (BE) | – | 27 files, +1620 −12 | 41 files, +3520 −93 | 172 files, +14558 −1459 | 175 files, +14720 −1457 |
| Diff vs `v1-epicode` (FE) | – | – | – | – | 87 files, +8418 −1505 |

Notes:

- The 114 warnings at `v1-epicode` were recounted on the tag; the M3d report quoted 109 because it was measured on a different commit. Almost all are nullability (`CS8618`, `CS8604`, …); brought to 0 without turning checks off.
- Diff lines include tests and migrations (most of the growth).
- The 8 E2E flows were not re-run for this table: they only run locally, not in CI.
- The FE CI does not run the E2E tests (it would need SQL Server on the runner).

Audit items closed, per tag (details in [`docs/AUDIT-prima.md`](docs/AUDIT-prima.md), written in Italian):

- **`v2-m1`**: test project + CI; `Customer` as the default role (every registration used to create an Admin); IDOR on cart and orders; cancel/restore order without ownership check; `UpdateUser` without `[Authorize]`; JWT key out of the repo; restricted CORS.
- **`v2-m2`**: historical price on `OrderItem`; transactional checkout with atomic stock decrement; quantity validation (400/409/404).
- **`v2-m3`**: global error handler + `ProblemDetails` with `code`; centralised mapping; `AsNoTracking`, pagination (`X-Total-Count`) and order queries; product soft delete; account reactivation; login without user enumeration; `RequireUniqueEmail`; build warnings 110 → 0.
- **`v2-m4-be`**: `GET /Review/me` for the FE (+3 tests).
- **FE `v2-m4`**: single `apiClient`, `VITE_API_URL`, token in one place, 401 → logout, no emails in URLs (`/me`), tests and CI, lint and `npm audit` at 0, paginated back office with `expectedQuantity`.

## How I worked with AI

I used Claude Code for most of the code; decisions and checks stayed with me. Concretely:

- **The initial audit as the source of truth.** Before touching code I had an audit produced and reviewed ([`docs/AUDIT-prima.md`](docs/AUDIT-prima.md)): problems ranked by severity plus starting metrics. The baseline is never rewritten; each milestone appends a status section (closed / moved / out of scope).
- **Milestones and slices.** Work is split into milestones (M0–M4), each into small slices (M3a1, M3a2, M3b, M3c1, M3c2, M3d, M4a–M4g): one task, one branch, small commits, PR into `develop` with green CI, `main`/`master` only at the end of a milestone with an annotated tag.
- **Plan mode and plan review.** For every non-trivial slice the agent proposed a plan (`docs/PIANO-*.md`) and I corrected it before giving the go-ahead. The rules live in `CLAUDE.md`: plan before code, minimal changes, no unrequested rewrites, no AI attribution in commits or PRs.
- **Test-first, red commit → fix.** For every flaw or bug: first a failing test (separate commit, `test: red - …`), then the minimal fix. The git history shows the pairs.
- **Manual migration checks on SQL Server.** The tests run on SQLite, which is not SQL Server. For migrations: backup with `RESTORE VERIFYONLY`, `database update`, `Down` and `Up` on a copy with real data, counts before and after (`AspNetUserRoles`, products, orders).
- **Moving to autonomy with a deny list.** In the first slices every step was approved. Once rules and tests were solid enough, the agent worked autonomously within explicit limits: a deny list in `.claude/settings.json` (force push, `git reset --hard`, `git filter-repo`, `rm -rf`, reading `.env*` and secrets) and the `CLAUDE.md` rules (no direct push or merge to `main`/`develop`, everything goes through a PR).
- **E2E tests that found a real bug.** Unit tests with a simulated network did not see it: Playwright against the real BE and DB exposed the NavBar crash. When Redux persistence dropped an expired slice (expired token, cart idle for over an hour), redux-persist's `autoMergeLevel1` copied `undefined` over the initial state and the app crashed on startup. Fixed test-first (red commit, then fix).

What the AI did not replace: deciding what is in scope, checking the numbers, trying migrations on the real DB, reading the diff before merging.

## Known limits and next steps

From the audit ("out of scope" sections and future milestones):

- **Tests against a real SQL Server in CI.** BE tests use in-memory SQLite; migrations and concurrency on the real DB are checked by hand. Next step: Testcontainers (or an `mssql` container in GitHub Actions) for the BE and for the FE E2E tests, which currently run locally only.
- **SuperAdmin and email.** Admins are currently created by hand in the DB. Planned: a `SuperAdmin` role, `POST /Account/admins` with an email invitation (single-use, expiring Identity token), `POST /Account/set-password`, protection of the last SuperAdmin. Prerequisite: an `IEmailSender` (Mailpit/smtp4dev in development, SMTP in production); the same infrastructure enables "forgot password".
- **Email → id.** The `Cart/{email}` and `Order/user/{email}` routes remain as `[Obsolete]` next to the `/me` routes: they should be removed.
- **httpOnly cookie.** The JWT sits in `localStorage` (via redux-persist): an XSS can read it. The fix is an httpOnly cookie + refresh token, which needs BE changes. A CSP must be set as an HTTP header at deploy time.
- **Seller in the back office.** The BE lets `Seller` manage products and categories; the FE back office is `Admin`-only.

## Contact

- 📧 viktoryturiaci2323@gmail.com
- 💼 [Vittorio Turiaci](https://www.linkedin.com/in/vittorio-turiaci-2646a9333/)
