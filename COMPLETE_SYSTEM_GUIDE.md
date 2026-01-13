# ?? Complete System - Quick Start

## ? All Services Ready!

Your Hotel Booking system now includes:
- **4 Microservices** (Admin, Client, Notification, Predict)
- **Frontend** (React + Nginx)
- **Database** (PostgreSQL)
- **Cache** (Redis)

---

## ?? One-Command Start

```bash
docker-compose up -d --build
```

---

## ?? Access Everything

| Service | URL | Purpose |
|---------|-----|---------|
| **?? Frontend** | http://localhost:3000 | User Interface |
| **?? Admin API** | http://localhost:8081/swagger | Hotel Management |
| **?? Client API** | http://localhost:8083/swagger | Search & Booking |
| **?? Notification API** | http://localhost:8085/swagger | Notifications |
| **?? Predict API** | http://localhost:8087/docs | ML Pricing |
| **??? PostgreSQL** | localhost:5433 | Database |
| **? Redis** | localhost:6379 | Cache |

---

## ?? Service Status

```bash
# Check all services
docker-compose ps

# Expected:
# ? hotelbooking-frontend       (Port 3000)
# ? hotelbooking-adminapi        (Port 8081)
# ? hotelbooking-clientapi       (Port 8083)
# ? hotelbooking-notificationapi (Port 8085)
# ? hotelbooking-predictapi      (Port 8087)
# ? hotelbooking-postgres        (Port 5433)
# ? hotelbooking-redis           (Port 6379)
```

---

## ?? Common Commands

### **Start/Stop**
```bash
# Start everything
docker-compose up -d

# Stop everything
docker-compose down

# Restart specific service
docker-compose restart predictapi
```

### **Logs**
```bash
# All logs
docker-compose logs -f

# Specific service
docker-compose logs -f predictapi
docker-compose logs -f adminapi
```

### **Rebuild**
```bash
# Rebuild all
docker-compose build --no-cache

# Rebuild specific service
docker-compose build --no-cache predictapi

# Rebuild and restart
docker-compose up -d --build
```

---

## ?? Quick Tests

### **Test 1: Frontend**
```bash
curl http://localhost:3000
# Expected: HTML with React app
```

### **Test 2: APIs**
```bash
curl http://localhost:8081/health  # AdminAPI
curl http://localhost:8083/health  # ClientAPI
curl http://localhost:8085/health  # NotificationAPI
curl http://localhost:8087/health  # PredictAPI
```

### **Test 3: ML Prediction**
```bash
curl -X POST http://localhost:8087/api/v1/pricing/predict \
  -H "Content-Type: application/json" \
  -d '{
    "checkInDate": "2026-03-14",
    "nights": 2,
    "adults": 2,
    "children": 0,
    "hotelType": "City Hotel",
    "marketSegment": "Direct",
    "customerType": "Transient",
    "depositType": "No Deposit",
    "meal": "BB",
    "isRepeatedGuest": 0,
    "specialRequests": 0,
    "leadTimeDays": 14
  }'
```

---

## ?? Project Structure

```
HotelBooking/
??? HotelBooking.AdminAPI/
?   ??? Dockerfile ?
?   ??? .dockerignore ?
??? HotelBooking.ClientAPI/
?   ??? Dockerfile ?
?   ??? .dockerignore ?
??? HotelBooking.NotificationAPI/
?   ??? Dockerfile ?
?   ??? .dockerignore ?
??? HotelBooking.PredictAPI/
?   ??? Dockerfile ?
?   ??? .dockerignore ?
?   ??? app.py ?
?   ??? train.py ?
?   ??? requirements.txt ?
?   ??? artifacts/
?       ??? adr_model.joblib ?
??? HotelBooking.Frontend/
?   ??? Dockerfile ?
?   ??? nginx.conf ?
?   ??? .dockerignore ?
?   ??? .env.example ?
??? docker-compose.yml ?
??? .env.example ?
??? Documentation/
    ??? DOCKER_SUCCESS.md
    ??? SWAGGER_FIXED.md
    ??? FRONTEND_DOCKER_GUIDE.md
    ??? PREDICTAPI_DOCKER_GUIDE.md ?
```

