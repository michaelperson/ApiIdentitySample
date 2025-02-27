using BlazorAuthApp.Client.Infra;
using BlazorAuthApp.Client.Services.Interfaces;
using BlazorAuthApp.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace BlazorAuthApp.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ISessionStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime JSRuntime;

        public AuthService(HttpClient httpClient, ISessionStorageService localStorage, AuthenticationStateProvider authStateProvider, IJSRuntime jSRuntime)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            JSRuntime = jSRuntime;
        }

        public async Task<bool> Register(RegisterRequest registerRequest)
        { 
            var response = await _httpClient.PostAsJsonAsync("register", registerRequest);
            return response.IsSuccessStatusCode;
        }

        public async Task<AccessTokenResponse> Login(LoginRequest loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("login", loginRequest);
            await JSRuntime.InvokeVoidAsync("console.log", response);
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
                await _localStorage.SetItemAsync("authToken", tokenResponse.AccessToken);
                await _localStorage.SetItemAsync("refreshToken", tokenResponse.RefreshToken);

                await JSRuntime.InvokeVoidAsync("console.log", tokenResponse.AccessToken);
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(tokenResponse.AccessToken);

                await JSRuntime.InvokeVoidAsync("console.log", tokenResponse);
                return tokenResponse;
            }

            return null;
        }

        public async Task<bool> LoginWithMicrosoft(string returnUrl)
        {
            var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
            var loginUrl = $"login-microsoft?returnUrl={encodedReturnUrl}";

            // Rediriger vers la page de connexion Microsoft
            // Cette méthode ouvre une redirection dans le navigateur
            var absoluteUri = new Uri(_httpClient.BaseAddress, loginUrl).AbsoluteUri;

            await JSRuntime.InvokeVoidAsync("window.location.replace", absoluteUri);

            return true;
        }

        public async Task<InfoResponse> GetUserInfo()
        {
            var response = await _httpClient.GetAsync("manage/info");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InfoResponse>();
            }

            return null;
        }

        public async Task<bool> RefreshToken()
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var refreshRequest = new RefreshRequest { RefreshToken = refreshToken };
            var response = await _httpClient.PostAsJsonAsync("refresh", refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
                await _localStorage.SetItemAsync("authToken", tokenResponse.AccessToken);
                await _localStorage.SetItemAsync("refreshToken", tokenResponse.RefreshToken);

                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(tokenResponse.AccessToken);

                return true;
            }

            return false;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<bool> LogoutMicrosoft(string returnUrl)
        {
            var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
            var logoutUrl = $"logout-microsoft?returnUrl={encodedReturnUrl}";

            var absoluteUri = new Uri(_httpClient.BaseAddress, logoutUrl).AbsoluteUri;
            await JSRuntime.InvokeVoidAsync("window.location.replace", absoluteUri);

            await Logout(); // Nettoyer les tokens locaux aussi

            return true;
        }
    }
}
