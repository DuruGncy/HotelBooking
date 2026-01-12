# ?? Hotel Booking Admin API

## Overview

The Admin API is a microservice dedicated to hotel administration and room availability management. This service provides secure endpoints for hotel administrators to manage their properties.

---

## ?? Purpose

This API handles:
- ? Room availability management (CRUD operations)
- ? Hotel and room information viewing
- ? Pricing management
- ? Date range availability configuration

---

## ?? Security

**Authentication**: JWT Bearer Token  
**Authorization**: ADMIN role required for all endpoints (except health check)

### Generate Admin Token

```csharp
// You'll need a JWT helper from the original API or create one
var token = JwtTokenHelper.GenerateAdminToken();
```

---

## ?? API Endpoints

### Health Check

#### `GET /api/admin/health`
Check if the service is running.

**Auth**: None required

**Response**:
```json
{
  "service": "Admin API",
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "description": "Admin API for managing hotel rooms and availability"
}
```

---

### Hotel Management

#### `GET /api/admin/hotels`
Get all hotels in the system.

**Auth**: Required (ADMIN)

**Response**:
```json
[
  {
    "id": 1,
    "name": "Grand Plaza Hotel",
    "location": "New York",
    "description": "Luxury hotel in the heart of Manhattan",
    "starRating": 5
  }
]
```

#### `GET /api/admin/rooms`
Get all rooms across all hotels.

**Auth**: Required (ADMIN)

**Response**:
```json
[
  {
    "id": 1,
    "hotelId": 1,
    "roomType": "Deluxe Suite",
    "maxOccupancy": 2,
    "description": "Spacious suite with king bed",
    "amenities": ["WiFi", "TV", "Mini Bar"]
  }
]
```

---

### Room Availability Management

#### `POST /api/admin/room-availability`
Add new room availability for a date range.

**Auth**: Required (ADMIN)

**Request**:
```json
{
  "hotelId": 1,
  "roomId": 1,
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-03-31T00:00:00Z",
  "availableRooms": 10,
  "pricePerNight": 250.00
}
```

**Validation**:
- Start date must be before end date
- Start date cannot be in the past
- Hotel and room must exist
- Cannot overlap with existing availability

**Response** (201 Created):
```json
{
  "id": 1,
  "hotelId": 1,
  "hotelName": "Grand Plaza Hotel",
  "roomId": 1,
  "roomType": "Deluxe Suite",
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-03-31T00:00:00Z",
  "availableRooms": 10,
  "pricePerNight": 250.00,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

---

#### `PUT /api/admin/room-availability`
Update existing room availability.

**Auth**: Required (ADMIN)

**Request**:
```json
{
  "availabilityId": 1,
  "availableRooms": 8,
  "pricePerNight": 275.00,
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-04-01T00:00:00Z"
}
```

**Note**: All fields except `availabilityId` are optional.

**Response** (200 OK):
```json
{
  "id": 1,
  "hotelId": 1,
  "hotelName": "Grand Plaza Hotel",
  "roomId": 1,
  "roomType": "Deluxe Suite",
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-04-01T00:00:00Z",
  "availableRooms": 8,
  "pricePerNight": 275.00,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T11:00:00Z"
}
```

---

#### `GET /api/admin/room-availability/{id}`
Get specific room availability by ID.

**Auth**: Required (ADMIN)

**Response** (200 OK):
```json
{
  "id": 1,
  "hotelId": 1,
  "hotelName": "Grand Plaza Hotel",
  "roomId": 1,
  "roomType": "Deluxe Suite",
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-03-31T00:00:00Z",
  "availableRooms": 10,
  "pricePerNight": 250.00,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

---

#### `GET /api/admin/hotels/{hotelId}/room-availabilities`
Get all room availabilities for a specific hotel.

**Auth**: Required (ADMIN)

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "hotelId": 1,
    "hotelName": "Grand Plaza Hotel",
    "roomId": 1,
    "roomType": "Deluxe Suite",
    "startDate": "2024-03-01T00:00:00Z",
    "endDate": "2024-03-31T00:00:00Z",
    "availableRooms": 10,
    "pricePerNight": 250.00,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": null
  }
]
```

---

#### `DELETE /api/admin/room-availability/{id}`
Delete room availability by ID.

**Auth**: Required (ADMIN)

**Response** (204 No Content)

---

## ?? Testing with Swagger

1. Run the Admin API
2. Navigate to: `https://localhost:{port}/swagger`
3. Click **Authorize** button
4. Enter: `Bearer {your-admin-token}`
5. Test endpoints!

