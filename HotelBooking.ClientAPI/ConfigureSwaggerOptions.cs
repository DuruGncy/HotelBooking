using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelBooking.ClientAPI;

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
                    Title = "Hotel Booking Client API",
                    Version = description.ApiVersion.ToString(),
                    Description = GetDescription(description),
                    Contact = new OpenApiContact
                    {
                        Name = "Client Support",
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
        var text = "Client API for searching and booking hotels for public users and authenticated guests.";

        if (description.IsDeprecated)
        {
            text += " This API version has been deprecated.";
        }

        if (description.ApiVersion.MajorVersion == 1)
        {
            text += "\n\nVersion 1.0 Features:\n" +
                    "• Search hotels by destination and dates\n" +
                    "• Real-time availability checking\n" +
                    "• Create and manage bookings\n" +
                    "• Cancel bookings with automatic capacity restore\n" +
                    "• Guest bookings (no authentication required)\n" +
                    "• View booking details and history";
        }

        return text;
    }
}
