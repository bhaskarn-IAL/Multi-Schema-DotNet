using MultiSchema.Common.Constant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.Mrm
{
    [Table("ModelVersions", Schema = DbSchema.Mrm)]
    public class ModelVersion : GovernedEntity
    {
        [Key]
        public long ModelVersionId { get; set; }

        public long ModelRecordId { get; set; }
        public ModelRecord? ModelRecord { get; set; }

        [Required, MaxLength(20)]
        public string VersionLabel { get; set; } = null!;

        public DateTime? EffectiveFrom { get; set; }

        /// <summary>Free-form configuration payload (JSON). Unbounded: nvarchar(max) / text / longtext / NCLOB.</summary>
        public string? ConfigurationJson { get; set; }
    }
}
