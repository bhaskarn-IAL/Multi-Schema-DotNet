using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.Aud;

namespace MultiSchema.DAL.Data
{
    public partial class MultiSchemaContext
    {
        /// <summary>aud: append-only. App gets INSERT + SELECT, never UPDATE/DELETE.</summary>
        private static void ConfigureAud(ModelBuilder modelBuilder, enDBInstance db)
        {
            modelBuilder.Entity<AuditTrail>(e =>
            {
                e.HasIndex(x => new { x.EntityName, x.EntityKey });
                e.HasIndex(x => x.OccurredOn);
            });

            modelBuilder.Entity<WorkflowHistory>(e => e.HasIndex(x => new { x.EntityName, x.EntityKey }));
            modelBuilder.Entity<LicenceEvidence>(e => e.HasIndex(x => x.CapturedOn));

            // SQL Server: aud tables are APPEND_ONLY ledger tables (tamper-evident, engine-enforced, like
            // ArthosCoreDevV1 dbo.PlatformAudit). EF migrations cannot create ledger tables, so they are
            // excluded here and created by Scripts/SQL Server/Schema/03_aud_append_only_ledger.sql.
            // EF still maps and inserts/queries them normally.
            if (db == enDBInstance.SQLSERVER)
            {
                modelBuilder.Entity<AuditTrail>().ToTable(tb => tb.ExcludeFromMigrations());
                modelBuilder.Entity<WorkflowHistory>().ToTable(tb => tb.ExcludeFromMigrations());
                modelBuilder.Entity<LicenceEvidence>().ToTable(tb => tb.ExcludeFromMigrations());
            }
        }
    }
}
