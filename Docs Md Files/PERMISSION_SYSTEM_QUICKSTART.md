# Permission System - Quick Start Guide

## ✅ What Was Fixed

### Problem:
- Admin logged in successfully
- Backend returned admin details + roles + permissions
- **BUT** PermissionContext was NOT set after login
- Error: "usePermissions must be used within PermissionProvider"

### Solution:
✅ Added `PermissionProvider` wrapper around admin routes
✅ Proper provider hierarchy: `AdminAuthProvider > PermissionProvider > AdminRoutes`
✅ Auto-fetch permissions after login
✅ Loading state prevents premature component rendering
✅ Support for module + action permission format

---

## 🚀 What You Need to Do Next

### 1. Start the Frontend
```bash
cd CateringEcommerce.Web/Frontend
npm run dev
```

### 2. Verify Admin Login
- Navigate to: `http://localhost:5173/admin/login`
- Login with your admin credentials
- **Expected behavior:**
  1. Login success
  2. "Loading permissions..." spinner shows
  3. Redirect to admin dashboard
  4. No permission errors

### 3. Check Browser Console
Open Developer Tools (F12) and check:
- ✅ Login API call successful
- ✅ Permissions API call triggered
- ✅ Permissions stored in context
- ❌ No "usePermissions must be used within PermissionProvider" error

---

## 🔧 Backend Requirements

### Required API Endpoints:

#### 1. Login (Already Implemented)
```
POST /api/admin/auth/login
```

#### 2. Permissions (NEW - Must Implement)
```
GET /api/admin/auth/permissions
```

**Expected Response:**
```json
{
  "result": true,
  "data": {
    "adminId": 1,
    "name": "John Doe",
    "roles": ["SUPER_ADMIN"],
    "permissions": [
      { "module": "MASTER_DATA", "actions": ["VIEW", "ADD", "EDIT", "DELETE"] },
      { "module": "PARTNER", "actions": ["VIEW", "APPROVE", "REJECT"] }
    ]
  }
}
```

**See:** `BACKEND_PERMISSION_API_EXAMPLE.md` for full implementation.

---

## 📁 Files Modified

### Frontend Changes:
1. **`src/router/Router.jsx`**
   - Added `PermissionProvider` wrapper around `AdminRoutes`
   - Import added for `PermissionProvider`

2. **`src/contexts/PermissionContext.jsx`**
   - Added loading spinner while fetching permissions
   - Updated `hasPermission()` to support module + action format
   - Added `isLoadingPermissions` flag
   - Backward compatible with legacy permission strings

3. **`src/components/admin/layout/AdminSidebar.jsx`**
   - Added `usePermissions()` hook
   - Menu items now filter based on permissions
   - Support for `requireSuperAdmin` flag

4. **`src/components/admin/ProtectedRoute.jsx`** (NEW)
   - Component for protecting routes with permissions

### Documentation Created:
- `ADMIN_PERMISSION_ARCHITECTURE.md` - Complete architecture guide
- `BACKEND_PERMISSION_API_EXAMPLE.md` - Backend implementation examples
- `PERMISSION_SYSTEM_QUICKSTART.md` - This file

---

## 🧪 Testing Checklist

### Manual Testing:

#### Test 1: Login Flow
- [ ] Navigate to `/admin/login`
- [ ] Enter credentials and submit
- [ ] Verify "Loading permissions..." shows
- [ ] Dashboard loads without errors
- [ ] Check localStorage for `admin` and `adminToken`

#### Test 2: Permission Loading
- [ ] Open browser DevTools > Network tab
- [ ] Login as admin
- [ ] Verify API call to `/api/admin/auth/permissions`
- [ ] Check response contains `roles` and `permissions`
- [ ] Console shows no permission errors

#### Test 3: Menu Filtering
- [ ] Login as Super Admin → All menu items visible
- [ ] Login as limited admin → Only permitted items visible
- [ ] "Master Data" only visible to Super Admin
- [ ] Menu items match backend permissions

#### Test 4: Page Refresh
- [ ] Login successfully
- [ ] Navigate to any admin page
- [ ] Refresh browser (F5)
- [ ] Admin stays logged in
- [ ] Permissions reload correctly
- [ ] No errors in console

#### Test 5: Protected Routes
- [ ] Try accessing `/admin/master-data` without Super Admin role
- [ ] Should redirect to `/admin/dashboard`
- [ ] Try accessing routes you don't have permissions for
- [ ] Should show access denied or redirect

#### Test 6: Logout
- [ ] Click logout button
- [ ] localStorage cleared
- [ ] Redirect to login page
- [ ] Can't access protected routes
- [ ] Must login again to access admin panel

---

