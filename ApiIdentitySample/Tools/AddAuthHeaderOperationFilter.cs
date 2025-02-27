using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ApiIdentitySample.Tools
{
    public class AddAuthHeaderOperationFilter : IOperationFilter
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public AddAuthHeaderOperationFilter(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Permet de mettre le cadenas dans swagger uniquement si il y a un AuthorizeAttribute
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var filterDescriptor = context.ApiDescription.ActionDescriptor.FilterDescriptors;


            MethodInfo methodInfo;
            var ss = context.ApiDescription.TryGetMethodInfo(out methodInfo);
            if (methodInfo == null) { return; }
            var controllerFilters = methodInfo.DeclaringType?.CustomAttributes;
            var actionFilters = methodInfo.CustomAttributes;
            if (controllerFilters != null) return;
            bool isAuthorized = controllerFilters.Any(d => d.AttributeType.Name == nameof(AuthorizeAttribute)) || actionFilters.Any(d => d.AttributeType.Name == nameof(AuthorizeAttribute));
            var allowAnonymous = actionFilters.Any(d => d.AttributeType.Name == nameof(AllowAnonymousAttribute));

            if ((isAuthorized && !allowAnonymous) || methodInfo.Name.Contains("MapIdentityApi")) //MapIdentityApi pour les endpoints identity générés
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<OpenApiParameter>();

                operation.Security = new List<OpenApiSecurityRequirement>();
                OpenApiSecurityScheme securityDefinition = new OpenApiSecurityScheme()
                {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "Inserer votre token dans le champs",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                };

                OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Id = "jwt_auth",
                        Type = ReferenceType.SecurityScheme
                    }
                };
                OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
                {
                    {securityScheme, new string[] { }},
                };


                var security = securityRequirements;
                //add security in here
                operation.Security.Add(new OpenApiSecurityRequirement
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
                             new string[] {}
                     }
                 });
            }
        }
    }
}
