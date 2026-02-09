# Backend Permission API - Implementation Example

## C# Controller Implementation

### 1. Create Permission Models

```csharp
// File: CateringEcommerce.Domain/Models/Admin/PermissionModels.cs

namespace CateringEcommerce.Domain.Models.Admin
{
    public class PermissionResponse
    {
        public int AdminId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<ModulePermission> Permissions { get; set; }
    }

    public class ModulePermission
    {
        public string Module { get; set; }
        public List<string> Actions { get; set; }
    }

    // For database mapping
    public class AdminPermission
    {
        public int PermissionId { get; set; }
        public int AdminId { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
    }
}
```

---

### 2. Update Admin Login Response

```csharp
// File: CateringEcommerce.API/Controllers/Admin/AdminAuthController.cs

[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
{
    try
    {
        // Validate credentials
        var admin = await _authRepository.ValidateAdminCredentials(request.Username, request.Password);

        if (admin == null)
        {
            return Ok(new ApiResponse
            {
                Result = false,
                Message = "Invalid username or password"
            });
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(admin);

        // Return response
        return Ok(new ApiResponse
        {
            Result = true,
            Message = "Login successful",
            Data = new
            {
                Token = token,
                AdminId = admin.AdminId,
                Name = admin.Name,
                Email = admin.Email,
                Role = admin.Role // e.g., "Super Admin", "Catering Manager"
            }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Login failed");
        return StatusCode(500, new ApiResponse
        {
            Result = false,
            Message = "Internal server error"
        });
    }
}
```

---

### 3. Add Permissions Endpoint

```csharp
// File: CateringEcommerce.API/Controllers/Admin/AdminAuthController.cs

[HttpGet("permissions")]
[Authorize] // Requires JWT token
public async Task<IActionResult> GetPermissions()
{
    try
    {
        // Get admin ID from JWT token claims
        var adminIdClaim = User.FindFirst("AdminId")?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
        {
            return Unauthorized(new ApiResponse
            {
                Result = false,
                Message = "Invalid token"
            });
        }

        // Fetch admin details
        var admin = await _authRepository.GetAdminById(adminId);
        if (admin == null)
        {
            return NotFound(new ApiResponse
            {
                Result = false,
                Message = "Admin not found"
            });
        }

        // Fetch permissions from database
        var permissions = await _authRepository.GetAdminPermissions(adminId);

        // Group permissions by module
        var groupedPermissions = permissions
            .GroupBy(p => p.Module)
            .Select(g => new ModulePermission
            {
                Module = g.Key,
                Actions = g.Select(p => p.Action).ToList()
            })
            .ToList();

        // Return response
        return Ok(new ApiResponse
        {
            Result = true,
            Message = "Permissions retrieved successfully",
            Data = new PermissionResponse
            {
                AdminId = admin.AdminId,
                Name = admin.Name,
                Email = admin.Email,
                Roles = admin.Roles, // e.g., ["SUPER_ADMIN"]
                Permissions = groupedPermissions
            }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to fetch permissions");
        return StatusCode(500, new ApiResponse
        {
            Result = false,
            Message = "Internal server error"
        });
    }
}
```

---

### 4. Database Schema for Permissions

```sql
-- Admin Roles Table
CREATE TABLE AdminRoles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE, -- e.g., 'SUPER_ADMIN', 'CATERING_ADMIN'
    Description NVARCHAR(255)
);

-- Admin-Role Mapping
CREATE TABLE AdminRoleMapping (
    AdminId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedDate DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (AdminId, RoleId),
    FOREIGN KEY (AdminId) REFERENCES Admins(AdminId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AdminRoles(RoleId) ON DELETE CASCADE
);

-- Permissions Table
CREATE TABLE Permissions (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    Module NVARCHAR(50) NOT NULL, -- e.g., 'MASTER_DATA', 'USER', 'PARTNER'
    Action NVARCHAR(50) NOT NULL, -- e.g., 'VIEW', 'ADD', 'EDIT', 'DELETE'
    Description NVARCHAR(255),
    UNIQUE (Module, Action)
);

-- Role-Permission Mapping
CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES AdminRoles(RoleId) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE
);

-- OR Direct Admin-Permission Mapping (if not using roles)
CREATE TABLE AdminPermissions (
    AdminId INT NOT NULL,
    PermissionId INT NOT NULL,
    GrantedDate DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (AdminId, PermissionId),
    FOREIGN KEY (AdminId) REFERENCES Admins(AdminId) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE
);
```

---

### 5. Repository Implementation

```csharp
// File: CateringEcommerce.BAL/Common/Admin/AdminAuthRepository.cs

public async Task<List<AdminPermission>> GetAdminPermissions(int adminId)
{
    try
    {
        // Option 1: Get permissions via roles
        var query = @"
            SELECT DISTINCT
                p.PermissionId,
                @AdminId AS AdminId,
                p.Module,
                p.Action
            FROM Permissions p
            INNER JOIN RolePermissions rp ON p.PermissionId = rp.PermissionId
            INNER JOIN AdminRoleMapping arm ON rp.RoleId = arm.RoleId
            WHERE arm.AdminId = @AdminId
        ";

        using (var connection = new SqlConnection(_connectionString))
        {
            var permissions = await connection.QueryAsync<AdminPermission>(
                query,
                new { AdminId = adminId }
            );

            return permissions.ToList();
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to get permissions for admin {adminId}");
        throw;
    }
}

public async Task<List<string>> GetAdminRoles(int adminId)
{
    try
    {
        var query = @"
            SELECT r.RoleName
            FROM AdminRoles r
            INNER JOIN AdminRoleMapping arm ON r.RoleId = arm.RoleId
            WHERE arm.AdminId = @AdminId
        ";

        using (var connection = new SqlConnection(_connectionString))
        {
            var roles = await connection.QueryAsync<string>(
                query,
                new { AdminId = adminId }
            );

            return roles.ToList();
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to get roles for admin {adminId}");
        throw;
    }
}
```

