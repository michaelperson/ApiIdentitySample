using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiIdentitySample.IdentityCustom.Configuration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable(nameof(ApplicationUser));

            builder.HasKey(x => x.Id).HasName("PKApplicationUser");

            builder.Property(t => t.Pseudo)
                .HasMaxLength(250).IsRequired(false);
            builder.Property(t => t.MotDePasse)
                .HasMaxLength(250).IsRequired(false);

            builder.Property(t => t.Email).IsRequired(false).HasMaxLength(300);
             

        }
    }
}
