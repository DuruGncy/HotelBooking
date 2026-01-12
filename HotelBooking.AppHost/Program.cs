var builder = DistributedApplication.CreateBuilder(args);

// NOTE: Redis removed temporarily - start Docker Desktop to enable caching
var cache = builder.AddRedis("cache");

// Add the Admin API - for managing hotel rooms and availability
var adminApi = builder.AddProject<Projects.HotelBooking_AdminAPI>("adminapi")
    .WithExternalHttpEndpoints();

// Add the Client API - for searching and booking hotels
var clientApi = builder.AddProject<Projects.HotelBooking_ClientAPI>("clientapi")
    .WithExternalHttpEndpoints();

// Add the Notification API - for sending notifications and alerts
var notificationApi = builder.AddProject<Projects.HotelBooking_NotificationAPI>("notificationapi")
    .WithExternalHttpEndpoints();

// Add the Gateway project with references to all APIs
var gateway = builder.AddProject<Projects.HotelBookingGateway>("gateway")
    .WithReference(adminApi)
    .WithReference(clientApi)
    .WithReference(notificationApi)
    .WithExternalHttpEndpoints();

// Add the React frontend
var frontend = builder.AddNpmApp("frontend", "../HotelBooking.Frontend", "start")
    .WithReference(gateway)
    .WithHttpEndpoint(env: "PORT", port: 3000)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
