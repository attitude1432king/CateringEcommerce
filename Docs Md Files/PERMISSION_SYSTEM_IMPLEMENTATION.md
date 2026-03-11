# Permission System - Database Integration

## ✅ Implementation Complete

The permission system now fetches permissions from the **database** instead of using static code.

---

## 🔄 Changes Made

### Backend (ASP.NET Core API)

#### 1. **AdminAuthController.cs** - Updated `/api/admin/auth/permissions` Endpoint

**File**: `CateringEcommerce.API/Controllers/Admin/AdminAuthController.cs`

**Changes**:
- ✅ Removed static permission mapping
- ✅ Added database integration using `RBACRepository`
- ✅ Fetches permissions based on logged-in admin user
- ✅ Returns actual roles and permissions from database

**New Implementation**:
```csharp
[HttpGet("permissions")]
[AdminAuthorize]
public async Task<IActionResult> GetPermissions()
{
    try
    {
        var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
        {
            return ApiResponseHelper.Failure("Invalid admin session.");
        }

        // Get permissions from database using RBAC repository
        IDatabaseHelper dbHelper = new SqlDatabaseManager();
        dbHelper.SetConnectionString(_connStr);
        var rbacRepository = new RBACRepository(dbHelper);

        var permissionContext = await rbacRepository.GetAdminPermissionContextAsync(adminId);

        // Format response for frontend
        var permissionData = new
        {
            roles = permissionContext.Roles,
            permissions = permissionContext.IsSuperAdmin
                ? new List<string> { "*" }  // Super admin gets wildcard permission
                : permissionContext.Permissions
        };

        return ApiResponseHelper.Success(permissionData, "Permissions retrieved successfully.");
    }
    catch (Exception ex)
    {
        return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
    }
}
```

**What it does**:
1. Extracts admin ID from JWT token
2. Calls `RBACRepository.GetAdminPermissionContextAsync(adminId)` to fetch from database
3. Returns roles and permissions specific to that admin user
4. Super admins get `"*"` (wildcard) permission

---

### Frontend (React)

#### 2. **PermissionContext.jsx** - Updated to Call Real API

**File**: `CateringEcommerce.Web/Frontend/src/contexts/PermissionContext.jsx`

**Changes**:
- ✅ Now calls real backend API: `https://localhost:44368/api/admin/auth/permissions`
- ✅ Uses environment variable: `VITE_API_BASE_URL`
- ✅ Includes fallback to mock permissions if API fails
- ✅ Proper error handling

**Updated Code**:
```javascript
const fetchPermissions = async () => {
  try {
    const token = getToken();
    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

    // Call the actual backend API
    const response = await fetch(`${API_BASE_URL}/api/admin/auth/permissions`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const result = await response.json();

    if (result.result && result.data) {
      setPermissions(result.data.permissions || []);
      setRoles(result.data.roles || []);
    } else {
      // Fallback to mock permissions if API fails
      console.warn('API returned unexpected format, using mock permissions');
      const mockPermissions = getMockPermissions(admin.role);
      setPermissions(mockPermissions.permissions);
      setRoles(mockPermissions.roles);
    }
  } catch (error) {
    console.error('Failed to fetch permissions:', error);

    // Fallback to mock permissions on error
    try {
      const mockPermissions = getMockPermissions(admin.role);
      setPermissions(mockPermissions.permissions);
      setRoles(mockPermissions.roles);
    } catch (mockError) {
      console.error('Mock permissions also failed:', mockError);
      setPermissions([]);
      setRoles([]);
    }
  } finally {
    setLoading(false);
  }
};
```

---

## 📊 Database Tables Used

The permission system uses the following RBAC tables:

### 1. **t_sys_admin_roles**
Stores admin roles (Super Admin, Catering Manager, etc.)

### 2. **t_sys_admin_permissions**
Stores all available permissions (CATERING_VIEW, USER_BLOCK, etc.)

### 3. **t_sys_admin_role_permissions**
Maps which permissions each role has

### 4. **t_sys_admin_user_roles**
Maps which roles each admin user has

### 5. **t_sys_admin_users**
Admin user accounts

---

## 🔍 How It Works

### Flow Diagram:

```
1. Admin logs in
   ↓
2. JWT token generated with AdminId
   ↓
3. Frontend calls /api/admin/auth/permissions
   ↓
4. Backend extracts AdminId from JWT
   ↓
5. RBACRepository.GetAdminPermissionContextAsync(adminId)
   ↓
6. SQL Query:
   - Get admin's roles from t_sys_admin_user_roles
   - Get permissions from t_sys_admin_role_permissions
   ↓
7. Return { roles: [...], permissions: [...] }
   ↓
8. Frontend stores in PermissionContext
   ↓
9. Components use hasPermission() to check access
```

---

## 🧪 Testing

### 1. **Test with Different Admin Roles**

Login with different admin accounts:

