# ?? Migration Guide - From Monolith to Microservices

This guide shows how to migrate your existing `HotelBooking.API` code to the new microservices.

---

## Overview

You have:
- ? **HotelBooking.API** (original monolithic API)
- ? **HotelBooking.AdminAPI** (new - with placeholder controller)
- ? **HotelBooking.ClientAPI** (new - with placeholder controller)
- ? **HotelBooking.NotificationAPI** (new - with placeholder controller)

**Goal**: Move the real implementation from the monolith to microservices.

---

## Migration Steps

### Step 1: Copy Models (Shared Data)

**Option A: Shared Library (Recommended)**
```bash
dotnet new classlib -n HotelBooking.Shared
```

Move these to the shared library:
- `Models/Hotel.cs`
- `Models/Room.cs`
- `Models/RoomAvailability.cs`
- `Models/Booking.cs`
- `Models/Notification.cs`
- `Models/User.cs`
- All DTOs from `Models/DTOs/`

**Option B: Duplicate Models**
Copy models to each microservice that needs them.

---

### Step 2: Migrate Admin API

#### Files to Copy/Move:

**From** `HotelBooking.API` **To** `HotelBooking.AdminAPI`

1. **Controller**:
   ```
   HotelBooking.API/Controllers/AdminHotelsController.cs
   ? HotelBooking.AdminAPI/Controllers/AdminController.cs
   ```
   - Replace the placeholder `AdminController.cs`
   - Update namespace to `HotelBooking.AdminAPI.Controllers`

2. **Service**:
   ```
   HotelBooking.API/Services/AdminHotelService.cs
   ? HotelBooking.AdminAPI/Services/AdminHotelService.cs
   ```

3. **Interface**:
   ```
   HotelBooking.API/Services/Interfaces/IAdminHotelService.cs
   ? HotelBooking.AdminAPI/Services/Interfaces/IAdminHotelService.cs
   ```

4. **DTOs**:
   ```
   HotelBooking.API/Models/DTOs/AddRoomAvailabilityRequest.cs
   HotelBooking.API/Models/DTOs/UpdateRoomAvailabilityRequest.cs
   HotelBooking.API/Models/DTOs/RoomAvailabilityResponse.cs
   ? HotelBooking.AdminAPI/Models/DTOs/
   ```

5. **Update Program.cs**:
   ```csharp
   // Add to HotelBooking.AdminAPI/Program.cs after line: builder.Services.AddControllers();
   
   // Register services
   builder.Services.AddScoped<IAdminHotelService, AdminHotelService>();
   
   // Seed test data (before app.Build())
   AdminHotelService.SeedTestData();
   ```

---

### Step 3: Migrate Client API

#### Files to Copy/Move:

**From** `HotelBooking.API` **To** `HotelBooking.ClientAPI`

1. **Controllers**:
   ```
   HotelBooking.API/Controllers/HotelSearchController.cs
   ? HotelBooking.ClientAPI/Controllers/HotelSearchController.cs
   
   HotelBooking.API/Controllers/BookHotelController.cs
   ? HotelBooking.ClientAPI/Controllers/BookHotelController.cs
   ```
   - Replace the placeholder `HotelsController.cs`
   - Update namespace to `HotelBooking.ClientAPI.Controllers`

2. **Services**:
   ```
   HotelBooking.API/Services/HotelSearchService.cs
   ? HotelBooking.ClientAPI/Services/HotelSearchService.cs
   
   HotelBooking.API/Services/BookHotelService.cs
   ? HotelBooking.ClientAPI/Services/BookHotelService.cs
   ```

3. **Interfaces**:
   ```
   HotelBooking.API/Services/Interfaces/IHotelSearchService.cs
   HotelBooking.API/Services/Interfaces/IBookHotelService.cs
   ? HotelBooking.ClientAPI/Services/Interfaces/
   ```

4. **DTOs**:
   ```
   HotelBooking.API/Models/DTOs/HotelSearchRequest.cs
   HotelBooking.API/Models/DTOs/HotelSearchResponse.cs
   HotelBooking.API/Models/DTOs/CreateBookingRequest.cs
   HotelBooking.API/Models/DTOs/BookingResponse.cs
   HotelBooking.API/Models/DTOs/CancelBookingRequest.cs
   ? HotelBooking.ClientAPI/Models/DTOs/
   ```

5. **Update Program.cs**:
   ```csharp
   // Add to HotelBooking.ClientAPI/Program.cs
   
   // Register services
   builder.Services.AddScoped<IHotelSearchService, HotelSearchService>();
   builder.Services.AddScoped<IBookHotelService, BookHotelService>();
   
   // Initialize data from AdminService
   // Note: In production, use shared database
   ```

---

### Step 4: Migrate Notification API

#### Files to Copy/Move:

**From** `HotelBooking.API` **To** `HotelBooking.NotificationAPI`

1. **Controller**:
   ```
   HotelBooking.API/Controllers/NotificationController.cs
   ? HotelBooking.NotificationAPI/Controllers/NotificationsController.cs
   ```
   - Replace the placeholder `NotificationsController.cs`
   - Update namespace to `HotelBooking.NotificationAPI.Controllers`

2. **Services**:
   ```
   HotelBooking.API/Services/NotificationService.cs
   ? HotelBooking.NotificationAPI/Services/NotificationService.cs
   
   HotelBooking.API/Services/Background/NotificationBackgroundService.cs
   ? HotelBooking.NotificationAPI/Services/Background/NotificationBackgroundService.cs
   ```

3. **Interface**:
   ```
   HotelBooking.API/Services/Interfaces/INotificationService.cs
   ? HotelBooking.NotificationAPI/Services/Interfaces/INotificationService.cs
   ```

