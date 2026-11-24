using MultiSchema.Common.EntityBase;
using System.ComponentModel.DataAnnotations;

namespace MultiSchema.DAL.Models.Mrm
{
    /// <summary>Common columns for every governed (mrm) record: audit + maker-checker.</summary>
    public abstract class GovernedEntity : IAudit, IMakerChecker
    {
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }

        public long MakerUserId { get; set; }
        public long? CheckerUserId { get; set; }
        [Required, MaxLength(20)]
        public string ApprovalStatus { get; set; } = "PENDING";
        public DateTime? CheckedOn { get; set; }
    }
}
