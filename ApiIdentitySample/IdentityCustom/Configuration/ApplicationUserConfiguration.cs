using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiIdentitySample.IdentityCustom.Configuration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(t => t.Pseudo)
                .HasMaxLength(250).IsRequired(false);

            builder.Property(t => t.AzureObjectId).IsRequired(false);

            builder.Property(t => t.AzureTenantId).IsRequired(false);



        }
}
}
