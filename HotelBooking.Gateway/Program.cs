using Yarp.ReverseProxy.Configuration;
using HotelBookingGateway.Middleware;
using Microsoft.OpenApi.Models;
using HotelBookingGateway.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add service discovery
builder.Services.AddServiceDiscovery();

// Configure HTTP client with service discovery
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    http.AddServiceDiscovery();
});

// Required for Swagger UI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger for Gateway with service documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel Booking API Gateway",
        Version = "v1.0",
        Description = @"
# Hotel Booking API Gateway

This gateway provides access to all Hotel Booking microservices through a single entry point.

## Connected Services

### 1. AdminAPI
**Purpose:** Hotel and room management for administrators

**Base Path:** `/api/v1.0/Admin`

**Key Endpoints:**
- `GET /api/v1.0/Admin/hotels` - Get all hotels
- `GET /api/v1.0/Admin/rooms` - Get all rooms
- `POST /api/v1.0/Admin/room-availability` - Add room availability
- `PUT /api/v1.0/Admin/room-availability` - Update room availability
- `DELETE /api/v1.0/Admin/room-availability/{id}` - Delete room availability

**Authentication:** Requires ADMIN JWT token

---

### 2. ClientAPI - HotelSearch
**Purpose:** Search and discover available hotels

**Base Path:** `/api/v1.0/HotelSearch`

**Key Endpoints:**
- `POST /api/v1.0/HotelSearch/search` - Search hotels by criteria
- `GET /api/v1.0/HotelSearch/hotels/{id}` - Get hotel details
- `GET /api/v1.0/HotelSearch/quick-search` - Quick search with query params

**Authentication:** Public (no authentication required)

---

### 3. ClientAPI - BookHotel
**Purpose:** Hotel booking management

**Base Path:** `/api/v1.0/BookHotel`

**Key Endpoints:**
- `POST /api/v1.0/BookHotel/book` - Create a booking
- `GET /api/v1.0/BookHotel/{id}` - Get booking by ID
- `GET /api/v1.0/BookHotel/reference/{reference}` - Get booking by reference
- `POST /api/v1.0/BookHotel/{id}/cancel` - Cancel booking
- `GET /api/v1.0/BookHotel/check-availability` - Check room availability

**Authentication:** Optional (guest bookings allowed)

---

### 4. NotificationAPI
**Purpose:** Notification and alert management

**Base Path:** `/api/v1.0/Notifications`

**Key Endpoints:**
- `GET /api/v1.0/Notifications/pending` - Get pending notifications
- `GET /api/v1.0/Notifications/hotel/{hotelId}` - Get hotel notifications
- `POST /api/v1.0/Notifications/trigger/low-capacity-check` - Trigger capacity check
- `POST /api/v1.0/Notifications/trigger/process-reservations` - Trigger reservation processing

**Authentication:** Requires ADMIN JWT token

---

### 5. PredictAPI
**Purpose:** Price prediction for bookings (machine learning service)

**Base Path:** `/api/v1/pricing`

**Key Endpoints:**
- `POST /api/v1/pricing/predict` - Predict price for a booking request payload
- `GET /health` - Predict service health (model loaded)

**Authentication:** Public (gateway forwards requests)

---

## Service Health Checks

- `GET /admin/health` - AdminAPI health
- `GET /client/health` - ClientAPI health
- `GET /notification/health` - NotificationAPI health

## Gateway Endpoints

- `GET /health` - Gateway health check
- `GET /services` - Service discovery

## Authentication

Most admin endpoints require a JWT Bearer token:
```
Authorization: Bearer <your-jwt-token>
```

## Versioning

All APIs support versioning:
- **URL:** `/api/v1.0/{controller}/{action}`
- **Header:** `X-Api-Version: 1.0`
- **Query:** `?api-version=1.0`

## Direct Service Access

You can also access services directly (when not going through gateway):
- **AdminAPI Swagger:** `https://localhost:7XXX/swagger`
- **ClientAPI Swagger:** `https://localhost:7XXX/swagger`
- **NotificationAPI Swagger:** `https://localhost:7XXX/swagger`

_(Ports are assigned dynamically by Aspire - check Aspire Dashboard)_
",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@hotelbookingapi.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Add JWT authentication to Swagger (for testing admin endpoints)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token for admin endpoints"
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
    // Add PredictAPI stub to the generated OpenAPI document
    options.DocumentFilter<PredictDocumentFilter>();
});

