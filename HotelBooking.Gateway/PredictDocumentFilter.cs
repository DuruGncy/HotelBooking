using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelBookingGateway.Swagger
{
    public class PredictDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var predictPath = new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Post] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { new OpenApiTag { Name = "PredictAPI" } },
                        Summary = "Predict price",
                        Description = "Proxy to PredictAPI: POST booking payload and return predicted price",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "Prediction result" }
                        },
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = "object" }
                                }
                            },
                            Required = true
                        }
                    }
                }
            };

            if (!swaggerDoc.Paths.ContainsKey("/api/v1/pricing/predict"))
            {
                swaggerDoc.Paths.Add("/api/v1/pricing/predict", predictPath);
            }
        }
    }
}