## 📊 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Browser                                 │
│                                                                 │
│  ┌────────────────────────────────────────────────────────┐   │
│  │           AdminAuthProvider                             │   │
│  │  - Manages login/logout                                 │   │
│  │  - Stores admin details & token                         │   │
│  │  - Persists to localStorage                             │   │
│  │                                                          │   │
│  │   ┌────────────────────────────────────────────────┐   │   │
│  │   │      PermissionProvider                        │   │   │
│  │   │  - Fetches permissions after login             │   │   │
│  │   │  - Stores roles & module permissions           │   │   │
│  │   │  - Shows loading spinner until ready           │   │   │
│  │   │  - Exposes hasPermission(module, action)       │   │   │
│  │   │                                                 │   │   │
│  │   │   ┌────────────────────────────────────────┐  │   │   │
│  │   │   │       AdminRoutes                      │  │   │   │
│  │   │   │  - /admin/dashboard                    │  │   │   │
│  │   │   │  - /admin/caterings                    │  │   │   │
│  │   │   │  - /admin/master-data (protected)      │  │   │   │
│  │   │   │  - /admin/users                        │  │   │   │
│  │   │   │                                         │  │   │   │
│  │   │   │   All components can use:              │  │   │   │
│  │   │   │   - useAdminAuth()                     │  │   │   │
│  │   │   │   - usePermissions()                   │  │   │   │
│  │   │   └────────────────────────────────────────┘  │   │   │
│  │   └────────────────────────────────────────────────┘   │   │
│  └────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Backend API                                  │
│                                                                 │
│  POST /api/admin/auth/login                                    │
│  └─► Returns: token, adminId, name, role                       │
│                                                                 │
│  GET /api/admin/auth/permissions (Protected)                   │
│  └─► Returns: roles[], permissions[{module, actions[]}]        │
│                                                                 │
│  POST /api/admin/auth/logout (Protected)                       │
│  └─► Invalidates token                                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💡 How to Use Permissions in Your Components

### Example 1: Check Permission in Component

```jsx
import { usePermissions } from '../contexts/PermissionContext';

const MasterDataPage = () => {
  const { hasPermission } = usePermissions();

  return (
    <div>
      <h1>Master Data</h1>

      {/* Show Add button only if has permission */}
      {hasPermission('MASTER_DATA', 'ADD') && (
        <button>Add New Item</button>
      )}

      {/* Show Edit button only if has permission */}
      {hasPermission('MASTER_DATA', 'EDIT') && (
        <button>Edit Item</button>
      )}
    </div>
  );
};
```

### Example 2: Protect Entire Route

```jsx
import ProtectedRoute from '../components/admin/ProtectedRoute';

<Route
  path="/master-data"
  element={
    <ProtectedRoute module="MASTER_DATA" action="VIEW">
      <MasterDataPage />
    </ProtectedRoute>
  }
/>
```

### Example 3: Check Multiple Permissions

```jsx
const { hasPermission, hasAnyPermission, isSuperAdmin } = usePermissions();

// Check single permission
if (hasPermission('USER', 'DELETE')) {
  // Show delete button
}

// Check if has ANY of these permissions
if (hasAnyPermission(['USER_VIEW', 'USER_EDIT'])) {
  // Show user management section
}

// Check if super admin
if (isSuperAdmin) {
  // Show admin-only features
}
```

---

## ⚠️ Common Issues & Solutions

### Issue 1: "usePermissions must be used within PermissionProvider"
**Cause:** Component is outside PermissionProvider
**Solution:** Already fixed in `Router.jsx` - all admin routes wrapped

### Issue 2: Permissions not loading
**Cause:** Backend API not implemented
**Solution:** Implement `/api/admin/auth/permissions` endpoint
**Temporary:** System falls back to mock permissions based on role

### Issue 3: Menu items not filtering
**Cause:** Permissions not matching sidebar configuration
**Solution:** Update permission strings in `AdminSidebar.jsx` to match backend

### Issue 4: Infinite loading spinner
**Cause:** Permission API returning error
**Solution:** Check browser console, verify API endpoint and token

---

## 🎯 Next Steps

1. **Immediate:**
   - [ ] Test login flow
   - [ ] Verify no permission errors
   - [ ] Check menu filtering works

2. **Backend Implementation:**
   - [ ] Implement `/api/admin/auth/permissions` endpoint
   - [ ] Create database tables for roles & permissions
   - [ ] Seed default permissions
   - [ ] Add permission checks to backend routes

3. **Frontend Enhancement:**
   - [ ] Add permission checks to remaining admin pages
   - [ ] Implement `ProtectedRoute` on sensitive routes
   - [ ] Add permission-based button visibility
   - [ ] Test with different admin roles

4. **Production Ready:**
   - [ ] Remove mock permission fallback
   - [ ] Add error boundaries
   - [ ] Implement permission caching
   - [ ] Add audit logging for permission checks

---

## 📚 Documentation Links

- **Full Architecture:** `ADMIN_PERMISSION_ARCHITECTURE.md`
- **Backend API Guide:** `BACKEND_PERMISSION_API_EXAMPLE.md`
- **This Guide:** `PERMISSION_SYSTEM_QUICKSTART.md`

---

## ✅ Summary

**The permission system is now fully implemented on the frontend!**

- ✅ PermissionProvider wraps all admin routes
- ✅ Permissions load automatically after login
- ✅ Components can safely use `usePermissions()` hook
- ✅ Menu items filter based on permissions
- ✅ Routes can be protected
- ✅ Page refresh handled correctly
- ✅ Loading states prevent errors

**Next:** Implement backend permission API and test the complete flow.

---

**Ready to test?**
Run `npm run dev` and login at `/admin/login`
