using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.Dat;

namespace MultiSchema.DAL.Data
{
    public partial class MultiSchemaContext
    {
        /// <summary>dat: bulk feeds and landing tables.</summary>
        private static void ConfigureDat(ModelBuilder modelBuilder, enDBInstance db)
        {
            modelBuilder.Entity<FeedBatch>(e => e.HasIndex(x => new { x.FeedName, x.AsOfDate }));

            modelBuilder.Entity<FeedLandingRow>(e =>
            {
                e.HasIndex(x => new { x.FeedBatchId, x.RowNumber }).IsUnique();
                // Purging a batch purges its landing rows.
                e.HasOne(x => x.Batch).WithMany(x => x.Rows)
                 .HasForeignKey(x => x.FeedBatchId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
