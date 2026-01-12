using Microsoft.Extensions.Caching.Memory;

namespace HotelBookingGateway.Middleware;

public class GatewayMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly string _gatewaySecret;

    public GatewayMiddleware(RequestDelegate next, IMemoryCache cache, IConfiguration config)
    {
        _next = next;
        _cache = cache;
        _gatewaySecret = config["GatewaySecret"] ?? "";
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var request = context.Request;
        var requestTime = DateTime.UtcNow;
        var method = request.Method;
        var path = request.Path + request.QueryString;
        var sourceIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        var requestSize = request.ContentLength ?? 0;

        // --- Logging request ---
        Console.WriteLine("----- Request -----");
        Console.WriteLine($"Timestamp: {requestTime:O}");
        Console.WriteLine($"Method: {method}");
        Console.WriteLine($"Path: {path}");
        Console.WriteLine($"Source IP: {sourceIp}");
        Console.WriteLine($"Request size: {requestSize} bytes");
        Console.WriteLine($"Headers: {System.Text.Json.JsonSerializer.Serialize(headers)}");
        Console.WriteLine("-------------------");

       
        // --- Gateway secret header for downstream API ---
        context.Request.Headers["X-Gateway-Secret"] = _gatewaySecret;

        // Call next middleware
        await _next(context);

        stopwatch.Stop();
        var response = context.Response;
        var latencyMs = stopwatch.ElapsedMilliseconds;
        var responseSizw = context.Response.ContentLength;

        // --- Logging response ---
        Console.WriteLine("----- Response -----");
        Console.WriteLine($"Status code: {response.StatusCode}");
        Console.WriteLine($"Latency: {latencyMs} ms");
        Console.WriteLine("-------------------");
    }
}

// --- Extension method for easy registration ---
public static class GatewayMiddlewareExtensions
{
    public static IApplicationBuilder UseGatewayMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GatewayMiddleware>();
    }
}
