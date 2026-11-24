using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.Rpt;

namespace MultiSchema.DAL.Data
{
    public partial class MultiSchemaContext
    {
        /// <summary>rpt: rebuildable projections; BI tools get SELECT on this schema only.</summary>
        private static void ConfigureRpt(ModelBuilder modelBuilder, enDBInstance db)
        {
            modelBuilder.Entity<ModelInventorySummary>(e =>
            {
                // Key is copied from mrm.ModelRecords, not generated here.
                e.Property(x => x.ModelRecordId).ValueGeneratedNever();
                e.HasIndex(x => x.ModelCode);
            });
        }
    }
}
