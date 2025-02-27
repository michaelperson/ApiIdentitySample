using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorAuthApp.Shared.Models
{
    public class UserInfoResponse
    {
        public string name { get; set; }
        [JsonPropertyName("claims")]
        public List<ClaimInfo> claims { get; set; }
    }

    public class ClaimInfo
    {
        public string type { get; set; }
        = string.Empty;
        public string value { get; set; }
        = string.Empty;
    }
}