---

## ?? Test Data

The API comes pre-seeded with test data:

### Hotels
| ID | Name | Location | Stars |
|----|------|----------|-------|
| 1 | Grand Plaza Hotel | New York | 5 |
| 2 | Seaside Resort | Miami | 4 |

### Rooms
| ID | Hotel ID | Type | Max Occupancy |
|----|----------|------|---------------|
| 1 | 1 | Deluxe Suite | 2 |
| 2 | 1 | Standard Room | 2 |
| 3 | 2 | Ocean View Suite | 4 |

### Pre-configured Availabilities
- Grand Plaza - Deluxe Suite: 5 rooms @ $250/night (90 days)
- Grand Plaza - Standard Room: 10 rooms @ $150/night (90 days)
- Seaside Resort - Ocean View: 8 rooms @ $300/night (90 days)

---

## ?? Data Model

### Hotel
```csharp
public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public int StarRating { get; set; }
}
```

### Room
```csharp
public class Room
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string RoomType { get; set; }
    public int MaxOccupancy { get; set; }
    public string Description { get; set; }
    public List<string> Amenities { get; set; }
}
```

### RoomAvailability
```csharp
public class RoomAvailability
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int HotelId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AvailableRooms { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

## ?? Error Responses

### 400 Bad Request
```json
{
  "error": "Start date must be before end date"
}
```

### 401 Unauthorized
```json
{
  "title": "Unauthorized",
  "status": 401
}
```

### 403 Forbidden
```json
{
  "title": "Forbidden",
  "status": 403
}
```

### 404 Not Found
```json
{
  "error": "Hotel with ID 999 not found"
}
```

---

## ?? Running Standalone

```bash
cd HotelBooking.AdminAPI
dotnet run
```

Access:
- Swagger UI: `https://localhost:{port}/swagger`
- Health Check: `https://localhost:{port}/health`

---

## ?? Integration with Other Services

### Through API Gateway
When running with Aspire, access through gateway:
```
https://localhost:{gateway-port}/api/admin/*
```

### Service Discovery
Registered as: `http://adminapi`

---

## ?? Business Rules

1. **Date Validation**:
   - Start date must be before end date
   - Start date cannot be in the past
   - Dates cannot overlap with existing availability

2. **Resource Validation**:
   - Hotel must exist
   - Room must exist and belong to the specified hotel

3. **Capacity Management**:
   - Available rooms must be at least 1
   - Price per night must be non-negative

4. **Authorization**:
   - All management endpoints require ADMIN role
   - Health check is public

---

## ??? Future Enhancements

### Planned Features
- [ ] Database integration (Entity Framework Core)
- [ ] Bulk availability operations
- [ ] Hotel CRUD operations
- [ ] Room CRUD operations
- [ ] Audit logging
- [ ] Price history tracking
- [ ] Seasonal pricing
- [ ] Analytics dashboard

### Production Readiness
- [ ] Replace in-memory storage with SQL Server/PostgreSQL
- [ ] Add Redis caching
- [ ] Implement event publishing for data changes
- [ ] Add comprehensive logging
- [ ] Implement retry policies
- [ ] Add rate limiting

---

## ?? Support

For issues or questions:
- Check main documentation: `MICROSERVICES_ARCHITECTURE.md`
- View migration guide: `MIGRATION_GUIDE.md`
- Quick start: `QUICK_START.md`

---

**Status**: ? Fully Implemented  
**Version**: 1.0  
**Last Updated**: 2024-01-15
