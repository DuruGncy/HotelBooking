using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelBookingAPI;

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
                    Title = "Hotel Booking API",
                    Version = description.ApiVersion.ToString(),
                    Description = GetDescription(description),
                    Contact = new OpenApiContact
                    {
                        Name = "Support",
                        Email = "support@hotelbookingapi.com"
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
        var text = "A comprehensive hotel booking system with automated capacity management.";

        if (description.IsDeprecated)
        {
            text += " This API version has been deprecated.";
        }

        if (description.ApiVersion.MajorVersion == 2)
        {
            text += "\n\nVersion 2.0 Features:\n" +
                    "• Price range filtering\n" +
                    "• Star rating filtering\n" +
                    "• Amenity-based search\n" +
                    "• Enhanced result metadata";
        }

        return text;
    }
}
