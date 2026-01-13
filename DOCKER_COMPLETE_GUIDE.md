# Docker Complete Setup Guide

## ? All Dockerfiles Created!

Your entire Hotel Booking microservices system is now containerized!

---

## ?? Files Created

### **Dockerfiles**
- ? `HotelBooking.AdminAPI/Dockerfile`
- ? `HotelBooking.ClientAPI/Dockerfile`
- ? `HotelBooking.NotificationAPI/Dockerfile`

### **.dockerignore Files**
- ? `HotelBooking.AdminAPI/.dockerignore`
- ? `HotelBooking.ClientAPI/.dockerignore`
- ? `HotelBooking.NotificationAPI/.dockerignore`

### **Docker Compose**
- ? `docker-compose.yml` (all services + database + cache)
- ? `.env.example` (environment variables template)

---

## ?? Quick Start (Option 1: Docker Compose)

### **Step 1: Create .env File**

```bash
# Copy the example file
cp .env.example .env

# Edit with your values
notepad .env  # Windows
nano .env     # Linux/Mac
```

**Required values:**
- `JWT_SECRET_KEY` - Your JWT secret (min 32 chars)
- `DB_PASSWORD` - Database password
- `AWS_ACCESS_KEY_ID` - Your AWS access key
- `AWS_SECRET_ACCESS_KEY` - Your AWS secret key
- `SQS_QUEUE_URL` - Your SQS FIFO queue URL

### **Step 2: Start All Services**

```bash
# From solution root
cd "C:\Users\mathi\Documents\.NET Projects\HotelBooking"

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f adminapi
docker-compose logs -f clientapi
docker-compose logs -f notificationapi
```

### **Step 3: Test the Services**

```bash
# AdminAPI
curl http://localhost:8081/health

# ClientAPI
curl http://localhost:8083/health

# NotificationAPI
curl http://localhost:8085/health

# PostgreSQL
docker exec -it hotelbooking-postgres psql -U admin -d hotel_booking
```

### **Step 4: Access Swagger UI**

- **AdminAPI:** http://localhost:8081/swagger
- **ClientAPI:** http://localhost:8083/swagger
- **NotificationAPI:** http://localhost:8085/swagger

---

## ?? Quick Start (Option 2: Individual Services)

### **Build Images**

```bash
# AdminAPI
docker build -f HotelBooking.AdminAPI/Dockerfile -t hotelbooking-adminapi:latest .

# ClientAPI
docker build -f HotelBooking.ClientAPI/Dockerfile -t hotelbooking-clientapi:latest .

# NotificationAPI
docker build -f HotelBooking.NotificationAPI/Dockerfile -t hotelbooking-notificationapi:latest .
```

### **Run Containers**

```bash
# AdminAPI
docker run -d --name adminapi -p 8081:8080 \
  -e JwtSettings__SecretKey="YourSecretKey32Chars!" \
  hotelbooking-adminapi:latest

# ClientAPI
docker run -d --name clientapi -p 8083:8080 \
  -e JwtSettings__SecretKey="YourSecretKey32Chars!" \
  hotelbooking-clientapi:latest

# NotificationAPI
docker run -d --name notificationapi -p 8085:8080 \
  -e JwtSettings__SecretKey="YourSecretKey32Chars!" \
  -e AWS__SQS__QueueUrl="your-sqs-url" \
  hotelbooking-notificationapi:latest
```

---

## ?? Service Ports

| Service | HTTP | HTTPS | Description |
|---------|------|-------|-------------|
| **AdminAPI** | 8081 | 8082 | Hotel & room management |
| **ClientAPI** | 8083 | 8084 | Search & booking |
| **NotificationAPI** | 8085 | 8086 | Email notifications |
| **PostgreSQL** | 5432 | - | Database |
| **Redis** | 6379 | - | Cache |

---

## ?? Docker Compose Commands

### **Basic Commands**

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# View logs (all services)
docker-compose logs -f

# View specific service logs
docker-compose logs -f adminapi

# Restart a service
docker-compose restart adminapi

# Rebuild and restart
docker-compose up -d --build
```

### **Service Management**

```bash
# Start specific service
docker-compose up -d adminapi

