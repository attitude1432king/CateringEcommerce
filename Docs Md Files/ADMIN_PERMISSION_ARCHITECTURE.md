# Admin Permission Architecture - Implementation Guide

## Overview
This document explains the complete admin authentication and permission system architecture.

---

## Architecture Overview

### Provider Hierarchy
```
<AdminAuthProvider>           ← Handles login, stores admin details & token
  <PermissionProvider>        ← Fetches & manages permissions after login
    <AdminRoutes />           ← All admin routes & components
  </PermissionProvider>
</AdminAuthProvider>
```

**Location:** `src/router/Router.jsx`

---

## 1. Admin Authentication Flow

### Login Process
1. User enters credentials on `/admin/login`
2. `AdminAuthContext.login()` calls backend API `/api/admin/auth/login`
3. Backend returns:
   ```json
   {
     "result": true,
     "data": {
       "token": "jwt-token-here",
       "adminId": 123,
       "name": "John Doe",
       "email": "john@example.com",
       "role": "Super Admin"
     }
   }
   ```
4. Token and admin details stored in `localStorage`
5. Admin state updated in `AdminAuthContext`

### Authentication State
- **Stored in:** `AdminAuthContext`
- **Provides:**
  - `admin` - Admin user object
  - `loading` - Auth loading state
  - `login(username, password)` - Login function
  - `logout()` - Logout function
  - `getToken()` - Get JWT token
  - `isAuthenticated()` - Check auth status

---

## 2. Permission Management

### Permission Fetching
After successful login, `PermissionProvider` automatically:
1. Detects admin is logged in (via `AdminAuthContext`)
2. Calls `/api/admin/auth/permissions` with JWT token
3. Receives permissions in **module + action format:**
   ```json
   {
     "result": true,
     "data": {
       "roles": ["SUPER_ADMIN"],
       "permissions": [
         { "module": "MASTER_DATA", "actions": ["VIEW", "ADD", "EDIT", "DELETE"] },
         { "module": "PARTNER", "actions": ["VIEW", "APPROVE", "REJECT"] },
         { "module": "USER", "actions": ["VIEW", "BLOCK", "EDIT"] }
       ]
     }
   }
   ```
4. Stores permissions in state
5. Shows loading spinner until permissions are loaded
6. Only renders admin UI after permissions are ready

### Permission Provider API

**Import:**
```jsx
import { usePermissions } from '../contexts/PermissionContext';
```

**Available Methods:**

#### `hasPermission(module, action)`
Check if user has specific module + action permission:
```jsx
const { hasPermission } = usePermissions();

// Check if can view master data
if (hasPermission('MASTER_DATA', 'VIEW')) {
  // Show master data section
}

// Check if can edit users
if (hasPermission('USER', 'EDIT')) {
  // Show edit button
}
```

#### Legacy Format (backward compatible):
```jsx
// Old format still supported
if (hasPermission('USER_VIEW')) {
  // Works with simple string format
}
```

#### Other Available Methods:
- `hasAnyPermission(permissionList)` - Check if has ANY of the permissions
- `hasAllPermissions(permissionList)` - Check if has ALL permissions
- `hasRole(role)` - Check if has specific role
- `isSuperAdmin` - Boolean flag for super admin
- `loading` - Permission loading state
- `isLoadingPermissions` - Alternative loading state flag
- `refreshPermissions()` - Manually refresh permissions

---

## 3. Protecting Routes

### Method 1: Using ProtectedRoute Component

```jsx
import ProtectedRoute from '../components/admin/ProtectedRoute';

// Protect with module + action
<Route
  path="/master-data"
  element={
    <ProtectedRoute module="MASTER_DATA" action="VIEW">
      <MasterDataLayout />
    </ProtectedRoute>
  }
/>

// Protect with legacy permission string
<Route
  path="/users"
  element={
    <ProtectedRoute permission="USER_VIEW">
      <UserManagement />
    </ProtectedRoute>
  }
/>
```

### Method 2: Inline Permission Check

```jsx
import { usePermissions } from '../contexts/PermissionContext';
import { Navigate } from 'react-router-dom';

const MasterDataPage = () => {
  const { hasPermission } = usePermissions();

  if (!hasPermission('MASTER_DATA', 'VIEW')) {
    return <Navigate to="/admin/dashboard" replace />;
  }

  return <div>Master Data Content</div>;
};
```

---

## 4. Conditional UI Elements

### Hide/Show Buttons Based on Permissions

