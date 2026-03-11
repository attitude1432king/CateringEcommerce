# Category-Based Catering Search API Documentation

## Overview

This backend implementation supports the CategoryTiles component functionality, allowing users to search for caterings based on service categories: Wedding, Corporate Events, Party & Celebrations, and Decorations.

## Database Structure

### Event Types
- **Table**: `t_sys_catering_type_master`
- **Filter**: `c_category_id = 3` (Event Types)
- **Categories**: Wedding, Corporate, Birthday/Party

### Decorations
- **Table**: `t_sys_catering_decorations`
- **Logic**: Caterings with active decorations are filtered using an EXISTS query

### Catering Operations Mapping
- **Table**: `t_sys_catering_owner_operations`
- **Purpose**: Maps caterings to their supported event types

## API Endpoints

### 1. Search by Category

**Endpoint**: `GET /api/User/Home/SearchByCategory`

**Description**: Searches caterings based on predefined service categories

**Parameters**:
- `category` (required): Category name - "wedding", "corporate", "party"/"parties", or "decorations"
- `city` (optional): Filter by city name
- `pageNumber` (optional, default: 1): Page number for pagination
- `pageSize` (optional, default: 20): Number of results per page (max: 100)

**Example Requests**:
```
GET /api/User/Home/SearchByCategory?category=wedding
GET /api/User/Home/SearchByCategory?category=corporate&city=Mumbai
GET /api/User/Home/SearchByCategory?category=party&pageNumber=1&pageSize=10
GET /api/User/Home/SearchByCategory?category=decorations&city=Delhi
```

**Category Mappings**:
- `wedding` → Filters caterings offering "Wedding" event type
- `corporate` → Filters caterings offering "Corporate" event type
- `party` or `parties` → Filters caterings offering "Birthday" or "Party" event types
- `decorations` → Filters caterings that have active decorations (from `t_sys_catering_decorations`)

**Response**:
```json
{
  "success": true,
  "message": "Caterings for wedding category retrieved successfully.",
  "data": [
    {
      "id": 123,
      "cateringName": "Elite Caterers",
      "logoUrl": "/uploads/logo.jpg",
      "averageRating": 4.8,
      "totalReviews": 150,
      "minOrderValue": 5000,
      "status": "Active",
      "deliveryRadiusKm": 50,
      "isOnline": true,
      "city": "Mumbai",
      "area": "Andheri",
      "distanceKm": 0,
      "cuisineTypes": ["Indian", "Chinese"],
      "serviceTypes": ["Full Service", "Buffet"]
    }
  ],
  "pagination": {
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 3
  }
}
```

### 2. Get Event Types

**Endpoint**: `GET /api/User/Home/EventTypes`

**Description**: Returns all active event types for displaying on category tiles

**Response**:
```json
{
  "success": true,
  "message": "Event types retrieved successfully.",
  "data": [
    { "id": 1, "name": "Wedding" },
    { "id": 2, "name": "Corporate" },
    { "id": 3, "name": "Birthday" },
    { "id": 4, "name": "Party" }
  ]
}
```

### 3. Enhanced Search Endpoint

**Endpoint**: `GET /api/User/Home/Search`

**Description**: Comprehensive search with all filters (now includes decorations filter)

**New Parameter**:
- `hasDecorations` (optional): Boolean to filter caterings offering decorations

**Example Requests**:
```
GET /api/User/Home/Search?hasDecorations=true
GET /api/User/Home/Search?eventTypes=1,2&hasDecorations=true
GET /api/User/Home/Search?city=Mumbai&hasDecorations=true&minRating=4.0
```

**All Parameters**:
- `city`: City name filter
- `cuisineTypes`: Comma-separated cuisine type IDs
- `serviceTypes`: Comma-separated service type IDs
- `eventTypes`: Comma-separated event type IDs
- `keyword`: Search keyword
- `minRating`: Minimum average rating
- `onlineOnly`: Filter only online caterers
- `verifiedOnly`: Filter only verified caterers (default: true)
- `minOrderFrom`: Minimum order value - from
- `minOrderTo`: Minimum order value - to
- `deliveryRadius`: Delivery radius in km
- `hasDecorations`: Filter caterings with decorations (NEW)
- `pageNumber`: Page number (default: 1)
- `pageSize`: Page size (default: 20, max: 100)

## Implementation Details

### Model Changes

**File**: `CateringEcommerce.Domain/Models/User/CateringBrowseModel.cs`

Added property to `CateringSearchFilterDto`:
```csharp
public bool? HasDecorations { get; set; } // Filter caterings that offer decorations
```

### Repository Changes

**File**: `CateringEcommerce.BAL/Common/CateringBrowseRepository.cs`

