using BlazorAuthApp.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace BlazorAuthApp.Client.Pages
{
    public partial class Login: ComponentBase
    {
        private LoginRequest loginModel = new LoginRequest();
        private bool loading = false;
        private bool showErrors = false;
        private bool requiresTwoFactor = false;
        private string error = string.Empty;
        private string returnUrl = string.Empty;

        protected override void OnInitialized()
        {
            // Récupérer l'URL de retour depuis les paramètres de requête
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("returnUrl", out var param))
            {
                returnUrl = param.First();
            }
        }

        private async Task HandleLogin()
        {
            loading = true;
            showErrors = false;
            error = string.Empty;

            try
            {
                var result = await AuthService.Login(loginModel);
                await JSRuntime.InvokeVoidAsync("console.log", result);

                if (result != null)
                {
                    // Redirection après connexion réussie
                    NavigationManager.NavigateTo(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
                }
                else
                {
                    showErrors = true;
                    error = "Email ou mot de passe incorrect.";
                }
            }
            catch (Exception ex)
            {

                await JSRuntime.InvokeVoidAsync("console.log", ex.Message);
                await JSRuntime.InvokeVoidAsync("console.log", ex.StackTrace );
                showErrors = true;
                error = ex.Message;

                // Vérifier si une authentification à deux facteurs est requise
                if (ex.Message.Contains("TwoFactorRequired"))
                {
                    requiresTwoFactor = true;
                    error = "Veuillez fournir le code d'authentification à deux facteurs.";
                }
            }
            finally
            {
                loading = false;
            }
        }

        private async Task HandleMicrosoftLogin()
        {
            loading = true;

            try
            {
                // Redirection vers la page de connexion Microsoft
                await AuthService.LoginWithMicrosoft(string.IsNullOrEmpty(returnUrl) ? NavigationManager.BaseUri : returnUrl);
            }
            catch (Exception ex)
            {
                showErrors = true;
                error = $"Erreur lors de la connexion avec Microsoft: {ex.Message}";
            }
            finally
            {
                loading = false;
            }
        }
    }
}
