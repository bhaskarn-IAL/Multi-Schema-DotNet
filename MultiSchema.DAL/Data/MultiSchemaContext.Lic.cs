using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.Lic;

namespace MultiSchema.DAL.Data
{
    public partial class MultiSchemaContext
    {
        /// <summary>lic: client sysadmin writes; app reads only.</summary>
        private static void ConfigureLic(ModelBuilder modelBuilder, enDBInstance db)
        {
            modelBuilder.Entity<LicenceInstallation>(e => e.HasIndex(x => x.LicenceKey).IsUnique());

            modelBuilder.Entity<LicenceCertificate>(e =>
            {
                e.HasIndex(x => x.Thumbprint).IsUnique();
                e.HasOne(x => x.Installation).WithMany(x => x.Certificates)
                 .HasForeignKey(x => x.LicenceInstallationId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LicenceEntitlement>(e =>
            {
                e.HasIndex(x => new { x.LicenceInstallationId, x.FeatureCode }).IsUnique();
                e.HasOne(x => x.Installation).WithMany(x => x.Entitlements)
                 .HasForeignKey(x => x.LicenceInstallationId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
