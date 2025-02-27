using BlazorAuthApp.Shared.Models;

namespace BlazorAuthApp.Client.Services.Interfaces
{
    public interface IAuthService
    {
        Task<InfoResponse> GetUserInfo();
        Task<AccessTokenResponse> Login(LoginRequest loginRequest);
        Task<bool> LoginWithMicrosoft(string returnUrl);
        Task Logout();
        Task<bool> LogoutMicrosoft(string returnUrl);
        Task<bool> RefreshToken();
        Task<bool> Register(RegisterRequest registerRequest);
    }
}