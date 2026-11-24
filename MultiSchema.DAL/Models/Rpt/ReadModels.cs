using MultiSchema.Common.Constant;
using MultiSchema.Common.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MultiSchema.DAL.Models.Rpt
{
    // rpt: rebuildable projections. The only schema client BI tools can read.
    // No FKs back into mrm/sec: projections are truncated and rebuilt independently.

    [Table("ModelInventorySummary", Schema = DbSchema.Rpt)]
    public class ModelInventorySummary : IProjection
    {
        [Key]
        public long ModelRecordId { get; set; }

        [Required, MaxLength(50)]
        public string ModelCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string ModelName { get; set; } = null!;

        [MaxLength(20)]
        public string? RiskTier { get; set; }

        [MaxLength(200)]
        public string? OwnerName { get; set; }

        [MaxLength(20)]
        public string? LatestVersion { get; set; }

        [Required, MaxLength(20)]
        public string ApprovalStatus { get; set; } = null!;

        public int VersionCount { get; set; }

        public DateTime RefreshedOn { get; set; } = DateTime.UtcNow;
    }
}
