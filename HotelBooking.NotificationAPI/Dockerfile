# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY HotelBooking.NotificationAPI/HotelBooking.NotificationAPI.csproj HotelBooking.NotificationAPI/
COPY HotelBooking.ServiceDefaults/HotelBooking.ServiceDefaults.csproj HotelBooking.ServiceDefaults/

# Restore dependencies
RUN dotnet restore HotelBooking.NotificationAPI/HotelBooking.NotificationAPI.csproj

# Copy source code
COPY HotelBooking.NotificationAPI/ HotelBooking.NotificationAPI/
COPY HotelBooking.ServiceDefaults/ HotelBooking.ServiceDefaults/

# Build and publish in one step
WORKDIR /src/HotelBooking.NotificationAPI
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser

# Copy published files
COPY --from=build /app/publish .

# Expose port (HTTP only)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "HotelBooking.NotificationAPI.dll"]
