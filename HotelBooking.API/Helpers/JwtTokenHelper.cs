using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HotelBookingAPI.Helpers;

/// <summary>
/// Helper class to generate JWT tokens for testing purposes
/// </summary>
public static class JwtTokenHelper
{
    /// <summary>
    /// Generates a JWT token for testing
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="username">Username</param>
    /// <param name="role">User role (e.g., "ADMIN", "USER")</param>
    /// <param name="secretKey">JWT secret key</param>
    /// <param name="issuer">JWT issuer</param>
    /// <param name="audience">JWT audience</param>
    /// <param name="expiryMinutes">Token expiry in minutes (default: 60)</param>
    /// <returns>JWT token string</returns>
    public static string GenerateToken(
        string userId,
        string username,
        string role,
        string secretKey,
        string issuer,
        string audience,
        int expiryMinutes = 60)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generate an ADMIN role token with default settings
    /// </summary>
    public static string GenerateAdminToken(
        string userId = "admin-1",
        string username = "admin@hotels.com")
    {
        // These should match your appsettings.json
        return GenerateToken(
            userId,
            username,
            "ADMIN",
            "YourSecureSecretKeyThatIsAtLeast32CharactersLong!",
            "HotelBookingAPI",
            "HotelBookingAPIUsers"
        );
    }

    /// <summary>
    /// Generate a USER role token with default settings
    /// </summary>
    public static string GenerateUserToken(
        string userId = "user-1",
        string username = "user@hotels.com")
    {
        // These should match your appsettings.json
        return GenerateToken(
            userId,
            username,
            "USER",
            "YourSecureSecretKeyThatIsAtLeast32CharactersLong!",
            "HotelBookingAPI",
            "HotelBookingAPIUsers"
        );
    }
}
