using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelBooking.AdminAPI;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "Hotel Booking Admin API",
                    Version = description.ApiVersion.ToString(),
                    Description = GetDescription(description),
                    Contact = new OpenApiContact
                    {
                        Name = "Admin Support",
                        Email = "admin-support@hotelbookingapi.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
        }
    }

    private static string GetDescription(ApiVersionDescription description)
    {
        var text = "Admin API for managing hotel rooms, availability, and administrative operations.";

        if (description.IsDeprecated)
        {
            text += " This API version has been deprecated.";
        }

        if (description.ApiVersion.MajorVersion == 1)
        {
            text += "\n\nVersion 1.0 Features:\n" +
                    "• Add, update, and delete room availability\n" +
                    "• View all hotels and rooms\n" +
                    "• Manage availability by date range\n" +
                    "• Admin-only access with JWT authentication";
        }

        return text;
    }
}
