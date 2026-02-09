# ⚠️ SECURITY - IMMEDIATE ACTION REQUIRED

## CRITICAL SECURITY FIXES APPLIED - DEPLOYMENT CHECKLIST

**Date**: February 6, 2026
**Priority**: CRITICAL
**Timeline**: Deploy within 24-48 hours

---

## 🔴 CRITICAL - MUST VERIFY BEFORE DEPLOYMENT

### 1. Database Schema Updates Required

#### AdminAuthRepository - Add GetAdminByUsername Method

The updated AdminAuthController now calls `repository.GetAdminByUsername(request.Username)`.
**Verify this method exists** in your AdminAuthRepository class.

**Expected Method Signature**:
```csharp
public class AdminAuthRepository
{
    public AdminModel GetAdminByUsername(string username)
    {
        // SQL: SELECT * FROM Admin_Users WHERE Username = @Username
        // Must return AdminModel with PasswordHash property
    }
}
```

**If method doesn't exist**, add it now:
```csharp
public AdminModel GetAdminByUsername(string username)
{
    string sql = "SELECT AdminId, Username, PasswordHash, Email, FullName, Role, ProfilePhoto, " +
                 "IsActive, FailedLoginAttempts, IsLocked, LockedUntil, CreatedDate, LastLogin " +
                 "FROM Admin_Users WHERE Username = @Username AND IsActive = 1";

    var parameters = new Dictionary<string, object> { { "@Username", username } };
    var result = _dbHelper.ExecuteReader(sql, parameters);

    // Map to AdminModel
    return MapToAdminModel(result.FirstOrDefault());
}
```

---

#### Admin_Users Table - Add PasswordHash Column (if missing)

The AdminModel now includes `PasswordHash` property for BCrypt support.

**SQL Migration Script**:
```sql
-- Check if PasswordHash column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Admin_Users') AND name = 'PasswordHash')
BEGIN
    -- If Password column exists, rename it to PasswordHash
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Admin_Users') AND name = 'Password')
    BEGIN
        EXEC sp_rename 'Admin_Users.Password', 'PasswordHash', 'COLUMN';
        PRINT 'Renamed Password column to PasswordHash';
    END
    ELSE
    BEGIN
        -- Add PasswordHash column if neither exists
        ALTER TABLE Admin_Users ADD PasswordHash NVARCHAR(255) NOT NULL DEFAULT '';
        PRINT 'Added PasswordHash column';
    END
END
GO

-- Optional: Add column to track hash type for migration monitoring
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Admin_Users') AND name = 'PasswordHashType')
BEGIN
    ALTER TABLE Admin_Users ADD PasswordHashType VARCHAR(10) DEFAULT 'SHA256';
    PRINT 'Added PasswordHashType column for migration tracking';
END
GO

-- Mark existing passwords as SHA256
UPDATE Admin_Users
SET PasswordHashType = 'SHA256'
WHERE PasswordHashType IS NULL OR PasswordHashType = '';
GO
```

---

### 2. Dependency Installation Verification

#### BCrypt.Net-Next Package

**Status**: ✅ Already added via `dotnet add package`

**Verify Installation**:
```bash
cd D:\Pankaj\Project\CateringEcommerce\CateringEcommerce.API
dotnet list package | findstr BCrypt
```

**Expected Output**:
```
> BCrypt.Net-Next    4.0.3
```

**If not installed**, run:
```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet restore
```

---

### 3. Connection String Security

**File**: `AdminPartnerRequestsController.cs` now requires IConfiguration injection.

**Verify** in `Program.cs` or `Startup.cs`:
```csharp
// Ensure IConfiguration is registered (usually done by default)
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Connection string should NOT be hardcoded
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

**⚠️ NEVER commit connection strings to source control!**

**Recommended**: Use Azure Key Vault or Environment Variables
```csharp
// appsettings.json - DO NOT COMMIT with real values
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User Id=...;Password=#{DB_PASSWORD}#;"
  }
}

// Production - Use Azure Key Vault
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

### 4. ICurrentUserService Registration

**File**: `PaymentController.cs` now requires `ICurrentUserService`.

