using BlazorAuthApp.Shared.Models;
using System.Net.Http.Json;

namespace BlazorAuthApp.Client.Pages
{
    public partial class ForgotPassword
    {
        private ForgotPasswordRequest forgotPasswordModel = new ForgotPasswordRequest();
        private bool loading = false;
        private bool showErrors = false;
        private bool requestSent = false;
        private string error = string.Empty;

        private async Task HandleForgotPassword()
        {
            loading = true;
            showErrors = false;
            error = string.Empty;

            try
            {
                var response = await Http.PostAsJsonAsync("forgotPassword", forgotPasswordModel);

                if (response.IsSuccessStatusCode)
                {
                    requestSent = true;
                }
                else
                {
                    showErrors = true;
                    error = "Une erreur est survenue. Veuillez réessayer.";
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
    }
}
