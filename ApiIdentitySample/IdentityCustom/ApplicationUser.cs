using Microsoft.AspNetCore.Identity;

namespace ApiIdentitySample.IdentityCustom
{
    public class ApplicationUser : IdentityUser
    {
        public int Id { get; set; }
        public string Pseudo { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string MotDePasse { get; set; } = null!;

        public DateTime? DateInscription { get; set; }


        public string AzureObjectId { get; set; }
        public string AzureTenantId { get; set; }


    }
     

}