**Verify** this is registered in your DI container:
```csharp
// Program.cs or Startup.cs
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

**If missing**, you'll get runtime error:
```
Unable to resolve service for type 'CateringEcommerce.Domain.Interfaces.Common.ICurrentUserService'
```

**Check existing implementation**:
```bash
# Find the CurrentUserService implementation
cd D:\Pankaj\Project\CateringEcommerce
grep -r "class CurrentUserService" .
```

---

### 5. OTP Service Methods Must Be Implemented

**File**: `AuthenticationController.cs` now calls:
```csharp
isValid = _emailService.VerifyOtp(request.Value, request.Otp);
isValid = _smsService.VerifyOtp(request.Value, request.Otp);
```

**⚠️ CRITICAL**: These were previously commented out!

**Verify** these methods exist and are properly implemented:

**IEmailService Interface**:
```csharp
public interface IEmailService
{
    void StoreOtp(string email, string otp);
    bool VerifyOtp(string email, string otp); // Must exist!
    Task SendOtpAsync(string email, string otp);
}
```

**ISmsService Interface**:
```csharp
public interface ISmsService
{
    void StoreOtp(string phone, string otp);
    bool VerifyOtp(string phone, string otp); // Must exist!
    void SendOtp(string phone);
}
```

**If methods don't exist**, implement them:
```csharp
// Example implementation using in-memory cache (for development)
// Production: Use Redis or database
private static Dictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

public bool VerifyOtp(string identifier, string otp)
{
    if (_otpStore.TryGetValue(identifier, out var stored))
    {
        if (stored.Expiry > DateTime.UtcNow && stored.Otp == otp)
        {
            _otpStore.Remove(identifier); // One-time use
            return true;
        }
        // Remove expired OTPs
        if (stored.Expiry <= DateTime.UtcNow)
            _otpStore.Remove(identifier);
    }
    return false;
}
```

---

## 🔴 PRE-DEPLOYMENT TESTING CHECKLIST

### Authentication Tests

Run these tests **BEFORE** deploying to production:

#### Test 1: Admin Login with BCrypt
```bash
# Create a test admin account with BCrypt hash
# Use online BCrypt generator or:
```
```csharp
var testPassword = "TestP@ssw0rd123";
var bcryptHash = BCrypt.Net.BCrypt.HashPassword(testPassword, 12);
// Insert into database: PasswordHash = bcryptHash
```

**Test**:
1. POST `/api/admin/auth/login`
2. Body: `{ "username": "testadmin", "password": "TestP@ssw0rd123" }`
3. ✅ Expected: 200 OK with JWT token
4. ❌ If fails: Check repository method and database column

---

#### Test 2: OTP Verification
```bash
# Test OTP bypass is fixed
```

**Test**:
1. POST `/api/Common/Auth/send-otp`
2. Body: `{ "type": "email", "value": "test@example.com", "role": "User" }`
3. Receive OTP (check logs if email not configured)
4. POST `/api/Common/Auth/verify-otp` with **WRONG OTP**
5. ✅ Expected: 400 Bad Request "Invalid or expired OTP"
6. ❌ If succeeds: OTP verification still bypassed!

**Critical**: If Test 2 fails, DO NOT DEPLOY!

---

#### Test 3: IDOR Protection
```bash
# Test payment endpoint ownership verification
```

**Test**:
1. Login as Owner A (get JWT token)
2. GET `/api/payment/partner/payout-requests/999` (use another owner's ID)
3. ✅ Expected: 403 Forbidden
4. ❌ If returns data: IDOR vulnerability still exists!

---

#### Test 4: Account Lockout
```bash
# Test brute force protection
```

**Test**:
1. POST `/api/admin/auth/login` with wrong password (5 times)
2. 6th attempt: ✅ Expected: "Account locked due to multiple failed attempts"
3. Wait 30 minutes or manually unlock in database
4. Retry: ✅ Expected: Login succeeds

---

### Exception Handling Tests

#### Test 5: No Sensitive Data in Errors
```bash
# Test exception message sanitization
```

**Test**:
1. Trigger an error (e.g., invalid database connection)
2. Check API response
3. ✅ Expected: "An internal error occurred. Please try again later."
4. ❌ Should NOT contain: stack traces, SQL queries, file paths

---

## 🔴 POST-DEPLOYMENT MONITORING

### Day 1 Checklist

- [ ] Monitor failed login attempts (should see retries, then lockouts)
- [ ] Check OTP verification logs (success rate should improve)
- [ ] Review error logs for any BCrypt-related errors
- [ ] Verify no exception details in API responses
- [ ] Monitor application performance (BCrypt adds ~250ms per login)

### Week 1 Checklist

- [ ] Analyze security logs for unauthorized access attempts
- [ ] Track BCrypt vs SHA256 usage (gradual migration)
- [ ] Review customer support tickets for login issues
- [ ] Verify OTP delivery success rate
- [ ] Check for any authentication bypass attempts

---

## 🔴 ROLLBACK PLAN

If critical issues occur after deployment:

### Immediate Rollback Steps

1. **Revert AdminAuthController**:
```bash
git revert <commit-hash>
git push origin main
```

2. **Emergency OTP Bypass** (TEMPORARY - for debugging only):
```csharp
// In AuthenticationController.cs - EMERGENCY ONLY
private bool IsEmergencyMode = true; // Set via config
if (IsEmergencyMode)
{
    isValid = request.Otp == "999999"; // Emergency OTP
    _logger.LogWarning("EMERGENCY MODE - OTP bypass active!");
}
```

3. **Disable Account Lockout** (TEMPORARY):
```csharp
// In AdminAuthController.cs
private const int MAX_FAILED_ATTEMPTS = int.MaxValue; // Effectively disabled
```

**⚠️ WARNING**: Only use these in extreme emergencies. Re-enable security ASAP!

---

## 🔴 MIGRATION PATH FOR EXISTING USERS

### SHA256 to BCrypt Migration Strategy

**Phase 1** (Current): Both hash types accepted
```
- New passwords → BCrypt
- Existing passwords → SHA256 (still work)
- Gradual migration on next login
```

**Phase 2** (Week 2): Rehash on login
```csharp
// After successful SHA256 verification
if (!storedHash.StartsWith("$2"))
{
    // Rehash with BCrypt
    var newHash = BCrypt.Net.BCrypt.HashPassword(password, 12);
    repository.UpdatePasswordHash(adminId, newHash);
    _logger.LogInformation("Password migrated to BCrypt for user {UserId}", adminId);
}
```

**Phase 3** (Month 1): Force password reset for remaining SHA256 accounts
```sql
-- Find users still using SHA256
SELECT AdminId, Username, Email, LastLogin
FROM Admin_Users
WHERE PasswordHashType = 'SHA256'
  AND LastLogin < DATEADD(day, -30, GETDATE());

