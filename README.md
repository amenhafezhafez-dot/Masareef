
Masareef
Personal and small-business expense tracking. Households track income, expenses and debts; shop owners additionally track products, sales and per-shop profit.

ASP.NET Core 8 REST API in a layered architecture, SQL Server for storage, and a React single-page frontend.

Masareef.Api        HTTP layer — controllers, authorization policies, middleware
Masareef.BLL        business rules and validation
Masareef.DAL        EF Core DbContext and repositories
Masareef.Entities   models, DTOs, shared types
Masareef.Console    console client (separate from the API)
masareef-frontend   React 18 + Vite SPA
Prerequisites
.NET 8 SDK	builds and runs the API
SQL Server (Express is fine)	database
Node.js 18+	only if you want to run the frontend
Getting started
1. Clone
git clone https://github.com/amenhafezhafez-dot/Masareef.git
cd Masareef
2. Create the database
CREATE DATABASE MasareefDB COLLATE Arabic_CI_AS;
Then run the two scripts in order:

sqlcmd -S .\SQLEXPRESS -d MasareefDB -E -i Database/00_base_schema.sql
sqlcmd -S .\SQLEXPRESS -d MasareefDB -E -i Database/01_security_schema.sql
Script	Creates
00_base_schema.sql	the 11 core tables and the vw_DebtSummary view
01_security_schema.sql	Users.Role, RefreshTokens, AuditLogs
Both are idempotent — re-running them changes nothing — and each ends with a verification block that prints OK per object. Structure only, no seed data.

Skipping the second script does not stop the app from starting. It makes every login fail with Invalid column name 'Role', which leaves the whole API unusable.

3. Set the signing key
The JWT signing key is not in the repository and does not travel with it. Every environment generates its own.

Generate one:

openssl rand -base64 48
Development — stored outside the project folder:

dotnet user-secrets set "Jwt:Key" "<your key>" --project Masareef.Api
Production — set an environment variable:

Host	How
Azure App Service	Configuration → Application settings → Jwt__Key
IIS / Windows Server	System environment variables, or web.config → environmentVariables
Linux + systemd	Environment="Jwt__Key=..." in the unit file
Docker	docker run -e Jwt__Key="..."
__ (double underscore) maps to : in the configuration path, because environment variable names cannot contain a colon on every platform.

Two things worth knowing:

Keep the key stable. Changing it invalidates every issued token and signs all users out immediately.
The app refuses to start if the key is missing or under 32 characters. That is deliberate — starting with a weak key would be worse than not starting at all.
4. Configure the rest
Setting	Development	Production (environment variable)
Database	appsettings.Development.json	ConnectionStrings__DefaultConnection
Allowed frontend origins	appsettings.Development.json	Cors__Origins
Signing key	User Secrets	Jwt__Key
Startup reports every missing setting at once, along with the environment it resolved and the configuration sources it searched.

5. Run the API
dotnet run --project Masareef.Api --launch-profile https
Swagger UI is available in Development only, at https://localhost:7146/swagger.

6. Run the frontend
cd masareef-frontend
npm install
npm run dev
Opens on http://localhost:5173. Vite proxies /api/* to http://localhost:5238, so the browser sees a single origin and no CORS configuration is needed while developing. If you run the API on a different port, update the proxy target in vite.config.js.

⚠️ HTTPS is required in production
The refresh token lives in a cookie marked Secure, so the browser will not send it over plain HTTP. Deploying without TLS produces:

the cookie silently rejected, with no error anywhere
POST /api/auth/refresh always returning 401
users signed out every 15 minutes, when the access token expires
This surfaces as a mysterious bug rather than a configuration error. localhost is exempt — browsers treat it as a secure context — which is why local development works over HTTP.

In production: a real TLS certificate, and Cors__Origins values starting with https://.

Security
Area	Implementation
Authentication	JWT signed with HMAC-SHA256, 15-minute lifetime. Issuer, audience, lifetime and signature all validated
Passwords	BCrypt, with a per-user salt
Authorization	Policy-based. AdminOnly is a custom IAuthorizationRequirement plus handler reading the role claim, so the rule lives in one place
Ownership	Every query filters by the authenticated user id. The ResourceOwner policy adds an explicit gate on irreversible operations — debt write-off, where the repository method itself takes no user id
Refresh tokens	7 days, in an HttpOnly + Secure cookie scoped to /api/auth. Stored as a SHA-256 hash, never raw. Rotated on every use, and reuse of a revoked token is recorded
Logout	Revokes the token server-side rather than only clearing the cookie
Rate limiting	5 requests per minute per IP across /api/auth/*
Logging	Serilog to console and daily rolling files under Logs/
Auditing	AuditLogs table: logins, registrations, deletions, write-offs, deactivations, denied ownership attempts. A failed audit write is logged but never fails the request
Secrets	Never committed. User Secrets in development, environment variables in production
Roles
Registration through the API always produces Role = 'User'. There is no API path that creates an administrator. Promotion is a deliberate database operation:

UPDATE dbo.Users SET [Role] = 'Admin' WHERE Email = 'someone@example.com';
The role is carried inside the token, so the user must sign in again after being promoted.

API
Route	Purpose
POST /api/auth/register login refresh logout	authentication, rate limited
/api/home/expenses incomes	household income and expenses
/api/debts	debts, payments, write-off
/api/businesses	shops, their income and expenses, profit
/api/businesses/{id}/products, /api/products	inventory and sales
/api/categories	user and system categories
/api/reports	dashboard, trends, breakdowns
/api/admin/audit	audit trail — AdminOnly
All endpoints except /api/auth/* require Authorization: Bearer <token>.

/api/admin/audit returns the 200 most recent events and accepts ?userId= and ?action= filters.

Not committed
.gitignore excludes build output (bin/, obj/), IDE state (.vs/), node_modules/, and Logs/ — log files contain user email addresses and request data. Secrets are never in the repository to begin with; they live in User Secrets or environment variables.

