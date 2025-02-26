using ApiIdentitySample.IdentityCustom;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ApiIdentitySample.Services
{
    public class EntraIdAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EntraIdAccountService> _logger;

        public EntraIdAccountService(
            UserManager<ApplicationUser> userManager,
            ILogger<EntraIdAccountService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task ProcessEntraIdUserAsync(TokenValidatedContext context)
        {
            var principal = context.Principal;
            if (principal == null)
            {
                _logger.LogWarning("Principal est null dans TokenValidatedContext");
                return;
            }

            // Extraire les informations des claims
            var objectId = principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                ?? principal.FindFirstValue("oid");

            var tenantId = principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid")
                ?? principal.FindFirstValue("tid");

            var email = principal.FindFirstValue("preferred_username")
                ?? principal.FindFirstValue(ClaimTypes.Email)
                ?? principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(objectId) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Impossible d'extraire objectId ou email des claims");
                return;
            }

            // Vérifier si l'utilisateur existe par son identifiant Entra ID
            var user = await _userManager.FindByLoginAsync("AzureAD", objectId);

            if (user == null)
            {
                // Vérifier si l'utilisateur existe par email
                user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // Créer un nouvel utilisateur
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true // Considéré comme confirmé car provenant de Entra ID
                    };

                    // Obtenir d'autres informations d'utilisateur si disponibles
                    var givenName = principal.FindFirstValue(ClaimTypes.GivenName);
                    var surname = principal.FindFirstValue(ClaimTypes.Surname);
                    var displayName = principal.FindFirstValue("name");

                    if (!string.IsNullOrEmpty(givenName))
                        user.NormalizedUserName = givenName;

                    if (!string.IsNullOrEmpty(surname))
                        user.Pseudo = surname;

                    // Créer l'utilisateur
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        _logger.LogError("Échec de création de l'utilisateur Entra ID: {Errors}",
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        throw new Exception("Échec de la création du compte utilisateur");
                    }
                }

                // Ajouter la connexion externe
                var loginInfo = new UserLoginInfo("AzureAD", objectId, "Azure Active Directory");
                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError("Échec d'ajout de la connexion externe Entra ID: {Errors}",
                        string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                }
            }

            // Mettre à jour les claims de l'utilisateur avec les informations de Entra ID
            var identity = (ClaimsIdentity)principal.Identity;

            // Ajouter des claims personnalisés
            identity.AddClaim(new Claim("AzureADObjectId", objectId));
            if (!string.IsNullOrEmpty(tenantId))
                identity.AddClaim(new Claim("AzureADTenantId", tenantId));

            
            // Mettre à jour les claims de base de l'utilisateur avec Identity
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
            identity.AddClaim(userIdClaim);

            // Déterminer les rôles de l'utilisateur
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    }
}