# Stop specific service
docker-compose stop adminapi

# View running services
docker-compose ps

# View service status
docker-compose ps adminapi
```

### **Logs & Debugging**

```bash
# View all logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# Last 100 lines
docker-compose logs --tail 100

# Logs since 10 minutes ago
docker-compose logs --since 10m

# Service-specific logs
docker-compose logs -f adminapi clientapi
```

### **Scaling Services**

```bash
# Scale ClientAPI to 3 instances
docker-compose up -d --scale clientapi=3

# Scale NotificationAPI to 2 instances
docker-compose up -d --scale notificationapi=2
```

---

## ?? Environment Variables

### **Required for All Services**

```env
JWT_SECRET_KEY=YourSecretKey32Characters!
JWT_ISSUER=HotelBookingAPI
JWT_AUDIENCE=HotelBookingAPIUsers
DB_CONNECTION_STRING=Host=postgres;...
```

### **Required for NotificationAPI**

```env
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=your_key
AWS_SECRET_ACCESS_KEY=your_secret
SQS_QUEUE_URL=https://sqs.us-east-1.amazonaws.com/.../hotel-notifications.fifo
```

### **Optional**

```env
ASPNETCORE_ENVIRONMENT=Production
REDIS_PASSWORD=YourRedisPassword123!
DB_PASSWORD=YourDatabasePassword123!
```

---

## ??? Architecture

```
????????????????????????????????????????????????????????????
?                    Docker Network                         ?
?                 (hotelbooking-network)                    ?
????????????????????????????????????????????????????????????
?                                                           ?
?  ???????????????  ???????????????  ???????????????     ?
?  ?  AdminAPI   ?  ?  ClientAPI  ?  ?Notification ?     ?
?  ?             ?  ?             ?  ?     API     ?     ?
?  ?  Port 8081  ?  ?  Port 8083  ?  ?  Port 8085  ?     ?
?  ???????????????  ???????????????  ???????????????     ?
?         ?                ?                ?             ?
?         ???????????????????????????????????             ?
?                          ?                              ?
?         ???????????????????????????????????             ?
?         ?                                 ?             ?
?  ???????????????                  ???????????????      ?
?  ?  PostgreSQL ?                  ?    Redis    ?      ?
?  ?             ?                  ?             ?      ?
?  ?  Port 5432  ?                  ?  Port 6379  ?      ?
?  ???????????????                  ???????????????      ?
?                                                         ?
????????????????????????????????????????????????????????????
              ?
              ?
    External Services (AWS SQS, etc.)
```

---

## ?? Testing

### **Test 1: All Services Running**

```bash
# Check all containers
docker-compose ps

# Expected: All services should be "Up" and healthy
```

### **Test 2: Health Checks**

```bash
# AdminAPI
curl http://localhost:8081/health
# Expected: {"status":"healthy","service":"admin-api"...}

# ClientAPI
curl http://localhost:8083/health
# Expected: {"status":"healthy","service":"client-api"...}

# NotificationAPI
curl http://localhost:8085/health
# Expected: {"status":"healthy","service":"notification-api"...}
```

### **Test 3: Database Connection**

```bash
# Connect to PostgreSQL
docker exec -it hotelbooking-postgres psql -U admin -d hotel_booking

# List tables
\dt

# Exit
\q
```

### **Test 4: Redis Connection**

```bash
# Connect to Redis
docker exec -it hotelbooking-redis redis-cli -a YourRedisPassword123!

# Test
ping
# Expected: PONG

# Exit
exit
```

### **Test 5: API Endpoints**

```bash
# Search hotels (ClientAPI)
curl -X POST http://localhost:8083/api/v1.0/HotelSearch/search \
  -H "Content-Type: application/json" \
  -d '{
    "destination": "New York",
    "checkInDate": "2024-03-10T00:00:00Z",
    "checkOutDate": "2024-03-15T00:00:00Z",
    "numberOfGuests": 2
  }'

