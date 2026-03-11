# Payment System Table Naming Fix - Summary

## Date: 2026-01-29

## Overview
Fixed table naming inconsistencies in the split payment system to match the master SQL schema naming convention (`t_sys_` prefix instead of `tbl_`).

## Files Changed

### 1. Database Schema Files

#### `Database/Split_Payment_Schema.sql`
- **Changed:** All table names updated from `tbl_` prefix to `t_sys_` prefix
- **Tables Updated:**
  - `tbl_PaymentTransactions` → `t_sys_payment_transactions`
  - `tbl_OrderPaymentSummary` → `t_sys_order_payment_summary`
  - `tbl_EscrowLedger` → `t_sys_escrow_ledger`
  - `tbl_VendorPayoutRequests` → `t_sys_vendor_payout_requests`
  - `tbl_EMIPlans` → `t_sys_emi_plans`
  - `tbl_PaymentGatewayConfig` → `t_sys_payment_gateway_config`
- **Foreign Keys Updated:**
  - References to `tbl_Orders` → `t_sys_order`
  - References to `tbl_User` → `t_sys_user`
  - References to `tbl_CateringOwners` → `t_sys_catering_owner` (using `c_ownerid` column)

#### `Database/Split_Payment_StoredProcedures.sql`
- **Updated Stored Procedures:**
  - `sp_InitializeOrderPayment`
  - `sp_ProcessAdvancePayment`
  - `sp_ProcessFinalPayment`
  - `sp_ReleaseAdvanceToVendor`
  - `sp_ProcessFinalVendorPayout`
  - `sp_GetEMIPlans`
  - `sp_GetPaymentSummary`
- **Changes:** All table references updated to use `t_sys_` prefix
- **Column Name Fix:** Changed `c_businessname` to `c_catering_name` to match master schema

### 2. C# Code Files

#### `CateringEcommerce.BAL/Base/Payment/SplitPaymentRepository.cs`
- **Changed:** All SQL queries updated to use correct table names
- **Methods Updated:**
  - `GetOrderTransactionsAsync`
  - `GetEscrowLedgerAsync`
  - `CalculateEMIAsync`
  - `GetVendorPayoutRequestsAsync`
  - `GetPaymentDashboardAsync`
  - `GetVendorPayoutDashboardAsync`
  - `GetEscrowDashboardAsync`
  - `GetPaymentGatewayConfigAsync`
  - `GetActivePaymentGatewaysAsync`
- **Column Name Fix:** Changed `c_businessname` to `c_catering_name`

#### `CateringEcommerce.Domain/Interfaces/IDatabaseHelper.cs`
- **Added Methods:**
  ```csharp
  Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedureName, SqlParameter[] parameters = null);
  Task<List<T>> ExecuteQueryAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);
  ```

#### `CateringEcommerce.BAL/DatabaseHelper/DatabaseHelperBase.cs`
- **Added Abstract Methods:**
  ```csharp
  public abstract Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedureName, SqlParameter[] parameters = null);
  public abstract Task<List<T>> ExecuteQueryAsync<T>(string query, SqlParameter[] parameters = null, CommandType commandType = CommandType.Text);
  ```

#### `CateringEcommerce.BAL/DatabaseHelper/SqlDatabaseManager.cs`
- **Implemented New Methods:**
  - `ExecuteStoredProcedureAsync<T>` - Generic method to execute stored procedures and return typed objects
  - `ExecuteQueryAsync<T>` - Generic method to execute queries and return list of typed objects
  - `MapDataRowToObject<T>` - Helper method to map DataRow to C# objects using reflection

#### `CateringEcommerce.BAL/Base/Admin/RBACRepository.cs`
- **Added Method:**
  ```csharp
  public async Task<bool> HasPermissionAsync(long adminId, string permissionCode)
  {
      return await AdminHasPermissionAsync(adminId, permissionCode);
  }
  ```
  - Added as alias for backward compatibility with controllers

## Build Status
- **Before:** 18 Errors
- **After:** 0 Errors ✅
- **Warnings:** 444 (acceptable - mostly null reference warnings)

## Table Naming Convention Reference

### Master Schema Convention (`mastersql.sql`)
| Old Name (tbl_) | New Name (t_sys_) | Description |
|-----------------|-------------------|-------------|
| tbl_User | t_sys_user | User accounts |
| tbl_CateringOwners | t_sys_catering_owner | Catering business owners |
| tbl_Orders | t_sys_order | Order records |

### Payment Schema Tables
| Table Name | Purpose |
|------------|---------|
| t_sys_payment_transactions | All payment transactions (advance, final, refund, commission) |
| t_sys_order_payment_summary | Overall payment status per order |
| t_sys_escrow_ledger | Escrow transaction tracking |
| t_sys_vendor_payout_requests | Vendor payout request tracking |
| t_sys_emi_plans | EMI payment plan configurations |
| t_sys_payment_gateway_config | Payment gateway settings |

## Testing Recommendations

1. **Database Migration:**
   - Run `Split_Payment_Schema.sql` on clean database
   - Run `Split_Payment_StoredProcedures.sql`
   - Verify all foreign keys are created successfully

2. **Code Testing:**
   - Test payment initialization: `sp_InitializeOrderPayment`
   - Test advance payment: `sp_ProcessAdvancePayment`
   - Test final payment: `sp_ProcessFinalPayment`
   - Test escrow release: `sp_ReleaseAdvanceToVendor`
   - Test vendor payout: `sp_ProcessFinalVendorPayout`

3. **Repository Testing:**
   - Test `SplitPaymentRepository` methods with actual database
   - Verify data mapping works correctly with new generic methods
   - Test EMI calculations
   - Test dashboard queries

## Notes
- All changes maintain backward compatibility where possible
- Generic methods use reflection for object mapping - performance acceptable for most use cases
- Foreign key constraints properly reference master schema tables
- No data migration script needed as tables are new

## Next Steps
1. Execute schema scripts on development database
2. Test all payment workflows
3. Update any stored procedures that reference old table names
4. Consider adding indexes for performance optimization
