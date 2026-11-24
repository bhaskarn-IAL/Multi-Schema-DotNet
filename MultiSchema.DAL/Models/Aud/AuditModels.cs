using MultiSchema.Common.Constant;
using MultiSchema.Common.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.Aud
{
    // aud: append-only. No navigation FKs into mrm/sec on purpose — evidence must outlive
    // (and never block deletes/purges of) the rows it describes. Ids are stored as plain values.
    //
    // On SQL Server these tables are excluded from EF migrations and created by
    // Scripts/SQL Server/Schema/03_aud_append_only_ledger.sql as APPEND_ONLY ledger tables.
    // Keep the column list there in sync with these classes.

    [Table("AuditTrail", Schema = DbSchema.Aud)]
    public class AuditTrail : IAppendOnly
    {
        [Key]
        public long AuditTrailId { get; set; }

        [Required, MaxLength(10)]
        public string SourceSchema { get; set; } = null!;

        [Required, MaxLength(128)]
        public string EntityName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string EntityKey { get; set; } = null!;

        /// <summary>INSERT / UPDATE / DELETE / APPROVE / REJECT ...</summary>
        [Required, MaxLength(20)]
        public string Action { get; set; } = null!;

        public string? OldValuesJson { get; set; }
        public string? NewValuesJson { get; set; }

        public long? UserId { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    [Table("WorkflowHistory", Schema = DbSchema.Aud)]
    public class WorkflowHistory : IAppendOnly
    {
        [Key]
        public long WorkflowHistoryId { get; set; }

        [Required, MaxLength(128)]
        public string EntityName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string EntityKey { get; set; } = null!;

        [MaxLength(20)]
        public string? FromStatus { get; set; }

        [Required, MaxLength(20)]
        public string ToStatus { get; set; } = null!;

        public long ActedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }

        public DateTime ActedOn { get; set; } = DateTime.UtcNow;
    }

    [Table("LicenceEvidence", Schema = DbSchema.Aud)]
    public class LicenceEvidence : IAppendOnly
    {
        [Key]
        public long LicenceEvidenceId { get; set; }

        [Required, MaxLength(50)]
        public string EvidenceType { get; set; } = null!;

        [MaxLength(100)]
        public string? LicenceKey { get; set; }

        public string? PayloadJson { get; set; }

        public DateTime CapturedOn { get; set; } = DateTime.UtcNow;
    }
}