```jsx
const UserTable = () => {
  const { hasPermission } = usePermissions();

  return (
    <div>
      {/* Always visible */}
      <ViewButton />

      {/* Show only if can edit */}
      {hasPermission('USER', 'EDIT') && <EditButton />}

      {/* Show only if can delete */}
      {hasPermission('USER', 'DELETE') && <DeleteButton />}
    </div>
  );
};
```

### Dynamic Form Fields

```jsx
const AdminForm = () => {
  const { hasPermission } = usePermissions();

  return (
    <form>
      <input name="name" disabled={!hasPermission('ADMIN', 'EDIT')} />

      {hasPermission('ADMIN', 'DELETE') && (
        <button type="button">Delete Admin</button>
      )}
    </form>
  );
};
```

---

## 5. Menu/Sidebar Integration

The `AdminSidebar` component already implements permission-based filtering:

```jsx
const navigation = [
  {
    name: 'Dashboard',
    href: '/admin/dashboard',
    icon: LayoutDashboard
    // No permission - always visible
  },
  {
    name: 'Caterings',
    href: '/admin/caterings',
    icon: Store,
    permission: 'CATERING_VIEW'  // Only show if has this permission
  },
  {
    name: 'Master Data',
    href: '/admin/master-data',
    icon: Database,
    requireSuperAdmin: true  // Only show to super admins
  }
];
```

**Menu items are automatically filtered based on:**
- User's permissions
- Super admin status
- Custom requirements

---

## 6. Backend API Requirements

### Login Endpoint
**POST** `/api/admin/auth/login`

**Request:**
```json
{
  "username": "admin@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "result": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "adminId": 1,
    "name": "John Doe",
    "email": "admin@example.com",
    "role": "Super Admin"
  }
}
```

### Permissions Endpoint
**GET** `/api/admin/auth/permissions`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:**
```json
{
  "result": true,
  "data": {
    "adminId": 1,
    "name": "John Doe",
    "roles": ["SUPER_ADMIN"],
    "permissions": [
      { "module": "MASTER_DATA", "actions": ["VIEW", "ADD", "EDIT", "DELETE"] },
      { "module": "PARTNER", "actions": ["VIEW", "APPROVE", "REJECT"] },
      { "module": "USER", "actions": ["VIEW", "BLOCK", "EDIT"] },
      { "module": "ORDER", "actions": ["VIEW"] },
      { "module": "EARNINGS", "actions": ["VIEW", "EXPORT"] }
    ]
  }
}
```

### Logout Endpoint
**POST** `/api/admin/auth/logout`

**Headers:**
```
Authorization: Bearer {token}
```

---

## 7. Permission Modules & Actions

### Standard Modules:
- `MASTER_DATA` - Master data management
- `PARTNER` - Partner/catering management
- `USER` - User management
- `ORDER` - Order management
- `EARNINGS` - Financial reports
- `REVIEW` - Review management
- `ADMIN` - Admin user management

### Standard Actions:
- `VIEW` - Read/view access
- `ADD` / `CREATE` - Create new records
- `EDIT` / `UPDATE` - Modify existing records
- `DELETE` - Delete records
- `APPROVE` - Approve requests
- `REJECT` - Reject requests
- `EXPORT` - Export data
- `BLOCK` - Block/unblock users

---

## 8. Fallback & Mock Data

If the backend permission API fails, the system falls back to **mock permissions** based on the admin's role:

```jsx
const permissionMap = {
  'Super Admin': {
    roles: ['SUPER_ADMIN'],
    permissions: ['*']  // All permissions
  },
  'Catering Manager': {
    roles: ['CATERING_ADMIN'],
    permissions: ['CATERING_VIEW', 'CATERING_VERIFY', 'CATERING_BLOCK', 'CATERING_EDIT']
  },
  'User Manager': {
    roles: ['USER_ADMIN'],
    permissions: ['USER_VIEW', 'USER_BLOCK', 'USER_EDIT', 'REVIEW_VIEW', 'REVIEW_MODERATE']
  }
};
```

**Note:** Remove mock fallback once backend is fully implemented.

---

## 9. Loading States

### Global Permission Loading
The `PermissionProvider` shows a loading spinner while fetching permissions:

```jsx
if (loading && admin) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="w-16 h-16 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
        <p className="text-gray-600 font-medium">Loading permissions...</p>
      </div>
    </div>
  );
}
```

**This prevents:**
- Accessing routes before permissions are loaded
- Components calling `usePermissions()` prematurely
- Permission check errors