// Memory cache for rate limiting
builder.Services.AddMemoryCache();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add YARP Reverse Proxy with dynamic configuration
builder.Services.AddReverseProxy()
    .LoadFromMemory(new[]
    {
        // Admin API routes - versioned
        new RouteConfig
        {
            RouteId = "admin_api_versioned",
            Match = new RouteMatch
            {
                Path = "/api/v{version}/Admin/{**catch-all}"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/api/v{version}/Admin/{**catch-all}" }
                }
            },
            ClusterId = "admin_cluster"
        },
        // Admin API routes - legacy (no version)
        new RouteConfig
        {
            RouteId = "admin_api_legacy",
            Match = new RouteMatch
            {
                Path = "/api/Admin/{**catch-all}"
            },
            ClusterId = "admin_cluster"
        },
        // Client API routes - HotelSearch versioned
        new RouteConfig
        {
            RouteId = "client_search_versioned",
            Match = new RouteMatch
            {
                Path = "/api/v{version}/HotelSearch/{**catch-all}"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/api/v{version}/HotelSearch/{**catch-all}" }
                }
            },
            ClusterId = "client_cluster"
        },
        // Client API routes - BookHotel versioned
        new RouteConfig
        {
            RouteId = "client_book_versioned",
            Match = new RouteMatch
            {
                Path = "/api/v{version}/BookHotel/{**catch-all}"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/api/v{version}/BookHotel/{**catch-all}" }
                }
            },
            ClusterId = "client_cluster"
        },
        // Client API routes - legacy (no version)
        new RouteConfig
        {
            RouteId = "client_api_legacy",
            Match = new RouteMatch
            {
                Path = "/api/{controller:regex(^(HotelSearch|BookHotel)$)}/{**catch-all}"
            },
            ClusterId = "client_cluster"
        },
        // Notification API routes - versioned
        new RouteConfig
        {
            RouteId = "notification_api_versioned",
            Match = new RouteMatch
            {
                Path = "/api/v{version}/Notifications/{**catch-all}"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/api/v{version}/Notifications/{**catch-all}" }
                }
            },
            ClusterId = "notification_cluster"
        },
        // Notification API routes - legacy (no version)
        new RouteConfig
        {
            RouteId = "notification_api_legacy",
            Match = new RouteMatch
            {
                Path = "/api/Notifications/{**catch-all}"
            },
            ClusterId = "notification_cluster"
        },
        // Predict API routes - versioned
        new RouteConfig
        {
            RouteId = "predict_api_versioned",
            Match = new RouteMatch
            {
                Path = "/api/v{version}/pricing/{**catch-all}"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/api/v{version}/pricing/{**catch-all}" }
                }
            },
            ClusterId = "predict_cluster"
        },
        // Predict API routes - legacy (no version)
        new RouteConfig
        {
            RouteId = "predict_api_legacy",
            Match = new RouteMatch
            {
                Path = "/api/pricing/{**catch-all}"
            },
            ClusterId = "predict_cluster"
        },
        // Health check routes for each service
        new RouteConfig
        {
            RouteId = "admin_health",
            Match = new RouteMatch
            {
                Path = "/admin/health"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/health" }
                }
            },
            ClusterId = "admin_cluster"
        },
        new RouteConfig
        {
            RouteId = "client_health",
            Match = new RouteMatch
            {
                Path = "/client/health"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/health" }
                }
            },
            ClusterId = "client_cluster"
        },
        new RouteConfig
        {
            RouteId = "notification_health",
            Match = new RouteMatch
            {
                Path = "/notification/health"
            },
            Transforms = new[]
            {
                new Dictionary<string, string>
                {
                    { "PathPattern", "/health" }
                }
            },
            ClusterId = "notification_cluster"
        }
    },
    new[]
    {
        new ClusterConfig
        {
            ClusterId = "admin_cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "admin", new DestinationConfig { Address = "http://adminapi" } }
            }
        },
        new ClusterConfig
        {
            ClusterId = "client_cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "client", new DestinationConfig { Address = "http://clientapi" } }
            }
        },
        new ClusterConfig
        {
            ClusterId = "notification_cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "notification", new DestinationConfig { Address = "http://notificationapi" } }
            }
        }
        ,
        new ClusterConfig
        {
            ClusterId = "predict_cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "predict", new DestinationConfig { Address = "http://predictapi:8000" } }
            }
        }
    });

