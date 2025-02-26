using ApiIdentitySample.IdentityCustom;
using ApiIdentitySample.IdentityCustom.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiIdentitySample.Data
{
    public class IdentitySampleDbContext: IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> AppUser { get; set; }

        public IdentitySampleDbContext()
        {
            
        }

        public IdentitySampleDbContext (DbContextOptions options):base(options) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration<ApplicationUser>(new ApplicationUserConfiguration());

            base.OnModelCreating(builder);
        }
    }
}