---

## 10. Page Refresh Handling

The system automatically handles page refreshes:

1. **AdminAuthProvider** checks `localStorage` for stored token/admin on mount:
   ```jsx
   useEffect(() => {
     const storedAdmin = localStorage.getItem('admin');
     const token = localStorage.getItem('adminToken');

     if (storedAdmin && token) {
       setAdmin(JSON.parse(storedAdmin));
     }
     setLoading(false);
   }, []);
   ```

2. **PermissionProvider** detects admin presence and refetches permissions:
   ```jsx
   useEffect(() => {
     if (admin) {
       fetchPermissions();
     }
   }, [admin]);
   ```

**Result:** Admin stays logged in and permissions are reloaded after page refresh.

---

## 11. Security Best Practices

### ✅ Do's:
- Always protect sensitive routes with `ProtectedRoute` or permission checks
- Check permissions on both frontend AND backend
- Use JWT tokens with reasonable expiration times
- Clear `localStorage` on logout
- Validate token on every API request (backend)

### ❌ Don'ts:
- Never trust frontend-only permission checks
- Don't store sensitive data in `localStorage`
- Don't expose all admin endpoints without permission checks
- Don't hardcode permissions in components (use the context)

---

## 12. Testing the Implementation

### Test Checklist:

1. **Login Flow**
   - [ ] Admin can login successfully
   - [ ] Token is stored in localStorage
   - [ ] Admin details are available in context

2. **Permission Loading**
   - [ ] Permissions fetch after login
   - [ ] Loading spinner shows while fetching
   - [ ] Admin UI renders after permissions load

3. **Permission Checks**
   - [ ] Super Admin sees all menu items
   - [ ] Limited admin sees filtered menu
   - [ ] Protected routes block unauthorized access
   - [ ] Buttons hide/show based on permissions

4. **Page Refresh**
   - [ ] Admin stays logged in after refresh
   - [ ] Permissions reload correctly
   - [ ] No permission errors on refresh

5. **Logout**
   - [ ] Logout clears localStorage
   - [ ] Redirects to login page
   - [ ] Can't access protected routes after logout

---

## 13. Troubleshooting

### Error: "usePermissions must be used within PermissionProvider"
**Cause:** Component is calling `usePermissions()` outside the provider.
**Solution:** Ensure `PermissionProvider` wraps all admin routes in `Router.jsx`.

### Permissions not loading
**Check:**
1. Admin is logged in (`AdminAuthContext.admin` is set)
2. Token is present in localStorage
3. Backend API `/api/admin/auth/permissions` is accessible
4. Check browser console for API errors

### Menu items not filtering
**Check:**
1. `AdminSidebar` is using `usePermissions()`
2. Navigation items have correct `permission` or `requireSuperAdmin` properties
3. Permissions are loaded (`loading === false`)

---

## 14. Example: Complete Component

```jsx
import { usePermissions } from '../contexts/PermissionContext';
import { useAdminAuth } from '../contexts/AdminAuthContext';

const MasterDataPage = () => {
  const { admin } = useAdminAuth();
  const { hasPermission, isSuperAdmin, loading } = usePermissions();

  // Show loading state
  if (loading) {
    return <div>Loading permissions...</div>;
  }

  // Check permission
  if (!hasPermission('MASTER_DATA', 'VIEW')) {
    return <div>Access Denied</div>;
  }

  return (
    <div>
      <h1>Master Data Management</h1>
      <p>Welcome, {admin.name}</p>

      {/* Conditional rendering */}
      {hasPermission('MASTER_DATA', 'ADD') && (
        <button>Add New Item</button>
      )}

      {hasPermission('MASTER_DATA', 'EDIT') && (
        <button>Edit Item</button>
      )}

      {hasPermission('MASTER_DATA', 'DELETE') && (
        <button>Delete Item</button>
      )}

      {/* Super admin only */}
      {isSuperAdmin && (
        <button>Danger Zone: Reset All Data</button>
      )}
    </div>
  );
};

export default MasterDataPage;
```

---

## Summary

✅ **Architecture is now complete:**
- AdminAuthProvider handles authentication
- PermissionProvider loads and manages permissions
- Providers are correctly nested in Router.jsx
- Components can safely use `usePermissions()` hook
- Menu items auto-filter based on permissions
- Routes can be protected with `ProtectedRoute` component
- Loading states prevent premature access
- Page refresh handled correctly

**The permission error is now fixed!** Admin can login, permissions load, and all components have access to the permission context.
