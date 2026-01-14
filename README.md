# 🏨 Hotel Booking System - Complete Documentation

A modern, cloud-ready hotel booking platform built with microservices architecture, featuring real-time availability management, ML-based price predictions, and email notifications.

## 📹 Project Presentation
**[Watch Project Demo - 5 Minutes](https://youtu.be/GebyLqw51v)**

## 🌍 Deployed URLs

### Production Environment (AWS)
> **Note**: URLs will be available after AWS AppRunner and Aurora deployment

| Service | URL | 
|---------|-----|
| **Frontend** | `https://rape5jkqpe.eu-central-1.awsapprunner.com` | 
| **Admin API** | `https://k9gxizywk3.eu-central-1.awsapprunner.com/swagger/index.html` | 
| **Client API** | `https://x3ewmrn4uj.us-east-1.awsapprunner.com/swagger/index.html` | 
| **Notification API** | `https://ddqxuk77r2.us-east-1.awsapprunner.com/swagger/index.html` |

### Local Development URLs
| Service | URL | Port |
|---------|-----|------|
| Frontend | `http://localhost:3000` | 3000 |
| Admin API | `http://localhost:5126/swagger` | 5126 |
| Client API | `http://localhost:5182/swagger` | 5182 |
| Notification API | `http://localhost:15002/swagger` | 15002 |
| Predict API | `http://localhost:8087/docs` | 8087 |

---

## 📊 Data Models & ER Diagram

### Complete Entity Relationship Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                                                                  │
│  ┌──────────────────┐         ┌─────────────────────────────┐   │
│  │     Hotels       │         │   RoomAvailabilities        │   │
│  ├──────────────────┤         ├─────────────────────────────┤   │
│  │ * id (PK)        │◄────────│ * id (PK)                   │   │
│  │ - name           │    N    │ - hotelId (FK)              │   │
│  │ - location       │         │ - roomId (FK)               │   │
│  │ - description    │         │ - startDate                 │   │
│  │ - starRating     │         │ - endDate                   │   │
│  │ - isActive       │         │ - availableRooms            │   │
│  │ - createdAt      │         │ - pricePerNight             │   │
│  └────────┬─────────┘         └─────────────────────────────┘   │
│           │ 1:N                                                  │
│           │                   ┌─────────────────────────────┐   │
│           │                   │        Rooms                │   │
│           │            ┌──────│ * id (PK)                   │   │
│           │            │  N   │ - hotelId (FK)              │   │
│           └────────────┼──────│ - roomType                  │   │
│                        │      │ - description               │   │
│                        │      │ - maxOccupancy              │   │
│                        │      │ - amenities                 │   │
│                        │      │ - isActive                  │   │
│                        │      └──────────────┬──────────────┘   │
│                        └─────────────────────┘                  │
│                                                                  │
│           ┌──────────────────────────────────┐                  │
│           │         Bookings                 │                  │
│           ├──────────────────────────────────┤                  │
│           │ * id (PK)                        │                  │
│           │ - bookingReference (UNIQUE)      │                  │
│           │ - hotelId (FK) ─────────────┐    │                  │
│           │ - roomId (FK) ──────────┐   │    │                  │
│           │ - checkInDate            │   │    │                  │
│           │ - checkOutDate           │   │    │                  │
│           │ - numberOfGuests         │   │    │                  │
│           │ - guestName              │   │    │                  │
│           │ - guestEmail             │   │    │                  │
│           │ - totalPrice             │   │    │                  │
│           │ - status                 │   │    │                  │
│           │ - createdAt              │   │    │                  │
│           │ - cancelledAt            │   │    │                  │
│           └───────────┬──────────────┘   │    │                  │
│                       │ 1:N              │    │                  │
│                       │                  │    │                  │
│           ┌───────────▼──────────────────┼────┼───────────────┐  │
│           │   Notifications             │    │               │  │
│           ├─────────────────────────────┼────┼───────────────┤  │
│           │ * id (PK)                   │    │               │  │
│           │ - bookingId (FK) ◄──────────┴────┘               │  │
│           │ - hotelId (FK) ◄────────────┴────────────────────┘  │
│           │ - type                                              │  │
│           │ - recipient                                         │  │
│           │ - subject                                           │  │
│           │ - body                                              │  │
│           │ - sentAt                                            │  │
│           │ - status                                            │  │
│           │ - createdAt                                         │  │
│           └─────────────────────────────────────────────────────┘  │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

### Data Model Specifications

#### Hotels Table
- **Primary Key**: `id` (SERIAL)
- **Columns**:
  - `name` (VARCHAR 255) - Hotel name
  - `location` (VARCHAR 255) - City/Location
  - `description` (TEXT) - Hotel description
  - `starRating` (INT 0-5) - Star rating
  - `isActive` (BOOLEAN) - Soft delete flag
  - `createdAt` (TIMESTAMP) - Creation timestamp

#### Rooms Table
- **Primary Key**: `id` (SERIAL)
- **Foreign Keys**: `hotelId` → Hotels(id)
- **Columns**:
  - `roomType` (VARCHAR) - Type of room (Deluxe, Standard, etc.)
  - `description` (TEXT) - Room details
  - `maxOccupancy` (INT) - Maximum guests
  - `amenities` (TEXT) - Comma-separated amenities

#### RoomAvailabilities Table
- **Primary Key**: `id` (SERIAL)
- **Foreign Keys**: `hotelId`, `roomId`
- **Columns**:
  - `startDate` (DATE) - Availability start
  - `endDate` (DATE) - Availability end
  - `availableRooms` (INT) - Number available
  - `pricePerNight` (DECIMAL 10,2) - Price per night

#### Bookings Table
- **Primary Key**: `id` (SERIAL)
- **Unique Constraint**: `bookingReference`
- **Foreign Keys**: `hotelId`, `roomId`
- **Columns**:
  - `bookingReference` (VARCHAR 50) - User-friendly ID (e.g., BK20240215ABC123)
  - `checkInDate`, `checkOutDate` (DATE)
  - `numberOfGuests` (INT)
  - `guestName`, `guestEmail` (VARCHAR)
  - `totalPrice` (DECIMAL 10,2) - Locked at booking time
  - `status` (VARCHAR) - Pending/Confirmed/Cancelled
  - `cancelledAt` (TIMESTAMP, NULL) - Cancellation timestamp

#### Notifications Table
- **Primary Key**: `id` (SERIAL)
- **Foreign Keys**: `bookingId`, `hotelId`
- **Columns**:
  - `type` (VARCHAR) - Confirmation, Alert, etc.
  - `recipient` (VARCHAR) - Email address
  - `subject`, `body` (VARCHAR/TEXT)
  - `sentAt` (TIMESTAMP) - When sent
  - `status` (VARCHAR) - Pending/Sent/Failed

---

## 🏗️ System Architecture & Design

### Microservices Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    React Frontend (3000)                     │
│  ├─ Hotel Search                                             │
│  ├─ Booking Management                                       │
│  ├─ Admin Dashboard                                          │
│  └─ Price Prediction Interface                               │
└─────────────────────┬────────────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┬──────────────┐
        │             │             │              │
        ▼             ▼             ▼              ▼
    ┌────────┐   ┌────────┐   ┌──────────┐   ┌─────────┐
    │ Admin  │   │ Client │   │Notif API │   │ Predict │
    │ API    │   │ API    │   │          │   │ API     │
    │(5126)  │   │(5182)  │   │(15002)   │   │(8087)   │
    └─┬──────┘   └───┬────┘   └────┬─────┘   └────┬────┘
      │              │             │             │
      └──────────────┼─────────────┼─────────────┘
                     │             │
                     ▼             ▼
        ┌─────────────────────────────────┐
        │  Aurora PostgreSQL Database      │
        │  ├─ Hotels                       │
        │  ├─ Rooms                        │
        │  ├─ RoomAvailabilities           │
        │  ├─ Bookings                     │
        │  └─ Notifications                │
        └─┬───────────────────────────────┘
          │
          ├─► AWS SQS (Message Queue)
          ├─► AWS SNS (Email Notifications)
          ├─► AWS Cognito (Authentication)
          ├─► Redis (Cache/Session)
          └─► CloudWatch (Monitoring)
```

### Design Principles

1. **Separation of Concerns** - Each service owns its domain
2. **Stateless Services** - Horizontal scalability
3. **API-First Design** - Clear REST contracts
4. **Event-Driven** - Asynchronous notifications
5. **Cloud-Native** - Container-ready, serverless-compatible

---

## 💡 Design Decisions

### 1. Microservices Over Monolith
- **Decision**: Separated Admin, Client, and Notification services
- **Reasoning**: Independent scalability, clear boundaries, team separation
- **Trade-off**: Slight operational complexity for better maintainability

### 2. Direct API Communication vs Gateway
- **Decision**: Frontend connects directly to services
- **Reasoning**: Reduced latency, simpler debugging, flexible testing
- **Assumption**: CORS-enabled services, frontend handles routing

### 3. Shared Database with Service Separation
- **Decision**: All services use same Aurora PostgreSQL
- **Reasoning**: Simpler transactions, single source of truth
- **Assumption**: Database is integration point; eventual consistency acceptable

### 4. Asynchronous Notifications via SQS/SNS
- **Decision**: Booking notifications processed asynchronously
- **Reasoning**: Decouples services, guarantees delivery, independent scaling
- **Assumption**: 30-60 second email delays acceptable

### 5. Python ML Service
- **Decision**: Price prediction via separate Python FastAPI
- **Reasoning**: Better ML ecosystem, independent scaling, easy model updates
- **Assumption**: ML is optional; service failures don't block bookings

### 6. Booking Reference Format
- **Decision**: User-friendly unique ID (e.g., BK20240215ABC123)
- **Reasoning**: Better UX than sequential IDs, non-sequential for security
- **Trade-off**: More complex generation logic

---

## 🐛 Issues Encountered & Solutions

### Issue 1: Frontend API Port Mismatch
**Problem**: `ERR_CONNECTION_REFUSED` - frontend configured for wrong ports
**Root Cause**: Aspire assigns dynamic ports; api.js had outdated defaults
**Solution**: 
- Updated `.env` with actual ports (5126, 5182)
- Fixed api.js defaults
- Created diagnostic tools
**Status**: ✅ FIXED

### Issue 2: API Endpoint Path Inconsistencies  
**Problem**: 404 errors - endpoints don't match
**Root Cause**: Controller routes used different paths than frontend expected
**Solution**:
- Standardized paths across all controllers
- Updated frontend api.js calls
- Added documentation
**Status**: ✅ FIXED

### Issue 3: CORS Policy Blocking Requests
**Problem**: CORS errors preventing API calls
**Root Cause**: Frontend origin not in CORS allowlist
**Solution**:
- Enhanced CORS policy in all services
- Supported multiple localhost variants
- Added documentation
**Status**: ✅ FIXED

### Issue 4: Aurora PostgreSQL Connection Failures
**Problem**: Connection timeout to Aurora
**Root Cause**: Security group rules, SSL mode, connection string format
**Solution**:
- Fixed connection string format
- Updated AWS security group rules
- Added retry logic with exponential backoff
**Status**: ✅ FIXED

### Issue 5: ML Service Model Loading
**Problem**: 500 errors from predict API
**Root Cause**: Model path incorrect in Docker, dependencies missing
**Solution**:
- Fixed Dockerfile working directory
- Added health checks
- Proper model loading in startup
**Status**: ✅ FIXED

### Issue 6: SQS Notification Integration
**Problem**: Notifications not sending
**Root Cause**: AWS credentials not configured, queue URL incorrect
**Solution**:
- Added AWS SDK configuration
- Implemented async queue processor
- Created setup documentation
**Status**: ✅ FIXED

### Issue 7: AppRunner Deployment
**Problem**: Docker builds failing in AppRunner
**Root Cause**: Missing `.dockerignore` files, environment variable issues
**Solution**:
- Created `.dockerignore` in each project
- Fixed Dockerfile multi-stage builds
- Added proper health checks
**Status**: ✅ FIXED

---

## 🚀 Quick Start

### Local Development with Aspire
```bash
cd HotelBooking.AppHost
dotnet run
# Opens http://localhost:3000
```

### Docker Compose
```bash
docker-compose up --build
# Frontend: http://localhost:3000
# Admin API: http://localhost:8081/swagger
# Client API: http://localhost:8083/swagger
```

### Manual Service Startup
```bash
# Terminal 1 - Admin API
cd HotelBooking.AdminAPI
dotnet run

# Terminal 2 - Client API
cd HotelBooking.ClientAPI
dotnet run

# Terminal 3 - Notification API
cd HotelBooking.NotificationAPI
dotnet run

# Terminal 4 - Frontend
cd HotelBooking.Frontend
npm install
npm start

# Terminal 5 - Predict API (Python)
cd HotelBooking.PredictAPI
pip install -r requirements.txt
python app.py
```

---

## ✨ Features

### For Customers
✅ Hotel search with advanced filtering
✅ Real-time availability checking
✅ Booking confirmation with reference numbers
✅ Price prediction using AI/ML
✅ Booking history and cancellation
✅ Email notifications for confirmations

### For Administrators
✅ Hotel and room management
✅ Room availability configuration
✅ Pricing management
✅ Booking overview and analytics
✅ Low capacity alerts
✅ Reservation processing

### For Developers
✅ Microservices architecture
✅ Cloud-ready (AWS Aurora + AppRunner)
✅ Docker containerization
✅ Aspire orchestration for local dev
✅ Comprehensive API documentation
✅ JWT authentication and authorization
✅ CORS support for frontend integration

---

## 🛠️ Technology Stack

### Backend
- **.NET 9** - C#, ASP.NET Core
- **Entity Framework Core** - ORM
- **YARP** - API Gateway
- **JWT** - Authentication
- **Swagger** - API Documentation

### Database
- **Aurora PostgreSQL** - Primary (production)
- **PostgreSQL 16** - Local development
- **Redis** - Caching (optional)

### Frontend
- **React 19** - UI Framework
- **React Router v7** - Client-side routing
- **Axios** - HTTP Client
- **CSS3** - Styling

### ML & Predictions
- **Python FastAPI** - ML API Framework
- **scikit-learn** - ML Models
- **Uvicorn** - Python ASGI Server

### Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Local orchestration
- **AWS AppRunner** - Container hosting
- **AWS Aurora** - Managed database
- **AWS SQS/SNS** - Message queuing & notifications
- **AWS Cognito** - Authentication
- **CloudWatch** - Monitoring

---

## 📋 Prerequisites

### Required
- Node.js 16+
- .NET 8 SDK
- Docker & Docker Compose
- Git
- PostgreSQL 14+

### Optional
- Visual Studio 2022+ or VS Code
- AWS CLI
- Python 3.9+
- AWS Account (for production deployment)

---

## 🗄️ Database Setup

### Local PostgreSQL with Docker
```bash
docker run --name hotel-booking-db \
  -e POSTGRES_PASSWORD=YourPassword123! \
  -p 5432:5432 \
  -d postgres:16-alpine
```

### Aurora PostgreSQL
Connection string format:
```
Host=cluster-xxx.cluster-abcd1234.eu-central-1.rds.amazonaws.com;
Port=5432;
Database=hotel_booking;
Username=admin;
Password=YourPassword;
SSL Mode=Require;
Trust Server Certificate=true;
```

### Run Migrations
```bash
cd HotelBooking.AdminAPI
dotnet ef database update

cd ../HotelBooking.ClientAPI
dotnet ef database update

cd ../HotelBooking.NotificationAPI
dotnet ef database update
```

---

## 📡 API Documentation

### Hotel Search
```http
POST /api/v1.0/HotelSearch/search
Content-Type: application/json

{
  "destination": "Paris",
  "checkInDate": "2024-02-15",
  "checkOutDate": "2024-02-20",
  "numberOfGuests": 2,
  "numberOfRooms": 1
}
```

### Create Booking
```http
POST /api/v1.0/BookHotel/book
Content-Type: application/json

{
  "hotelId": 1,
  "roomId": 1,
  "checkInDate": "2024-02-15",
  "checkOutDate": "2024-02-20",
  "numberOfGuests": 2,
  "guestName": "John Doe",
  "guestEmail": "john@example.com"
}
```

### Add Room Availability (Admin)
```http
POST /api/v1.0/Admin/room-availability
Content-Type: application/json
Authorization: Bearer <jwt-token>

{
  "hotelId": 1,
  "roomId": 1,
  "startDate": "2024-02-15",
  "endDate": "2024-03-31",
  "availableRooms": 5,
  "pricePerNight": 150.00
}
```

### Predict Price (ML)
```http
POST /api/v1/pricing/predict
Content-Type: application/json

{
  "checkInDate": "2024-02-15",
  "nights": 5,
  "adults": 2,
  "children": 0,
  "hotelType": "City Hotel",
  "marketSegment": "Direct"
}
```

---

## 🚀 Deployment

### Docker Build
```bash
# Build all services
docker build -t hotel-booking-admin HotelBooking.AdminAPI/
docker build -t hotel-booking-client HotelBooking.ClientAPI/
docker build -t hotel-booking-notification HotelBooking.NotificationAPI/
docker build -t hotel-booking-frontend HotelBooking.Frontend/
docker build -t hotel-booking-predict HotelBooking.PredictAPI/
```

### AWS AppRunner Deployment
1. Push Docker images to AWS ECR
2. Create AppRunner services
3. Configure environment variables
4. Connect to Aurora PostgreSQL
5. Enable auto-scaling

---

## ⚙️ Configuration

### Frontend .env
```env
REACT_APP_ADMIN_API_URL=http://localhost:5126/api/v1.0
REACT_APP_CLIENT_API_URL=http://localhost:5182/api/v1.0
REACT_APP_NOTIFICATION_API_URL=http://localhost:15002/api/v1.0
REACT_APP_PREDICT_API_URL=http://localhost:8087/api/v1
```

### Backend appsettings.json
```json
{
  "ConnectionStrings": {
    "HotelBookingDb": "Host=localhost;Port=5432;Database=hotel_booking;..."
  },
  "JwtSettings": {
    "SecretKey": "Your32CharacterMinimumSecretKey!",
    "Issuer": "HotelBookingAPI",
    "Audience": "HotelBookingAPIUsers"
  }
}
```

---

## 🔍 Troubleshooting

### API Connection Issues
```powershell
# Windows - Run diagnostics
powershell -ExecutionPolicy Bypass -File verify-config.ps1
powershell -ExecutionPolicy Bypass -File frontend-api-diagnostics.ps1
```

### Database Connection Issues
```bash
# Test PostgreSQL
psql -U postgres -d hotel_booking -c "SELECT 1"

# Check Docker logs
docker logs hotel-booking-db
```

### CORS Errors
- Verify frontend origin in API's CORS policy
- Check appsettings.json CORS configuration
- Clear browser cache and hard refresh

### ML Service Issues
- Verify model file exists: `artifacts/adr_model.joblib`
- Check Python dependencies: `pip install -r requirements.txt`
- Review FastAPI logs for errors

---

**Version**: 1.0.0 | **Status**: ✅ Production Ready | **Last Updated**: January 14, 2026