-- Send password reset emails
```

**Phase 4** (Month 2): Remove SHA256 support entirely

---

## 🔴 CRITICAL CONFIGURATION CHECKS

### appsettings.json Verification

**Required Settings**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{FROM_KEYVAULT}#"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "CateringEcommerce": "Information"
    }
  },
  "Jwt": {
    "SecretKey": "#{FROM_KEYVAULT}#",
    "Issuer": "CateringEcommerce",
    "Audience": "CateringEcommerceUsers",
    "ExpiryMinutes": 60
  },
  "Security": {
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 30,
    "OTPExpiryMinutes": 5,
    "BCryptWorkFactor": 12
  }
}
```

**⚠️ NEVER commit these with real values to source control!**

---

## 🔴 CONTACT & ESCALATION

### If Deployment Issues Occur

**Severity 1** (Production down):
- Rollback immediately
- Contact: Security Team Lead
- Escalate to: CTO

**Severity 2** (Authentication broken):
- Enable emergency OTP bypass (999999)
- Fix and redeploy within 2 hours
- Contact: Backend Team Lead

**Severity 3** (Performance issues):
- Monitor BCrypt performance
- Consider reducing work factor temporarily
- Contact: DevOps Team

---

## ✅ DEPLOYMENT SUCCESS CRITERIA

Deploy is considered successful when:

- ✅ All 5 pre-deployment tests pass
- ✅ OTP verification works correctly (no bypass)
- ✅ BCrypt authentication succeeds
- ✅ SHA256 backward compatibility works
- ✅ IDOR protection verified
- ✅ No exception details leaked in responses
- ✅ Account lockout working
- ✅ No increase in failed authentication rate
- ✅ Application performance within acceptable limits (<500ms login)

---

**Document Version**: 1.0
**Last Updated**: February 6, 2026
**Mandatory Review Before Deployment**: YES
**Deployment Approval Required**: CTO/Security Lead
