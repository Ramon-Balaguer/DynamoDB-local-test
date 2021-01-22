using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DynamoDB_local_test_api
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ParameterDescriptions.Any(x => x.Type == typeof(IFormFile)))
            {
                operation.Parameters.Clear();
              /*  operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "UploadFile",
                    In = ParameterLocation.Header,
                    Description = "Upload File",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "file",
                        Format = "binary"
                    }
                });*/
                var uploadFileMediaType = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = "object",
                        Properties =
                        {
                            ["uploadedFile"] = new OpenApiSchema()
                            {
                                Description = "Upload File",
                                Type = "file",
                                Format = "binary"
                            }
                        },
                        Required = new HashSet<string>()
                        {
                            "uploadedFile"
                        }
                    }
                };
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = uploadFileMediaType
                    }
                };
            }
        }
    }
}
