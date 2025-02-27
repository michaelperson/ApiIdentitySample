using BlazorAuthApp.Client.Infra.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorAuthApp.Client.Infra
{
    public class CustomHttpClient : ICustomHttpClient
    {
        private readonly HttpClient _httpClient;

        public CustomHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }

        public async Task<T> PostAsync<T>(string uri, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, data);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }
    }
}
