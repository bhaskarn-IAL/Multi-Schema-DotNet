using MultiSchema.Common.Constant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.Mrm
{
    [Table("LookupCategories", Schema = DbSchema.Mrm)]
    public class LookupCategory : GovernedEntity
    {
        [Key]
        public int LookupCategoryId { get; set; }

        [Required, MaxLength(50)]
        public string CategoryCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string CategoryName { get; set; } = null!;

        public ICollection<Lookup> Lookups { get; set; } = new List<Lookup>();
    }

    [Table("Lookups", Schema = DbSchema.Mrm)]
    public class Lookup : GovernedEntity
    {
        [Key]
        public int LookupId { get; set; }

        public int LookupCategoryId { get; set; }
        public LookupCategory? Category { get; set; }

        [Required, MaxLength(50)]
        public string LookupCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string LookupValue { get; set; } = null!;

        public int SortOrder { get; set; }
    }
}
