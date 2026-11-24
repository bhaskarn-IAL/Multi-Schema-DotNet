using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.Mrm;

namespace MultiSchema.DAL.Data
{
    public partial class MultiSchemaContext
    {
        /// <summary>mrm: governed records. App R/W, maker-checker, audited.</summary>
        private static void ConfigureMrm(ModelBuilder modelBuilder, enDBInstance db)
        {
            modelBuilder.Entity<ModelRecord>(e =>
            {
                e.HasIndex(x => x.ModelCode).IsUnique();

                // Cross-schema FK: mrm.ModelRecords.OwnerUserId -> sec.Users.UserId
                e.HasOne(x => x.Owner)
                 .WithMany()
                 .HasForeignKey(x => x.OwnerUserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ModelVersion>(e =>
            {
                e.HasIndex(x => new { x.ModelRecordId, x.VersionLabel }).IsUnique();
                e.HasOne(x => x.ModelRecord)
                 .WithMany(x => x.Versions)
                 .HasForeignKey(x => x.ModelRecordId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LookupCategory>(e => e.HasIndex(x => x.CategoryCode).IsUnique());

            modelBuilder.Entity<Lookup>(e =>
            {
                e.HasIndex(x => new { x.LookupCategoryId, x.LookupCode }).IsUnique();
                e.HasOne(x => x.Category)
                 .WithMany(x => x.Lookups)
                 .HasForeignKey(x => x.LookupCategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // SQL Server only: system-versioned (temporal) tables give point-in-time history of
            // governed records for free (as in ArthosCoreDevV1 dbo.Lookups). Other engines rely on aud.AuditTrail.
            if (db == enDBInstance.SQLSERVER)
            {
                modelBuilder.Entity<ModelRecord>().ToTable(tb => tb.IsTemporal(t => t.UseHistoryTable("ModelRecordsHistory", DbSchema.Mrm)));
                modelBuilder.Entity<ModelVersion>().ToTable(tb => tb.IsTemporal(t => t.UseHistoryTable("ModelVersionsHistory", DbSchema.Mrm)));
                modelBuilder.Entity<LookupCategory>().ToTable(tb => tb.IsTemporal(t => t.UseHistoryTable("LookupCategoriesHistory", DbSchema.Mrm)));
                modelBuilder.Entity<Lookup>().ToTable(tb => tb.IsTemporal(t => t.UseHistoryTable("LookupsHistory", DbSchema.Mrm)));
            }
        }
    }
}
