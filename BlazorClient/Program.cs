using BlazorClient;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuration de l'HttpClient par défaut
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configuration pour l'authentification personnalisée avec Entra ID
builder.Services.AddOidcAuthentication(options =>
{
    // Configuration de base
    options.ProviderOptions.Authority = "https://login.microsoftonline.com/9c523e69-1868-4f28-826a-993ddf8f33a8/v2.0/"; // Par exemple: https://login.microsoftonline.com/{TENANT_ID}/v2.0/
    options.ProviderOptions.ClientId = "47670ea0-53be-4277-aa69-5686611f2281";
    options.ProviderOptions.ResponseType = "code";

    // Rediriger vers votre endpoint personnalisé au lieu du endpoint standard
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");

    // Configuration des endpoints personnalisés
    options.ProviderOptions.RedirectUri = "https://localhost:7289/authentication/login-callback";
    options.ProviderOptions.PostLogoutRedirectUri = "https://localhost:7289/";

    // Utilisation de votre endpoint API personnalisé
    options.AuthenticationPaths.LogInPath = "login-microsoft";
    options.AuthenticationPaths.LogOutPath = "logout-microsoft";
    options.AuthenticationPaths.ProfilePath = "user-info";

    // Configuration supplémentaire si nécessaire
    options.UserOptions.RoleClaim = "roles";
});

// Configuration pour les appels API authentifiés
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7238");
})
.AddHttpMessageHandler(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
        .ConfigureHandler(
            authorizedUrls: new[] { "https://localhost:7238" },
            scopes: new[] { "openid profile email" }
        );
    return handler;
});
await builder.Build().RunAsync();
