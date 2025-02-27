using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuthApp.Shared.Models
{
    public class TwoFactorResponse
    {
        public string SharedKey { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public IEnumerable<string> RecoveryCodes { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
    }
}
