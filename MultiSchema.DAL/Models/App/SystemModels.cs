using MultiSchema.Common.Constant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.App
{
    // app: ModelMint system tables. Seeded by IAL releases, invisible to business users, high churn, purgeable.

    [Table("ProductMetadata", Schema = DbSchema.App)]
    public class ProductMetadata
    {
        [Key]
        public int ProductMetadataId { get; set; }

        [Required, MaxLength(100)]
        public string MetaKey { get; set; } = null!;

        public string? MetaValue { get; set; }

        [Required, MaxLength(20)]
        public string ReleaseVersion { get; set; } = null!;

        public DateTime SeededOn { get; set; } = DateTime.UtcNow;
    }

    [Table("RuntimeJobs", Schema = DbSchema.App)]
    public class RuntimeJob
    {
        [Key]
        public long RuntimeJobId { get; set; }

        [Required, MaxLength(100)]
        public string JobType { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "QUEUED";

        public string? PayloadJson { get; set; }

        public int Attempts { get; set; }

        public DateTime QueuedOn { get; set; } = DateTime.UtcNow;
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }

        [MaxLength(2000)]
        public string? LastError { get; set; }
    }
}
