using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelBooking.NotificationAPI;

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
                    Title = "Hotel Booking Notification API",
                    Version = description.ApiVersion.ToString(),
                    Description = GetDescription(description),
                    Contact = new OpenApiContact
                    {
                        Name = "Notification Support",
                        Email = "notifications@hotelbookingapi.com"
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
        var text = "Notification API for managing hotel booking notifications, alerts, and automated email notifications.";

        if (description.IsDeprecated)
        {
            text += " This API version has been deprecated.";
        }

        if (description.ApiVersion.MajorVersion == 1)
        {
            text += "\n\nVersion 1.0 Features:\n" +
                    "• Booking confirmation emails\n" +
                    "• Booking cancellation notifications\n" +
                    "• Low capacity alerts for hotel admins\n" +
                    "• Automated background processing\n" +
                    "• Notification queue management\n" +
                    "• Manual trigger endpoints for testing";
        }

        return text;
    }
}
