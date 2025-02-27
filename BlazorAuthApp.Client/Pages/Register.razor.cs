using BlazorAuthApp.Shared.Models;

namespace BlazorAuthApp.Client.Pages
{
    public partial class Register
    {
        private RegisterRequest registerModel = new RegisterRequest();
        private string confirmPassword = string.Empty;
        private bool loading = false;
        private bool showErrors = false;
        private bool registrationSuccessful = false;
        private string error = string.Empty;
        private string passwordMatchError = string.Empty;

        private async Task HandleRegistration()
        {
            loading = true;
            showErrors = false;
            error = string.Empty;
            passwordMatchError = string.Empty;

            // Vérifier que les mots de passe correspondent
            if (registerModel.Password != confirmPassword)
            {
                passwordMatchError = "Les mots de passe ne correspondent pas.";
                loading = false;
                return;
            }

            try
            {
                var result = await AuthService.Register(registerModel);

                if (result)
                {
                    registrationSuccessful = true;
                }
                else
                {
                    showErrors = true;
                    error = "Erreur lors de l'inscription. Veuillez réessayer.";
                }
            }
            catch (Exception ex)
            {
                showErrors = true;
                error = ex.Message;
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
                // Redirection vers la page de connexion Microsoft (qui peut aussi créer un compte)
                await AuthService.LoginWithMicrosoft(NavigationManager.BaseUri);
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
