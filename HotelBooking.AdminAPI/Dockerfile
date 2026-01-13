# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY HotelBooking.AdminAPI/HotelBooking.AdminAPI.csproj HotelBooking.AdminAPI/
COPY HotelBooking.ServiceDefaults/HotelBooking.ServiceDefaults.csproj HotelBooking.ServiceDefaults/

# Restore dependencies
RUN dotnet restore HotelBooking.AdminAPI/HotelBooking.AdminAPI.csproj

# Copy source code
COPY HotelBooking.AdminAPI/ HotelBooking.AdminAPI/
COPY HotelBooking.ServiceDefaults/ HotelBooking.ServiceDefaults/

# Build and publish in one step
WORKDIR /src/HotelBooking.AdminAPI
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

# Run the application
ENTRYPOINT ["dotnet", "HotelBooking.AdminAPI.dll"]
