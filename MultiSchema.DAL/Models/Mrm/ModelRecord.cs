using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.Sec;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.Mrm
{
    /// <summary>Model inventory record. Cross-schema FK to sec.Users (owner).</summary>
    [Table("ModelRecords", Schema = DbSchema.Mrm)]
    public class ModelRecord : GovernedEntity
    {
        [Key]
        public long ModelRecordId { get; set; }

        [Required, MaxLength(50)]
        public string ModelCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string ModelName { get; set; } = null!;

        [MaxLength(20)]
        public string? RiskTier { get; set; }

        public long OwnerUserId { get; set; }
        public AppUser? Owner { get; set; }

        public ICollection<ModelVersion> Versions { get; set; } = new List<ModelVersion>();
    }
}
