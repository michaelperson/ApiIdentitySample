using BlazorAuthApp.Client.Services;
using BlazorAuthApp.Client.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using BlazorAuthApp.Shared.Models;
using System.Net.Http.Json;

namespace BlazorAuthApp.Client.Pages.Secured
{
    [Authorize]
    public partial class Profil
    {
        [Inject]
        IAuthService _AuthService { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        IHttpClientFactory _httpClientFactory { get; set; }

        private UserInfoResponse userInfo;
        protected override async Task OnInitializedAsync()
        {
            userInfo = new UserInfoResponse();
            var client = _httpClientFactory.CreateClient("IdentitySampleApi");
             userInfo =   await client.GetFromJsonAsync<UserInfoResponse>("/user-info");
            
        }

        private async Task Logout()
        {
            await _AuthService.Logout();
            NavigationManager.NavigateTo("/login");
        }

        
    }
}
