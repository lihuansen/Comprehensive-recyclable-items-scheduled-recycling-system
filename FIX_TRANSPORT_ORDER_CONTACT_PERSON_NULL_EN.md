# Transportation Order Creation Fix - ContactPerson NULL Error

## Quick Reference

**Issue**: Creating transportation orders failed with "Cannot insert NULL value into column ContactPerson"

**Root Cause**: The `GetRecyclerByUsername` method was not retrieving the `FullName` field from the database, resulting in NULL values being inserted into a NOT NULL column.

**Fix**: 
1. Added `FullName` to the SQL query in `StaffDAL.GetRecyclerByUsername`
2. Added fallback logic in `StaffController.CreateTransportationOrder` to use `Username` when `FullName` is NULL

**Files Changed**:
- `recycling.DAL/StaffDAL.cs` (2 lines)
- `recycling.Web.UI/Controllers/StaffController.cs` (1 line)

**Result**: Transportation orders can now be created successfully regardless of whether the recycler has a FullName or not.

## Changes Summary

### 1. StaffDAL.cs
```csharp
// BEFORE: FullName was missing
string sql = @"SELECT RecyclerID, Username, PasswordHash, PhoneNumber, Region, ...

// AFTER: FullName is included
string sql = @"SELECT RecyclerID, Username, PasswordHash, FullName, PhoneNumber, Region, ...

// BEFORE: FullName was not mapped
recycler = new Recyclers { Username, PasswordHash, PhoneNumber, ... }

// AFTER: FullName is mapped with NULL handling
recycler = new Recyclers { 
    Username, 
    PasswordHash, 
    FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : null,
    PhoneNumber, 
    ... 
}
```

### 2. StaffController.cs
```csharp
// BEFORE: Direct assignment could be NULL
ContactPerson = staff.FullName

// AFTER: Fallback to Username if FullName is NULL
ContactPerson = string.IsNullOrWhiteSpace(staff.FullName) ? staff.Username : staff.FullName
```

## Testing

### Scenario 1: Recycler with FullName
- Database: `FullName = "John Doe"`
- Result: `ContactPerson = "John Doe"` ✅

### Scenario 2: Recycler without FullName
- Database: `FullName = NULL`
- Result: `ContactPerson = "username123"` (falls back to Username) ✅

## Validation

- ✅ Code Review: Passed (minor style suggestions only)
- ✅ Security Scan (CodeQL): No vulnerabilities
- ✅ Breaking Changes: None
- ✅ Backwards Compatibility: Maintained

## Related Documentation

See `FIX_TRANSPORT_ORDER_CONTACT_PERSON_NULL_CN.md` for detailed Chinese documentation including:
- Detailed problem analysis
- Complete code changes with before/after comparison
- Flow diagrams
- Database schema information
- Testing scenarios
- Security considerations

## Database Schema

```sql
-- Recyclers table
CREATE TABLE Recyclers (
    FullName NVARCHAR(100) NULL,  -- ← Can be NULL
    ...
);

-- TransportationOrders table
CREATE TABLE TransportationOrders (
    ContactPerson NVARCHAR(50) NOT NULL,  -- ← Cannot be NULL
    ...
);
```

The mismatch between these constraints was the source of the error.
