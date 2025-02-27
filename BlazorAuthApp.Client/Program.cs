using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAuthApp.Client;
using BlazorAuthApp.Client.Infra;
using BlazorAuthApp.Client.Services.Interfaces;
using BlazorAuthApp.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorAuthApp.Client.Infra.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
 
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationHeaderHandler>();
// Configuration de l'URL de base de l'API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient<ICustomHttpClient, CustomHttpClient>("IdentitySampleApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<AuthenticationHeaderHandler>();

builder.Services.AddCascadingAuthenticationState();

// Authorization services
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