var app = builder.Build();

// Map default endpoints (for Aspire)
app.MapDefaultEndpoints();

// Use Gateway Middleware for logging
app.UseGatewayMiddleware();

// Enable CORS before routing
app.UseCors();

app.UseRouting();

// Enable Swagger in all environments (for gateway documentation)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Booking API Gateway v1.0");
    options.RoutePrefix = "swagger"; // Access at /swagger
    options.DocumentTitle = "Hotel Booking API Gateway";
    options.EnableDeepLinking();
    options.DisplayRequestDuration();
    options.EnableFilter();
    options.ShowExtensions();
    
    // Add custom CSS for better styling
    options.InjectStylesheet("/swagger-ui/custom.css");
    options.InjectJavascript("/swagger-ui/custom.js");
});

// Serve custom CSS for Swagger
app.MapGet("/swagger-ui/custom.css", () => Results.Content(@"
    .swagger-ui .topbar { 
        background-color: #2c3e50; 
    }
    .swagger-ui .info .title {
        color: #2c3e50;
        font-size: 36px;
    }
    .swagger-ui .info .description {
        font-size: 14px;
        line-height: 1.6;
    }
    .swagger-ui .opblock-tag {
        font-size: 18px;
        font-weight: bold;
        border-bottom: 2px solid #2c3e50;
        padding-bottom: 10px;
        margin-bottom: 20px;
    }
", "text/css"));

// Serve custom JS for Swagger UI to add a diagnostic link
app.MapGet("/swagger-ui/custom.js", () => Results.Content(@"
document.addEventListener('DOMContentLoaded', function(){
    try{
        var topbar = document.querySelector('.swagger-ui .topbar .wrapper') || document.querySelector('.swagger-ui .topbar');
        if(topbar){
            var a = document.createElement('a');
            a.href = '/diag/predict';
            a.textContent = 'Diag: Predict';
            a.style.marginLeft = '12px';
            a.style.color = '#fff';
            a.style.background = 'rgba(255,255,255,0.06)';
            a.style.padding = '6px 10px';
            a.style.borderRadius = '4px';
            a.style.textDecoration = 'none';
            a.target = '_blank';
            topbar.appendChild(a);
        }
    }catch(e){console.warn('custom swagger js error', e)}
});
", "application/javascript"));

app.MapControllers();

// Gateway health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    service = "gateway",
    timestamp = DateTime.UtcNow,
    version = "1.0",
    connectedServices = new[]
    {
        new { name = "AdminAPI", path = "/admin/health", swagger = "Direct service Swagger (via Aspire)" },
        new { name = "ClientAPI", path = "/client/health", swagger = "Direct service Swagger (via Aspire)" },
        new { name = "NotificationAPI", path = "/notification/health", swagger = "Direct service Swagger (via Aspire)" }
        ,new { name = "PredictAPI", path = "/api/v1/pricing/health", swagger = "Predict API (internal)" }
    }
})).WithName("GatewayHealth")
    .WithTags("Gateway")
    .WithOpenApi(operation => 
    {
        operation.Summary = "Gateway Health Check";
        operation.Description = "Returns the health status of the gateway and information about connected services";
        return operation;
    });

// Diagnostic page: performs a test POST to PredictAPI and renders results (HTML)
app.MapGet("/diag/predict", async (IHttpClientFactory httpFactory) =>
{
    var client = httpFactory.CreateClient();
    client.Timeout = TimeSpan.FromSeconds(5);

    var sample = new
    {
        checkInDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
        nights = 1,
        adults = 2,
        children = 0,
        hotelType = "City Hotel",
        marketSegment = "Direct",
        customerType = "Transient",
        depositType = "No Deposit",
        meal = "BB",
        isRepeatedGuest = 0,
        specialRequests = 0,
        leadTimeDays = 14
    };

    string predictResultText;
    try
    {
        // Call Predict API directly within the Docker network
        var resp = await client.PostAsJsonAsync("http://predictapi:8000/api/v1/pricing/predict", sample);
        var body = await resp.Content.ReadAsStringAsync();
        predictResultText = $"<pre>Status: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(body)}</pre>";
    }
    catch (Exception ex)
    {
        predictResultText = $"<pre>Error calling PredictAPI: {System.Text.Encodings.Web.HtmlEncoder.Default.Encode(ex.Message)}</pre>";
    }

    var html = $@"<html><head><title>Gateway → Predict diag</title>
        <style>body{{font-family:Arial,Helvetica,sans-serif;margin:24px}}pre{{background:#f6f6f6;padding:12px;border-radius:6px}}</style>
        </head><body>
        <h1>Gateway → Predict Diagnostic</h1>
        <p><strong>Gateway:</strong> /health → <a href='/health'>/health</a></p>
        <h2>Predict API test (via internal network)</h2>
        {predictResultText}
        <p>To test via gateway externally, curl <code>POST http://localhost:8089/api/v1/pricing/predict</code></p>
        </body></html>";

    return Results.Content(html, "text/html");
}).WithName("DiagPredict");

// Service discovery endpoint
app.MapGet("/services", () => Results.Ok(new
{
    gateway = new
    {
        version = "1.0",
        swagger = "/swagger",
        routes = new[]
        {
            new 
            { 
                service = "AdminAPI",
                baseUrl = "/api/v1.0/Admin",
                endpoints = new[]
                {
                    "GET /api/v1.0/Admin/hotels",
                    "GET /api/v1.0/Admin/rooms",
                    "POST /api/v1.0/Admin/room-availability",
                    "PUT /api/v1.0/Admin/room-availability",
                    "DELETE /api/v1.0/Admin/room-availability/{id}",
                    "GET /api/v1.0/Admin/room-availability/{id}",
                    "GET /api/v1.0/Admin/hotels/{hotelId}/room-availabilities"
                },
                authentication = "Required (ADMIN)",
                health = "/admin/health"
            },
            new 
            { 
                service = "ClientAPI - Search",
                baseUrl = "/api/v1.0/HotelSearch",
                endpoints = new[]
                {
                    "POST /api/v1.0/HotelSearch/search",
                    "GET /api/v1.0/HotelSearch/search",
                    "GET /api/v1.0/HotelSearch/hotels/{id}",
                    "GET /api/v1.0/HotelSearch/quick-search"
                },
                authentication = "Public",
                health = "/client/health"
            },
            new 
            { 
                service = "ClientAPI - Booking",
                baseUrl = "/api/v1.0/BookHotel",
                endpoints = new[]
                {
                    "POST /api/v1.0/BookHotel/book",
                    "GET /api/v1.0/BookHotel/{id}",
                    "GET /api/v1.0/BookHotel/reference/{reference}",
                    "GET /api/v1.0/BookHotel/check-availability",
                    "POST /api/v1.0/BookHotel/{id}/cancel",
                    "GET /api/v1.0/BookHotel/my-bookings",
                    "GET /api/v1.0/BookHotel/hotel/{hotelId}"
                },
                authentication = "Optional (guest allowed)",
                health = "/client/health"
            },
            new 
            { 
                service = "NotificationAPI",
                baseUrl = "/api/v1.0/Notifications",
                endpoints = new[]
                {
                    "GET /api/v1.0/Notifications/pending",
                    "GET /api/v1.0/Notifications/hotel/{hotelId}",
                    "GET /api/v1.0/Notifications/{id}",
                    "POST /api/v1.0/Notifications/trigger/low-capacity-check",
                    "POST /api/v1.0/Notifications/trigger/process-reservations"
                },
                authentication = "Required (ADMIN)",
                health = "/notification/health"
            }
            ,
            new 
            { 
                service = "PredictAPI",
                baseUrl = "/api/v1/pricing",
                endpoints = new[]
                {
                    "POST /api/v1/pricing/predict",
                    "GET /health"
                },
                authentication = "Public (proxied via gateway)",
                health = "/api/v1/pricing/health"
            }
        }
    }
})).WithName("ServiceDiscovery")
    .WithTags("Gateway")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Service Discovery";
        operation.Description = "Lists all available services, their endpoints, and authentication requirements";
        return operation;
    });

// Reverse proxy (YARP) - must be last
app.MapReverseProxy();

app.Run();