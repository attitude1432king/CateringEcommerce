# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**CateringEcommerce** is a multi-tenant catering event management platform built with ASP.NET Core Web API backend and React + Vite frontend. The system handles three main user roles: Users (customers), Partners (catering business owners), and Admins, plus Supervisors for event management.

## Solution Architecture

### Layer Structure (Clean Architecture)

```
CateringEcommerce.API/          # Web API Controllers, Entry Point
├── Controllers/
│   ├── Admin/                  # Admin management endpoints
│   ├── Owner/                  # Partner/Owner business management
│   ├── User/                   # Customer-facing endpoints
│   ├── Supervisor/             # Event supervisor endpoints
│   ├── Payment/                # Payment gateway integrations
│   └── Common/                 # Shared endpoints (auth, locations)
├── Hubs/                       # SignalR hubs for real-time features
└── Program.cs                  # DI registration, middleware configuration

CateringEcommerce.BAL/          # Business Logic Layer
├── Base/                       # Feature-specific business logic
│   ├── Admin/                  # Admin operations
│   ├── Owner/                  # Partner operations
│   ├── User/                   # Customer operations
│   ├── Supervisor/             # Supervisor operations
│   ├── Order/                  # Order processing
│   └── Payment/                # Payment processing
├── Common/                     # Shared repositories
├── Configuration/              # Email, SMS, Token services
├── DatabaseHelper/             # Database access layer
├── Notification/               # Notification microservice
└── Services/                   # Background jobs, integrations

CateringEcommerce.Domain/       # Domain Models & Interfaces
├── Interfaces/                 # Repository and service contracts
├── Models/                     # DTOs and domain models
└── Enums/                      # Shared enumerations

CateringEcommerce.Web/          # Legacy Web Project (contains Frontend)
└── Frontend/                   # React + Vite SPA
    ├── src/
    │   ├── pages/              # Page components
    │   ├── components/         # Reusable UI components
    │   ├── contexts/           # React context providers
    │   ├── services/           # API client modules
    │   ├── router/             # Route configurations
    │   └── utils/              # Helper functions
    └── public/                 # Static assets

Database/                       # SQL migration scripts
```

## Development Commands

### Backend (.NET)

**Build the solution:**
```bash
dotnet build CateringEcommerce.sln
```

**Run the API (from project root):**
```bash
dotnet run --project CateringEcommerce.API/CateringEcommerce.API.csproj
```

**Run with watch mode:**
```bash
dotnet watch --project CateringEcommerce.API/CateringEcommerce.API.csproj
```

**Test specific endpoint:**
```bash
# API runs on https://localhost:44368 by default
curl https://localhost:44368/api/[endpoint]
```

### Frontend (React + Vite)

**Install dependencies:**
```bash
cd CateringEcommerce.Web/Frontend
npm install
```

**Run development server:**
```bash
npm run dev
# Runs on http://localhost:5173
```

**Build for production:**
```bash
npm run build
```

**Lint:**
```bash
npm run lint
```

### Database

**Connection String Location:**
- `CateringEcommerce.API/appsettings.json` → `ConnectionStrings:DefaultConnection`
- Database: SQL Server (trusted connection)

**Execute migration scripts:**
```bash
# Use SQL Server Management Studio or Azure Data Studio
# Execute scripts from Database/ folder in order
```

**Key migration files:**
- `mastersql.sql` - Core schema
- `*_Migration.sql` - Feature-specific migrations
- `*_StoredProcedures.sql` - Stored procedures

## Key Architectural Patterns

### 1. Dependency Injection

All services are registered in `CateringEcommerce.API/Program.cs`. When adding new features:

1. Define interface in `CateringEcommerce.Domain/Interfaces/`
2. Implement in `CateringEcommerce.BAL/Base/` or `CateringEcommerce.BAL/Common/`
3. Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IYourInterface, YourImplementation>();
```

### 2. Database Access Pattern

All database operations go through `IDatabaseHelper` interface:

```csharp
// Inject IDatabaseHelper
private readonly IDatabaseHelper _db;

