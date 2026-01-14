using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using HotelBooking.NotificationAPI.Services;
using HotelBooking.NotificationAPI.Services.Interfaces;
using HotelBooking.NotificationAPI.Services.Background;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using HotelBooking.NotificationAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// ===== ADD AURORA POSTGRESQL DBCONTEXT =====
var connectionString = builder.Configuration.GetConnectionString("HotelBookingDb");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<NotificationDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });
    });
    
    Console.WriteLine("? Aurora PostgreSQL configured for NotificationAPI");
}
else
{
    Console.WriteLine("?? No database connection string found - using in-memory fallback");
}
// ===========================================

// Add services to the container.
builder.Services.AddControllers();

// Register Notification Service
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register Background Service for notification processing
builder.Services.AddHostedService<NotificationBackgroundService>();
// Register SQS polling hosted service to process notifications continuously
builder.Services.AddHostedService<HotelBooking.NotificationAPI.Services.Background.SqsPollingService>();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version"));
});

// Add API Explorer for versioned APIs
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with service provider
builder.Services.ConfigureOptions<HotelBooking.NotificationAPI.ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourDefaultSecretKeyForDevelopmentMinimum32Chars!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "HotelBookingAPI",
        ValidAudience = jwtSettings["Audience"] ?? "HotelBookingAPIUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("NotificationPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ===== AUTO-CHECK DATABASE CONNECTION =====
if (!string.IsNullOrEmpty(connectionString))
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            
            if (dbContext.Database.CanConnect())
            {
                Console.WriteLine("? Connected to Aurora PostgreSQL");
                Console.WriteLine($"?? Hotels: {dbContext.Hotels.Count()}");
                Console.WriteLine($"?? Bookings: {dbContext.Bookings.Count()}");
                Console.WriteLine($"?? Notifications: {dbContext.Notifications.Count()}");
            }
            else
            {
                Console.WriteLine("? Cannot connect to database");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Database error: {ex.Message}");
        }
    }
}
// ===========================================

// Map default endpoints (for Aspire).
app.MapDefaultEndpoints();

// Enable Swagger in all environments (removed environment check)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    // Create a Swagger endpoint for each version
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"Hotel Booking Notification API {description.GroupName.ToUpperInvariant()}");
    }
});

app.UseHttpsRedirection();
app.UseCors("NotificationPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "notification-api" }))
   .WithName("HealthCheck");

app.Run();
