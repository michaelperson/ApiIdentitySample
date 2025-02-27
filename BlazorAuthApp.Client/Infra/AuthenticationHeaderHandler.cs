using BlazorAuthApp.Client.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorAuthApp.Client.Infra
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly IAuthService _authService;
        private readonly IJSRuntime jSRuntime; 
        private bool _isRefreshing = false;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public AuthenticationHeaderHandler(ISessionStorageService sessionStorage, IAuthService authService, IJSRuntime jSRuntime )
        {
            _sessionStorage = sessionStorage;
            _authService = authService;
            this.jSRuntime = jSRuntime; 
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddTokenToRequest(request);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Attendre si un refresh est déjà en cours
                await _semaphore.WaitAsync();
                try
                {
                    if (!_isRefreshing)
                    {
                        _isRefreshing = true;

                        // Récupérer le refresh token

                        try
                        {
                            // Appeler le service d'authentification pour obtenir un nouveau token
                            var newTokens = await _authService.RefreshToken();

                            // Réessayer la requête originale avec le nouveau token
                            await AddTokenToRequest(request);
                            response = await base.SendAsync(request, cancellationToken);
                        }
                        catch
                        {
                            // En cas d'échec du refresh, nettoyer les tokens
                            await _sessionStorage.RemoveItemAsync("authToken");
                            await _sessionStorage.RemoveItemAsync("refreshToken");
                            throw;
                        }

                    }
                }
                finally
                {
                    _isRefreshing = false;
                    _semaphore.Release();
                }
            }

            return response;
        }

        private async Task AddTokenToRequest(HttpRequestMessage request)
        {
            var token = await _sessionStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                await jSRuntime.InvokeVoidAsync("console.log", token);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            } 
            else
            {
                await jSRuntime.InvokeVoidAsync("fetch", request, new { credentials = "include" });
            }
            
        }
    }
}

