using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuthApp.Shared.Models
{
    public class AccessTokenResponse
    {
        public string TokenType { get; set; } = "Bearer";
        public string AccessToken { get; set; }
        public long ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
    }
}
