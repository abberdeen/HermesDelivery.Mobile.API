using Swagger.Net;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace CourierAPI.Infrastructure.Extensions.OAuth
{
    /// <summary>
    /// AuthorizationHeaderParameterOperationFilter para introducir JWT en dialogo Swagger
    /// </summary>
    public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                operation.parameters = new List<Parameter>();

            operation.parameters.Add(new Parameter
            {
                name = "Authorization",
                @in = "header",
                description = "JWT Token",
                required = false,
                type = "string"
            });
        }
    }
}