---

## ?? Success Checklist

After running `docker-compose up -d --build`:

- ? All 7 containers running
- ? All health checks passing
- ? Frontend accessible at port 3000
- ? APIs accessible with Swagger
- ? PredictAPI accessible at port 8087
- ? Database ready on port 5433
- ? Redis ready on port 6379

---

## ?? What You Can Do Now

### **1. Use the Frontend**
```
http://localhost:3000
```
- Search for hotels
- View hotel details
- Create bookings
- Get ML-predicted pricing
- Manage reservations
- Admin dashboard

### **2. Test APIs with Swagger**
```
http://localhost:8081/swagger (Admin - .NET)
http://localhost:8083/swagger (Client - .NET)
http://localhost:8085/swagger (Notification - .NET)
http://localhost:8087/docs (Predict - Python FastAPI)
```
- Interactive API testing
- View all endpoints
- See request/response models
- Try authenticated endpoints

### **3. Test ML Predictions**
```
http://localhost:8087/docs
```
- Predict hotel prices
- Dynamic pricing based on:
  - Check-in date
  - Number of nights
  - Guest count
  - Hotel type
  - Market segment
  - Lead time

### **4. Access Database**
```bash
psql -h localhost -p 5433 -U admin -d hotel_booking
```

### **5. Monitor Services**
```bash
# View logs
docker-compose logs -f

# Check status
docker-compose ps

# Resource usage
docker stats
```

---

## ??? Tech Stack

| Service | Technology | Port |
|---------|------------|------|
| Frontend | React 19 + Nginx | 3000 |
| AdminAPI | .NET 9 + ASP.NET Core | 8081 |
| ClientAPI | .NET 9 + ASP.NET Core | 8083 |
| NotificationAPI | .NET 9 + ASP.NET Core | 8085 |
| **PredictAPI** | **Python 3.11 + FastAPI** | **8087** |
| Database | PostgreSQL 16 | 5433 |
| Cache | Redis 7 | 6379 |

---

## ?? Deploy to Production

### **Option 1: Push to Container Registry**
```bash
# Tag images
docker tag hotelbooking-frontend YOUR_REGISTRY/frontend:latest
docker tag hotelbooking-adminapi YOUR_REGISTRY/adminapi:latest
docker tag hotelbooking-clientapi YOUR_REGISTRY/clientapi:latest
docker tag hotelbooking-notificationapi YOUR_REGISTRY/notificationapi:latest
docker tag hotelbooking-predictapi YOUR_REGISTRY/predictapi:latest

# Push
docker push YOUR_REGISTRY/frontend:latest
docker push YOUR_REGISTRY/adminapi:latest
docker push YOUR_REGISTRY/clientapi:latest
docker push YOUR_REGISTRY/notificationapi:latest
docker push YOUR_REGISTRY/predictapi:latest
```

### **Option 2: Deploy to AWS ECS**
```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name hotel-booking

# Create task definitions for each service
# Deploy services with load balancer
# Configure auto-scaling
```

### **Option 3: Deploy to Kubernetes**
```bash
# Convert docker-compose to K8s manifests
# Apply to cluster
kubectl apply -f k8s/
```

---

## ?? Documentation

- ? `DOCKER_SUCCESS.md` - Docker deployment success
- ? `SWAGGER_FIXED.md` - Swagger UI configuration
- ? `FRONTEND_DOCKER_GUIDE.md` - Frontend setup guide
- ? `PREDICTAPI_DOCKER_GUIDE.md` - ML API setup guide
- ? `DOCKER_COMPLETE_GUIDE.md` - Complete Docker reference
- ? `DOCKER_QUICK_REFERENCE.md` - Quick commands

---

## ?? You're All Set!

**Your complete Hotel Booking system with ML is running!**

```bash
# Start everything
docker-compose up -d --build

# Access services
start http://localhost:3000         # Frontend
start http://localhost:8081/swagger # Admin API
start http://localhost:8083/swagger # Client API
start http://localhost:8085/swagger # Notification API
start http://localhost:8087/docs    # Predict API (ML)

# Check status
docker-compose ps
```

**Enjoy your microservices architecture with ML-powered pricing!** ??
