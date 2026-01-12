using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using HotelBookingAPI.Services;
using HotelBookingAPI.Services.Interfaces;
using HotelBookingAPI.Services.Background;

namespace HotelBookingAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Register services
        builder.Services.AddScoped<IAdminHotelService, AdminHotelService>();
        builder.Services.AddScoped<IHotelSearchService, HotelSearchService>();
        builder.Services.AddScoped<IBookHotelService, BookHotelService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        
        // Register background service for scheduled tasks
        builder.Services.AddHostedService<NotificationBackgroundService>();
        
        // Add API Versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
        
        // Add API Explorer for versioned APIs
        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        
        // Add Swagger for API documentation
        builder.Services.AddEndpointsApiExplorer();
        
        // Configure Swagger with service provider
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
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

        // Add Authorization
        builder.Services.AddAuthorization();

        // Add CORS for admin.hotels.com domain and public users
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.WithOrigins("https://admin.hotels.com", "http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
            
            options.AddPolicy("PublicPolicy", policy =>
            {
                policy.WithOrigins("https://www.hotels.com", "http://localhost:3001")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

        // Seed test data (remove in production)
        AdminHotelService.SeedTestData();
        
        // Initialize services with shared data using reflection
        var hotels = typeof(AdminHotelService)
            .GetField("_hotels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
            .GetValue(null) as List<HotelBookingAPI.Models.Hotel> ?? new();
        var rooms = typeof(AdminHotelService)
            .GetField("_rooms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
            .GetValue(null) as List<HotelBookingAPI.Models.Room> ?? new();
        var availabilities = typeof(AdminHotelService)
            .GetField("_availabilities", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
            .GetValue(null) as List<HotelBookingAPI.Models.RoomAvailability> ?? new();
        var bookings = BookHotelService.GetAllBookings();
        
        BookHotelService.InitializeData(hotels, rooms, availabilities);
        NotificationService.InitializeData(hotels, rooms, availabilities, bookings);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                // Create a Swagger endpoint for each version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"Hotel Booking API {description.GroupName.ToUpperInvariant()}");
                }
            });
        }

        app.UseHttpsRedirection();

        // Use CORS - apply both policies
        app.UseCors("AdminPolicy");

        // Add Authentication & Authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

