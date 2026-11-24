using MultiSchema.Common.Constant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.Dat
{
    // dat: bulk feed data and landing tables. Different writers (ETL loaders), own storage/filegroup,
    // retention and restore order. Bulk loads should go through provider bulk-copy, not EF change tracking.

    [Table("FeedBatches", Schema = DbSchema.Dat)]
    public class FeedBatch
    {
        [Key]
        public long FeedBatchId { get; set; }

        [Required, MaxLength(100)]
        public string FeedName { get; set; } = null!;

        public DateTime AsOfDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "LANDED";

        public long RowCount { get; set; }

        [MaxLength(500)]
        public string? SourceFile { get; set; }

        public DateTime LoadedOn { get; set; } = DateTime.UtcNow;

        public ICollection<FeedLandingRow> Rows { get; set; } = new List<FeedLandingRow>();
    }

    [Table("FeedLandingRows", Schema = DbSchema.Dat)]
    public class FeedLandingRow
    {
        [Key]
        public long FeedLandingRowId { get; set; }

        public long FeedBatchId { get; set; }
        public FeedBatch? Batch { get; set; }

        public int RowNumber { get; set; }

        /// <summary>Raw row as landed (JSON / delimited text).</summary>
        public string RawPayload { get; set; } = null!;

        [MaxLength(1000)]
        public string? ValidationError { get; set; }
    }
}