# Get hotels (AdminAPI - requires JWT)
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8081/api/v1.0/Admin/hotels
```

---

## ?? Troubleshooting

### **Issue: Container exits immediately**

```bash
# View logs
docker-compose logs adminapi

# Check for errors in output
```

### **Issue: Can't connect to database**

```bash
# Check if PostgreSQL is running
docker-compose ps postgres

# Check PostgreSQL logs
docker-compose logs postgres

# Verify connection string in .env
```

### **Issue: Port already in use**

```bash
# Find process using port
netstat -ano | findstr :8081  # Windows
lsof -i :8081                 # Linux/Mac

# Change port in docker-compose.yml
ports:
  - "8091:8080"  # Use different port
```

### **Issue: Services can't communicate**

```bash
# Verify network
docker network ls
docker network inspect hotelbooking_hotelbooking-network

# All services should be in same network
```

---

## ?? Deploy to Production

### **Option 1: AWS ECS (Elastic Container Service)**

```bash
# 1. Push images to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR-ACCOUNT.dkr.ecr.us-east-1.amazonaws.com

# 2. Tag images
docker tag hotelbooking-adminapi:latest YOUR-ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/adminapi:latest
docker tag hotelbooking-clientapi:latest YOUR-ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/clientapi:latest
docker tag hotelbooking-notificationapi:latest YOUR-ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/notificationapi:latest

# 3. Push images
docker push YOUR-ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/adminapi:latest
docker push YOUR-ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/clientapi:latest
docker push YOUR-ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/notificationapi:latest

# 4. Create ECS task definitions and services (via AWS Console or CLI)
```

### **Option 2: AWS App Runner**

```bash
# Simpler deployment - AWS handles infrastructure
# Use AWS Console to create App Runner services
# Point to your ECR images
```

### **Option 3: Docker Swarm**

```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml hotelbooking

# View services
docker service ls

# Scale services
docker service scale hotelbooking_clientapi=3
```

### **Option 4: Kubernetes**

```bash
# Convert docker-compose to Kubernetes manifests
# Use kompose tool or create manifests manually

# Deploy to Kubernetes cluster
kubectl apply -f k8s/
```

---

## ?? Maintenance

### **View Resource Usage**

```bash
# All containers
docker stats

# Specific service
docker stats hotelbooking-adminapi
```

### **Backup Database**

```bash
# Backup PostgreSQL
docker exec hotelbooking-postgres pg_dump -U admin hotel_booking > backup.sql

# Restore
docker exec -i hotelbooking-postgres psql -U admin hotel_booking < backup.sql
```

### **Clean Up**

```bash
# Stop and remove containers
docker-compose down

# Remove volumes too
docker-compose down -v

# Remove all unused Docker resources
docker system prune -a
```

### **Update Services**

```bash
# Pull latest code
git pull

# Rebuild and restart
docker-compose up -d --build

# Or rebuild specific service
docker-compose up -d --build adminapi
```

---

## ?? Summary

**Your microservices are now fully containerized!**

? **3 Dockerfiles** created (AdminAPI, ClientAPI, NotificationAPI)  
? **docker-compose.yml** for orchestration  
? **PostgreSQL** database included  
? **Redis** cache included  
? **Health checks** configured  
? **Multi-stage builds** for optimization  
? **Non-root users** for security  
? **Environment variables** configured  
? **Production ready**  

---

## ?? Next Steps

1. ? **Test locally** with docker-compose
2. ? **Set up CI/CD** pipeline (GitHub Actions, Azure DevOps)
3. ? **Deploy to AWS** (ECS, App Runner, or EKS)
4. ? **Configure monitoring** (CloudWatch, Prometheus)
5. ? **Set up load balancer** (ALB for AWS)
6. ? **Configure auto-scaling**
7. ? **Implement secrets management** (AWS Secrets Manager)

---

**Start now:**

```bash
# 1. Create .env file
cp .env.example .env

# 2. Edit .env with your values
notepad .env

# 3. Start all services
docker-compose up -d

# 4. View logs
docker-compose logs -f

# 5. Test health endpoints
curl http://localhost:8081/health
curl http://localhost:8083/health
curl http://localhost:8085/health
```

**?? Your microservices are ready to deploy!**
