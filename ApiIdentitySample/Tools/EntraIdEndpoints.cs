using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiIdentitySample.Tools
{
    public static class EntraIdEndpoints
    {
        public static void MapEntraIdEndpoints(this WebApplication app)
        {
            // Endpoint pour initialiser la connexion Entra ID
            app.MapGet("/login-microsoft", async (
                [FromQuery] string returnUrl,
                HttpContext context) =>
            {
                returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

                var properties = new AuthenticationProperties
                {
                    RedirectUri = returnUrl,
                    IsPersistent = true
                };

                return Results.Challenge(properties, new[] { OpenIdConnectDefaults.AuthenticationScheme });
            })
            .AllowAnonymous()
            .WithName("LoginMicrosoft");

            // Endpoint pour la déconnexion
            app.MapGet("/logout-microsoft", async (
                [FromQuery] string returnUrl,
                HttpContext context) =>
            {
                returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

                // Déconnexion de l'application
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);

                // Déconnexion de Microsoft Entra ID
                return Results.SignOut(
                    authenticationSchemes: new[] { OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = returnUrl
                    });
            })
            .RequireAuthorization()
            .WithName("LogoutMicrosoft");

            // Endpoint pour obtenir les informations de l'utilisateur connecté
            app.MapGet("/user-info", (ClaimsPrincipal user) =>
            {
                if (!user.Identity.IsAuthenticated)
                {
                    return Results.Unauthorized();
                }

                var userInfo = new
                {
                    Name = user.Identity.Name,
                    Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
                };

                return Results.Ok(userInfo);
            })
            .RequireAuthorization()
            .WithName("UserInfo");
        }
    }
}