---

### 6. Seed Default Permissions

```sql
-- Insert default permissions
INSERT INTO Permissions (Module, Action, Description) VALUES
-- Master Data
('MASTER_DATA', 'VIEW', 'View master data'),
('MASTER_DATA', 'ADD', 'Add new master data'),
('MASTER_DATA', 'EDIT', 'Edit master data'),
('MASTER_DATA', 'DELETE', 'Delete master data'),

-- Partner Management
('PARTNER', 'VIEW', 'View partners'),
('PARTNER', 'APPROVE', 'Approve partner requests'),
('PARTNER', 'REJECT', 'Reject partner requests'),
('PARTNER', 'BLOCK', 'Block partners'),
('PARTNER', 'EDIT', 'Edit partner details'),

-- User Management
('USER', 'VIEW', 'View users'),
('USER', 'BLOCK', 'Block/unblock users'),
('USER', 'EDIT', 'Edit user details'),
('USER', 'DELETE', 'Delete users'),

-- Order Management
('ORDER', 'VIEW', 'View orders'),
('ORDER', 'CANCEL', 'Cancel orders'),
('ORDER', 'REFUND', 'Process refunds'),

-- Earnings
('EARNINGS', 'VIEW', 'View earnings reports'),
('EARNINGS', 'EXPORT', 'Export earnings data'),

-- Reviews
('REVIEW', 'VIEW', 'View reviews'),
('REVIEW', 'MODERATE', 'Moderate reviews'),
('REVIEW', 'DELETE', 'Delete reviews'),

-- Admin Management
('ADMIN', 'VIEW', 'View admin users'),
('ADMIN', 'ADD', 'Add new admins'),
('ADMIN', 'EDIT', 'Edit admin details'),
('ADMIN', 'DELETE', 'Delete admins');

-- Create default roles
INSERT INTO AdminRoles (RoleName, Description) VALUES
('SUPER_ADMIN', 'Full system access'),
('CATERING_ADMIN', 'Manage caterings and partners'),
('USER_ADMIN', 'Manage users and reviews'),
('FINANCE_ADMIN', 'View financial reports'),
('MARKETING_ADMIN', 'Manage discounts and marketing');

-- Assign all permissions to SUPER_ADMIN
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT
    (SELECT RoleId FROM AdminRoles WHERE RoleName = 'SUPER_ADMIN'),
    PermissionId
FROM Permissions;

-- Assign specific permissions to CATERING_ADMIN
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT
    (SELECT RoleId FROM AdminRoles WHERE RoleName = 'CATERING_ADMIN'),
    PermissionId
FROM Permissions
WHERE Module IN ('PARTNER', 'ORDER')
   OR (Module = 'REVIEW' AND Action = 'VIEW');
```

---

### 7. Example API Response

**Request:**
```http
GET /api/admin/auth/permissions
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "result": true,
  "message": "Permissions retrieved successfully",
  "data": {
    "adminId": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "roles": ["SUPER_ADMIN"],
    "permissions": [
      {
        "module": "MASTER_DATA",
        "actions": ["VIEW", "ADD", "EDIT", "DELETE"]
      },
      {
        "module": "PARTNER",
        "actions": ["VIEW", "APPROVE", "REJECT", "BLOCK", "EDIT"]
      },
      {
        "module": "USER",
        "actions": ["VIEW", "BLOCK", "EDIT", "DELETE"]
      },
      {
        "module": "ORDER",
        "actions": ["VIEW", "CANCEL", "REFUND"]
      },
      {
        "module": "EARNINGS",
        "actions": ["VIEW", "EXPORT"]
      },
      {
        "module": "REVIEW",
        "actions": ["VIEW", "MODERATE", "DELETE"]
      },
      {
        "module": "ADMIN",
        "actions": ["VIEW", "ADD", "EDIT", "DELETE"]
      }
    ]
  }
}
```

---

### 8. JWT Token Configuration

Ensure JWT token includes admin ID:

```csharp
// JWT Service
public string GenerateToken(Admin admin)
{
    var claims = new[]
    {
        new Claim("AdminId", admin.AdminId.ToString()),
        new Claim(ClaimTypes.Name, admin.Name),
        new Claim(ClaimTypes.Email, admin.Email),
        new Claim(ClaimTypes.Role, admin.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

## Testing the Backend API

### Test with Postman/Insomnia:

1. **Login:**
   ```http
   POST http://localhost:44368/api/admin/auth/login
   Content-Type: application/json

   {
     "username": "admin@example.com",
     "password": "password123"
   }
   ```

2. **Get Permissions:**
   ```http
   GET http://localhost:44368/api/admin/auth/permissions
   Authorization: Bearer <token-from-login>
   ```

Expected result: Permissions grouped by module with actions.

---

## Summary

✅ **Backend API Requirements:**
- `/api/admin/auth/login` - Returns token + admin details
- `/api/admin/auth/permissions` - Returns roles + module permissions
- Database tables for roles, permissions, and mappings
- JWT token includes AdminId claim
- Permissions grouped by module

Once implemented, the frontend will automatically:
- Fetch permissions after login
- Store them in PermissionContext
- Filter menu items
- Protect routes
- Show/hide UI elements
