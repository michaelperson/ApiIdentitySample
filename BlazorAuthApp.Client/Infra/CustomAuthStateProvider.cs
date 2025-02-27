using BlazorAuthApp.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorAuthApp.Client.Infra
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private IEnumerable<Claim> _claims;
        public CustomAuthStateProvider(ILocalStorageService localStorage, IHttpClientFactory httpClient)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            ParseClaimsFromJwt(token); 
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(_claims, "jwt")));
        }

        public void NotifyUserAuthentication(string token)
        {
            ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(_claims, "jwt"));
               
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private async Task  ParseClaimsFromJwt(string jwt)
        {
            //Remarque : Idenity utilise un  OAuth2 avec token opaque donc impossible du côté client de le décoder
            HttpClient client = _httpClientFactory.CreateClient("IdentitySampleApi");
            HttpResponseMessage message =await client.GetAsync("/user-info");

            _claims = new List<Claim>();
        }

       
    }
}
