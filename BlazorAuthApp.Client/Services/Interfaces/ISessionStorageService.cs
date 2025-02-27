namespace BlazorAuthApp.Client.Services.Interfaces
{
    public interface ISessionStorageService
    {
        Task<T> GetItemAsync<T>(string key);
        Task SetItemAsync<T>(string key, T value);
        Task RemoveItemAsync(string key);
    }
}