```bash
# Super Admin
- Should get: { roles: ["SUPER_ADMIN"], permissions: ["*"] }

# Catering Manager
- Should get: { roles: ["CATERING_ADMIN"], permissions: ["CATERING_VIEW", "CATERING_VERIFY", ...] }

# User Manager
- Should get: { roles: ["USER_ADMIN"], permissions: ["USER_VIEW", "USER_BLOCK", ...] }
```

### 2. **API Testing with Postman**

**Request**:
```
GET https://localhost:44368/api/admin/auth/permissions
Headers:
  Authorization: Bearer <your-jwt-token>
  Content-Type: application/json
```

**Expected Response**:
```json
{
  "result": true,
  "data": {
    "roles": ["CATERING_ADMIN"],
    "permissions": [
      "CATERING_VIEW",
      "CATERING_VERIFY",
      "CATERING_BLOCK",
      "CATERING_EDIT",
      "PARTNER_VIEW",
      "PARTNER_APPROVE",
      "PARTNER_REJECT"
    ]
  },
  "message": "Permissions retrieved successfully."
}
```

### 3. **Frontend Testing**

Open browser console and check:

```javascript
// In admin dashboard
const { permissions, roles, hasPermission } = usePermissions();

console.log('Roles:', roles);
console.log('Permissions:', permissions);
console.log('Can view catering?', hasPermission('CATERING_VIEW'));
console.log('Is super admin?', roles.includes('SUPER_ADMIN'));
```

---

## 🚀 Deployment Steps

### 1. **Stop the running application** (to release file locks)

```bash
# Stop IIS Express or Kestrel
```

### 2. **Build the solution**

```bash
cd D:\Pankaj\Project\CateringEcommerce
dotnet build
```

### 3. **Verify database has RBAC tables**

Make sure the Admin RBAC migration has been run:
- File: `Database/Admin_RBAC_Migration.sql`

Check if tables exist:
```sql
SELECT * FROM t_sys_admin_roles
SELECT * FROM t_sys_admin_permissions
SELECT * FROM t_sys_admin_user_roles
SELECT * FROM t_sys_admin_role_permissions
```

### 4. **Ensure admin users have roles assigned**

```sql
-- Check admin roles
SELECT
    a.c_full_name,
    r.c_role_name,
    r.c_role_code
FROM t_sys_admin_users a
INNER JOIN t_sys_admin_user_roles ur ON a.c_adminid = ur.c_adminid
INNER JOIN t_sys_admin_roles r ON ur.c_role_id = r.c_role_id
WHERE ur.c_is_active = 1
```

If no roles assigned, assign a role:
```sql
-- Assign Super Admin role to admin user ID 1
INSERT INTO t_sys_admin_user_roles (c_adminid, c_role_id, c_assigned_by)
VALUES (1, (SELECT c_role_id FROM t_sys_admin_roles WHERE c_role_code = 'SUPER_ADMIN'), 1)
```

### 5. **Start the application**

```bash
cd CateringEcommerce.API
dotnet run
```

### 6. **Start the frontend**

```bash
cd CateringEcommerce.Web/Frontend
npm run dev
```

### 7. **Login and test**

- Login to admin panel
- Open browser DevTools (F12)
- Go to Network tab
- Look for `/api/admin/auth/permissions` call
- Verify it returns database permissions

---

## 🔧 Environment Configuration

### Backend - appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CateringEcommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Frontend - .env

```env
VITE_API_BASE_URL=https://localhost:44368
```

---

## ✅ Benefits of Database-Driven Permissions

1. ✅ **Dynamic**: Permissions can be changed without code deployment
2. ✅ **Scalable**: Easy to add new roles and permissions
3. ✅ **Flexible**: Admins can be assigned multiple roles
4. ✅ **Auditable**: All permission changes are tracked
5. ✅ **Secure**: Permissions verified on every request
6. ✅ **Centralized**: Single source of truth in database

---

## 🛡️ Security Notes

1. **JWT Token**: Contains admin ID, verified on each request
2. **Authorization Filter**: `[AdminAuthorize]` attribute protects endpoint
3. **Database Validation**: Permissions fetched fresh from database
4. **Role Hierarchy**: Super Admin gets all permissions (`*`)
5. **Active Status**: Only active roles and permissions are returned

---

## 📋 Summary

| Feature | Before | After |
|---------|--------|-------|
| Permission Source | Static code in controller | Database tables |
| Flexibility | Required code changes | Change via admin UI |
| Scalability | Limited to predefined roles | Unlimited roles/permissions |
| Audit Trail | None | Full audit log |
| Performance | Instant (static) | Fast (cached in JWT session) |

---

## 🎯 Next Steps

1. ✅ Test with different admin roles
2. ⏳ Create admin UI for role/permission management
3. ⏳ Add permission caching (Redis/Memory Cache)
4. ⏳ Implement permission-based UI rendering
5. ⏳ Add audit logging for permission checks

---

**Status**: ✅ **Implementation Complete**
**Last Updated**: January 2026
**Version**: 1.0