// Execute stored procedure
var result = await _db.ExecuteQueryAsync<YourModel>(
    "sp_YourProcedure",
    new SqlParameter[] {
        new SqlParameter("@Param", value)
    },
    CommandType.StoredProcedure
);
```

**Important:** Prefer stored procedures over inline SQL for complex operations.

### 3. Authentication & Authorization

**JWT Token Flow:**
- Tokens stored in httpOnly cookies (`adminToken`, `authToken`, `supervisorToken`)
- Token validation in `Program.cs` reads from cookies OR Authorization header
- Rate limiting configured per endpoint type (login, OTP, API)

**Role-Based Access:**
- Admin: Full RBAC system with permissions (see `t_sys_admin_roles`, `t_sys_admin_permissions`)
- Owner/Partner: Partner-specific data isolation by `CateringOwnerId`
- User: Standard user authentication
- Supervisor: Event-specific assignments

### 4. Real-Time Features

**SignalR Hubs:**
- `NotificationHub` - Real-time notifications (`/hubs/notifications`)
- `SupervisorTrackingHub` - Live tracking for supervisors (`/hubs/supervisor-tracking`)

### 5. Background Jobs (Hangfire)

**Dashboard:** `https://localhost:44368/hangfire`

**Configured Jobs:**
- Payment reminders (daily 10 AM)
- Auto-lock guest count (hourly)
- Auto-lock menu (hourly)
- Commission transition notices (daily 9 AM)
- Escalate stale complaints (every 2 hours)

**Add new job in Program.cs:**
```csharp
RecurringJob.AddOrUpdate<YourService>(
    "job-name",
    x => x.YourMethod(),
    Cron.Daily(10)
);
```

## Database Conventions

### Table Naming
- System tables: `t_sys_*` (e.g., `t_sys_user`, `t_sys_orders`)
- Catering-specific: `t_sys_catering_*`
- Availability: `t_catering_availability_*`
- Lookup/Master data: `t_sys_*_master` or `t_sys_*_type`

### Key Relationships
- Users: `t_sys_user` (UserId as PK)
- Partners: `t_sys_catering_owner` (CateringOwnerId as PK)
- Orders: `t_sys_orders` → `t_sys_order_items` → `t_sys_order_payments`
- Menu: `t_sys_fooditems` and `t_sys_catering_packages`
- Reviews: `t_sys_catering_review`

### Terminology Migration
**Note:** The system recently migrated from "Vendor" to "Partner" terminology. Some older tables/columns still use "Vendor" but new code should use "Partner" consistently. See `Vendor_To_Partner_Migration.sql` for details.

## Frontend Architecture

### Routing Structure
- User routes: `/` (home), `/caterings`, `/checkout`, `/orders`, `/profile`
- Admin routes: `/admin/*` (protected by `AdminAuthContext`)
- Owner routes: `/owner/*` (protected by `OwnerAuthContext`)
- Supervisor routes: `/supervisor/*` (protected by `SupervisorAuthContext`)

### API Client Pattern
All API calls are centralized in `src/services/`:
- `userApi.js` - Customer endpoints
- `ownerApi.js` - Partner endpoints
- `adminApi.js` - Admin endpoints
- Each exports configured axios instances with interceptors

### State Management
- Auth: React Context (`AdminAuthContext`, `AuthContext`, etc.)
- Permissions: `PermissionContext` for RBAC
- No global state library (Redux/Zustand) - uses React Context + local state

## Security Features

### Implemented Protections
- **CSRF Protection:** Anti-forgery tokens via `X-CSRF-TOKEN` header
- **Rate Limiting:** Per-endpoint limits (login: 3/15min, API: 100/min)
- **Security Headers:** CSP, X-Frame-Options, HSTS, etc. (see Program.cs middleware)
- **HttpOnly Cookies:** JWT tokens not accessible to JavaScript
- **2FA/OTP:** Twilio-based verification (`TwoFactorAuthService`)
- **OAuth:** Google/Facebook login support (`OAuthRepository`)

### Common Vulnerabilities Addressed
- SQL Injection: Parameterized queries via `SqlParameter`
- XSS: DOMPurify on frontend, CSP headers
- CSRF: Anti-forgery middleware
- Clickjacking: X-Frame-Options: DENY

## Configuration Files

### Backend
**appsettings.json** contains:
- `ConnectionStrings:DefaultConnection` - Database
- `Jwt` - Token configuration (Key, Issuer, Audience)
- `EmailSettings` - SMTP configuration
- `Twilio` - SMS/OTP configuration
- `RazorpaySettings` - Payment gateway
- `RabbitMQ` - Message queue (optional)

**Important:** Never commit real credentials. Use User Secrets for development:
```bash
dotnet user-secrets set "Jwt:Key" "your-secret-key"
```

