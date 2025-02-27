using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuthApp.Shared.Models
{
    public class InfoResponse
    {
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
