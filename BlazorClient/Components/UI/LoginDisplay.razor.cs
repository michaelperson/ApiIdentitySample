namespace BlazorClient.Components.UI
{
    public partial class LoginDisplay
    {
        private void BeginSignIn()
        {
            Navigation.NavigateTo($"login-microsoft?returnUrl={Uri.EscapeDataString(Navigation.Uri)}");
        }

        private async Task BeginSignOut()
        {
            await SignOutManager.SetSignOutState();
            Navigation.NavigateTo($"logout-microsoft?returnUrl={Uri.EscapeDataString("/")}");


        }


    }
}


