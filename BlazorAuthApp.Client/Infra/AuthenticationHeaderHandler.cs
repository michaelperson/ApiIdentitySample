using BlazorAuthApp.Client.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net;

namespace BlazorAuthApp.Client.Infra
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IAuthService _authService;
        private bool _isRefreshing = false;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public AuthenticationHeaderHandler(ILocalStorageService localStorage, IAuthService authService)
        {
            _localStorage = localStorage;
            _authService = authService;
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
                            await _localStorage.RemoveItemAsync("authToken");
                            await _localStorage.RemoveItemAsync("refreshToken");
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
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}

