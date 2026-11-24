using MultiSchema.Common.Constant;
using MultiSchema.Common.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.Sec
{
    [Table("Users", Schema = DbSchema.Sec)]
    public class AppUser : IAudit
    {
        [Key]
        public long UserId { get; set; }

        [Required, MaxLength(100)]
        public string LoginName { get; set; } = null!;

        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = null!;

        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>Subject id from the external IdP (e.g. Keycloak). No passwords are stored here.</summary>
        [MaxLength(100)]
        public string? ExternalSubjectId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    [Table("Roles", Schema = DbSchema.Sec)]
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string RoleCode { get; set; } = null!;

        [Required, MaxLength(200)]
        public string RoleName { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    [Table("Permissions", Schema = DbSchema.Sec)]
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required, MaxLength(100)]
        public string PermissionCode { get; set; } = null!;

        [MaxLength(400)]
        public string? Description { get; set; }
    }

    /// <summary>Composite key (UserId, RoleId) configured in MultiSchemaContext.Sec.cs.</summary>
    [Table("UserRoles", Schema = DbSchema.Sec)]
    public class UserRole
    {
        public long UserId { get; set; }
        public AppUser? User { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public DateTime AssignedOn { get; set; } = DateTime.UtcNow;
        public long? AssignedBy { get; set; }
    }

    /// <summary>Composite key (RoleId, PermissionId) configured in MultiSchemaContext.Sec.cs.</summary>
    [Table("RolePermissions", Schema = DbSchema.Sec)]
    public class RolePermission
    {
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }

    /// <summary>Segregation-of-duties rule: a user must not hold both roles.</summary>
    [Table("SodRules", Schema = DbSchema.Sec)]
    public class SodRule
    {
        [Key]
        public int SodRuleId { get; set; }

        public int RoleAId { get; set; }
        public Role? RoleA { get; set; }

        public int RoleBId { get; set; }
        public Role? RoleB { get; set; }

        [MaxLength(400)]
        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
