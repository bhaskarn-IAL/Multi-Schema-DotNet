using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.App;

namespace MultiSchema.DAL.Data
{
    public partial class MultiSchemaContext
    {
        /// <summary>app: product metadata and runtime machinery (also hosts __EFMigrationsHistory).</summary>
        private static void ConfigureApp(ModelBuilder modelBuilder, enDBInstance db)
        {
            modelBuilder.Entity<ProductMetadata>(e => e.HasIndex(x => x.MetaKey).IsUnique());
            modelBuilder.Entity<RuntimeJob>(e => e.HasIndex(x => new { x.Status, x.QueuedOn }));
        }
    }
}