### Frontend
**No .env file currently.** API base URL is hardcoded in service files. Consider creating `.env`:
```
VITE_API_BASE_URL=https://localhost:44368
```

## Notable Features

### Financial Strategy System
- **Auto-locking:** Guest count locks 5 days before event, menu locks 3 days before
- **Cancellation Policy:** Tiered refunds based on days before event (see `sp_CalculateCancellationRefund`)
- **Partner Commissions:** 8%-15% ladder based on monthly revenue (see `t_sys_vendor_partnership_tiers`)
- **Security Deposits:** ₹25,000 deposit system for partners (see `t_sys_vendor_security_deposits`)
- **Complaint Management:** Automated refund calculations with fraud detection

### Sample Tasting Flow
Partners can offer sample deliveries before event bookings. Tables: `t_sys_sample_delivery`, stored procedures in `Sample_Tasting_Complete_Schema.sql`.

### Supervisor Management
Event supervisors can be assigned to orders for on-site coordination. Includes photo validation, live tracking, and payment system.

### Split Payments
Orders support multiple payment stages (advance, balance, post-event). See `t_sys_order_payment_stages`.

## Common Issues & Solutions

### Build Errors
If controllers fail to load with `ReflectionTypeLoadException`:
1. Check all referenced assemblies are built
2. Verify `using` statements match actual namespaces
3. Ensure all DI registrations exist in `Program.cs`

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists (run `mastersql.sql` if new)

### Frontend CORS Errors
- API must be running on `https://localhost:44368`
- Frontend must be on `http://localhost:5173`
- Check `AllowReactApp` CORS policy in Program.cs

### JWT Token Issues
- Check token expiry (`ExpireMinutes` in appsettings.json)
- Verify cookie names match (`adminToken`, `authToken`, `supervisorToken`)
- Ensure `withCredentials: true` in axios calls

## Testing Guidance

### Manual Testing
1. **Admin Portal:** Login at `/admin` with admin credentials from `t_sys_admin`
2. **User Flow:** Register → Browse caterers → Add to cart → Checkout → Track order
3. **Owner Flow:** Register as partner → Complete 5-step onboarding → Manage menu/availability
4. **Payment Testing:** Use Razorpay test keys and test card numbers

### Sample Test Data
Execute `Database/Financial_Strategy_TestData.sql` for realistic test data including:
- Users, partners, orders
- Sample payments and cancellations
- Commission tier progressions

## External Dependencies

### Backend NuGet Packages
- Hangfire.SqlServer - Background jobs
- Twilio - SMS/OTP
- Razorpay (implied) - Payments
- SignalR - Real-time communication
- RabbitMQ.Client - Message queuing (optional)

### Frontend npm Packages
- react-router-dom - Routing
- axios - HTTP client
- react-hook-form + zod - Form validation
- chart.js + react-chartjs-2 - Analytics charts
- framer-motion - Animations
- lucide-react - Icons
- dompurify - XSS protection

## Migration from Vendor to Partner

Recent terminology change in the codebase:
- Database: Some tables still use `vendor` (e.g., `t_sys_vendor_security_deposits`)
- Code: Use "Partner" and "Owner" in new code
- Frontend: Consistently uses "Partner"

When adding features:
- Use "Partner" in C# classes, models, and interfaces
- Use "Owner" for the business owner role context
- Accept legacy "vendor" in database column names

## Documentation Files

For deeper implementation details, see:
- `PROJECT_COMPLETION_REPORT.md` - Overall project status
- `IMPLEMENTATION_STATUS.md` - Financial strategy features
- `*_IMPLEMENTATION_COMPLETE.md` - Feature-specific documentation
- `SECURITY_*.md` - Security implementation details
- `HANGFIRE_SETUP_GUIDE.md` - Background jobs setup
- `RABBITMQ_SETUP_GUIDE.md` - Message queue setup

## Development Workflow

1. **Create database migration** in `Database/` folder
2. **Define models** in `CateringEcommerce.Domain/Models/`
3. **Create interface** in `CateringEcommerce.Domain/Interfaces/`
4. **Implement repository** in `CateringEcommerce.BAL/Base/` or `Common/`
5. **Register DI** in `Program.cs`
6. **Create controller** in `CateringEcommerce.API/Controllers/`
7. **Build frontend API client** in `src/services/`
8. **Create React components** in `src/components/` and `src/pages/`
9. **Test end-to-end** with real data