Added decorations filter in `SearchCateringsWithFiltersAsync`:
```csharp
// Decorations filter - filter caterings that have decorations
if (filter.HasDecorations == true)
{
    sb.Append($@"
        AND EXISTS (
            SELECT 1
            FROM {Table.SysCateringDecorations} deco
            WHERE deco.c_ownerid = o.c_ownerid
            AND deco.c_is_active = 1
        ) ");
}
```

### Controller Changes

**File**: `CateringEcommerce.API/Controllers/User/HomeController.cs`

1. Added `SearchByCategory` endpoint
2. Added `GetEventTypes` endpoint
3. Added `GetEventTypeIdsByNameAsync` helper method
4. Updated `Search` endpoint to support `hasDecorations` parameter

## Frontend Integration

### CategoryTiles Component Usage

```javascript
// Wedding Category
const handleWeddingClick = () => {
  window.location.href = '/services/wedding';
  // OR use fetch:
  fetch('/api/User/Home/SearchByCategory?category=wedding')
    .then(res => res.json())
    .then(data => {
      // Display caterings
    });
};

// Corporate Category
fetch('/api/User/Home/SearchByCategory?category=corporate&city=Mumbai')
  .then(res => res.json())
  .then(data => setCaterings(data.data));

// Party Category
fetch('/api/User/Home/SearchByCategory?category=party')
  .then(res => res.json())
  .then(data => setCaterings(data.data));

// Decorations Category
fetch('/api/User/Home/SearchByCategory?category=decorations')
  .then(res => res.json())
  .then(data => setCaterings(data.data));
```

### Advanced Search with Multiple Filters

```javascript
// Search for wedding caterings with decorations in Mumbai
const params = new URLSearchParams({
  city: 'Mumbai',
  eventTypes: '1', // Wedding event type ID
  hasDecorations: 'true',
  minRating: '4.0',
  verifiedOnly: 'true'
});

fetch(`/api/User/Home/Search?${params}`)
  .then(res => res.json())
  .then(data => setCaterings(data.data));
```

## Testing

### Test Cases

1. **Test Wedding Category**:
   ```
   GET /api/User/Home/SearchByCategory?category=wedding
   Expected: Returns all caterings offering wedding event type
   ```

2. **Test Corporate Category**:
   ```
   GET /api/User/Home/SearchByCategory?category=corporate&city=Mumbai
   Expected: Returns Mumbai caterings offering corporate event type
   ```

3. **Test Party Category**:
   ```
   GET /api/User/Home/SearchByCategory?category=party
   Expected: Returns caterings offering birthday/party event types
   ```

4. **Test Decorations Category**:
   ```
   GET /api/User/Home/SearchByCategory?category=decorations
   Expected: Returns caterings with active decorations in t_sys_catering_decorations
   ```

5. **Test Invalid Category**:
   ```
   GET /api/User/Home/SearchByCategory?category=invalid
   Expected: 400 Bad Request with error message
   ```

6. **Test Event Types Endpoint**:
   ```
   GET /api/User/Home/EventTypes
   Expected: Returns all event types with c_category_id = 3
   ```

7. **Test Decorations Filter**:
   ```
   GET /api/User/Home/Search?hasDecorations=true
   Expected: Returns only caterings with decorations
   ```

## Database Requirements

### Prerequisites

1. Ensure `t_sys_catering_type_master` has event types with `c_category_id = 3`:
   ```sql
   SELECT * FROM t_sys_catering_type_master WHERE c_category_id = 3;
   ```

2. Ensure caterings are mapped in `t_sys_catering_owner_operations`:
   ```sql
   SELECT * FROM t_sys_catering_owner_operations;
   ```

3. Ensure decorations exist in `t_sys_catering_decorations`:
   ```sql
   SELECT * FROM t_sys_catering_decorations WHERE c_is_active = 1;
   ```

### Sample Event Types

| c_type_id | c_type_name | c_category_id |
|-----------|-------------|---------------|
| 1         | Wedding     | 3             |
| 2         | Corporate   | 3             |
| 3         | Birthday    | 3             |
| 4         | Party       | 3             |

## Error Handling

The API handles the following errors:

1. **Invalid Category**: Returns 400 with message listing valid categories
2. **Database Connection**: Returns 500 with generic error message
3. **No Results**: Returns empty array in data field with totalCount = 0
4. **Invalid Parameters**: Returns 400 with validation error message

## Performance Considerations

1. **Indexing**: Ensure indexes exist on:
   - `t_sys_catering_type_master(c_category_id, c_is_active)`
   - `t_sys_catering_decorations(c_ownerid, c_is_active)`
   - `t_sys_catering_owner_operations(c_ownerid, c_event_types)`

2. **Caching**: Consider caching event type mappings as they rarely change

3. **Pagination**: Always use pagination for large result sets

## Future Enhancements

1. Add caching for event type IDs
2. Support multiple categories in single search
3. Add sorting options (by rating, price, distance)
4. Add distance-based filtering using geo-coordinates
5. Support category combinations (e.g., Wedding + Decorations)