4. **DTOs**:
   ```
   HotelBooking.API/Models/DTOs/NotificationResponse.cs
   HotelBooking.API/Models/DTOs/LowCapacityAlert.cs
   ? HotelBooking.NotificationAPI/Models/DTOs/
   ```

5. **Update Program.cs**:
   ```csharp
   // Add to HotelBooking.NotificationAPI/Program.cs
   
   // Register services
   builder.Services.AddScoped<INotificationService, NotificationService>();
   builder.Services.AddHostedService<NotificationBackgroundService>();
   ```

---

## Quick Migration Script

Here's a PowerShell script to help with the migration:

```powershell
# Run from HotelBooking directory
$apiPath = "HotelBooking.API"

# Copy Admin API files
Copy-Item "$apiPath/Controllers/AdminHotelsController.cs" "HotelBooking.AdminAPI/Controllers/" -Force
Copy-Item "$apiPath/Services/AdminHotelService.cs" "HotelBooking.AdminAPI/Services/" -Force -Recurse

# Copy Client API files
Copy-Item "$apiPath/Controllers/HotelSearchController.cs" "HotelBooking.ClientAPI/Controllers/" -Force
Copy-Item "$apiPath/Controllers/BookHotelController.cs" "HotelBooking.ClientAPI/Controllers/" -Force
Copy-Item "$apiPath/Services/HotelSearchService.cs" "HotelBooking.ClientAPI/Services/" -Force
Copy-Item "$apiPath/Services/BookHotelService.cs" "HotelBooking.ClientAPI/Services/" -Force

# Copy Notification API files
Copy-Item "$apiPath/Controllers/NotificationController.cs" "HotelBooking.NotificationAPI/Controllers/" -Force
Copy-Item "$apiPath/Services/NotificationService.cs" "HotelBooking.NotificationAPI/Services/" -Force
Copy-Item "$apiPath/Services/Background" "HotelBooking.NotificationAPI/Services/" -Force -Recurse

# Copy Models to each service (or create shared library)
Copy-Item "$apiPath/Models" "HotelBooking.AdminAPI/" -Force -Recurse
Copy-Item "$apiPath/Models" "HotelBooking.ClientAPI/" -Force -Recurse
Copy-Item "$apiPath/Models" "HotelBooking.NotificationAPI/" -Force -Recurse

Write-Host "? Files copied! Now update namespaces and references."
```

---

## After Migration

### 1. Update Namespaces

Update all copied files to use the correct namespace:
- Admin API: `namespace HotelBooking.AdminAPI...`
- Client API: `namespace HotelBooking.ClientAPI...`
- Notification API: `namespace HotelBooking.NotificationAPI...`

### 2. Fix Using Statements

Update using statements to match new locations:
```csharp
// Old
using HotelBookingAPI.Models;
using HotelBookingAPI.Services;

// New (example for AdminAPI)
using HotelBooking.AdminAPI.Models;
using HotelBooking.AdminAPI.Services;
```

### 3. Update Service Registration

Each service's `Program.cs` should register its services:
```csharp
// Example for AdminAPI
builder.Services.AddScoped<IAdminHotelService, AdminHotelService>();
```

### 4. Data Sharing Strategy

**Short-term (Development)**:
- Use in-memory static data
- Initialize from seed data

**Long-term (Production)**:
- Shared SQL database
- Each service has its schema
- Use Redis for caching
- Implement event-driven updates

---

## Testing After Migration

### 1. Build Each Service
```bash
dotnet build HotelBooking.AdminAPI
dotnet build HotelBooking.ClientAPI
dotnet build HotelBooking.NotificationAPI
```

### 2. Test Individually
```bash
# Terminal 1
cd HotelBooking.AdminAPI
dotnet run

# Terminal 2
cd HotelBooking.ClientAPI
dotnet run

# Terminal 3
cd HotelBooking.NotificationAPI
dotnet run
```

### 3. Test Through Aspire
```bash
cd HotelBooking.AppHost
dotnet run
```

### 4. Verify in Swagger
- Open each service's Swagger UI
- Test endpoints
- Verify responses

---

## Database Migration (Optional but Recommended)

### Create Shared Database

```csharp
// Add to each service
services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("HotelDb")));
```

### Connection String
```json
{
  "ConnectionStrings": {
    "HotelDb": "Server=localhost;Database=HotelBooking;Trusted_Connection=true;"
  }
}
```

### Schema Separation
- `admin.*` - Admin API tables
- `client.*` - Client API tables  
- `notification.*` - Notification API tables
- `shared.*` - Shared tables (hotels, rooms)

---

## Troubleshooting

### Issue: Build Errors
**Fix**: Update all namespaces and using statements

### Issue: Services Can't Find Models
**Fix**: Create shared library or copy models to each service

### Issue: Data Not Synchronized
**Fix**: Implement shared database or message queue

### Issue: Authentication Not Working
**Fix**: Ensure JWT settings match across all services

---

## Verification Checklist

- [ ] All services build successfully
- [ ] Swagger UI works for each service
- [ ] Health checks return 200 OK
- [ ] Admin API endpoints work with authentication
- [ ] Client API search returns results
- [ ] Notification API sends notifications
- [ ] Gateway routes to all services correctly
- [ ] Aspire Dashboard shows all services healthy

---

## Clean Up (After Migration Complete)

Once migration is verified:

1. Archive the old API:
   ```bash
   mkdir Archive
   move HotelBooking.API Archive/
   ```

2. Update solution file (if using)

3. Update documentation

---

## Need Help?

- **Architecture Questions**: See `MICROSERVICES_ARCHITECTURE.md`
- **Quick Start**: See `QUICK_START.md`
- **Original API Reference**: See `HotelBooking.API/README.md`

---

**Happy Migrating! ??**

Remember: Migrate one service at a time and test thoroughly before moving to the next.
