namespace BlazorAuthApp.Client.Infra.Interfaces
{
    public interface ICustomHttpClient
    {
        Task<T> GetAsync<T>(string uri);
        Task<T> PostAsync<T>(string uri, object data); 
    }
}
