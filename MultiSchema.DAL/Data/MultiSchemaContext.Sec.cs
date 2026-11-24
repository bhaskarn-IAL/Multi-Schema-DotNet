using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Models.Sec;

namespace MultiSchema.DAL.Data
{
    public partial class MultiSchemaContext
    {
        /// <summary>sec: identity, roles, permissions, SoD. IAM/audit team gets SELECT on this schema only.</summary>
        private static void ConfigureSec(ModelBuilder modelBuilder, enDBInstance db)
        {
            modelBuilder.Entity<AppUser>(e =>
            {
                e.HasIndex(x => x.LoginName).IsUnique();
                e.HasIndex(x => x.ExternalSubjectId);
            });

            modelBuilder.Entity<Role>(e => e.HasIndex(x => x.RoleCode).IsUnique());
            modelBuilder.Entity<Permission>(e => e.HasIndex(x => x.PermissionCode).IsUnique());

            modelBuilder.Entity<UserRole>(e =>
            {
                e.HasKey(x => new { x.UserId, x.RoleId });
                e.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RolePermission>(e =>
            {
                e.HasKey(x => new { x.RoleId, x.PermissionId });
                e.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SodRule>(e =>
            {
                e.HasIndex(x => new { x.RoleAId, x.RoleBId }).IsUnique();
                // Two FKs to the same table: Restrict avoids SQL Server's multiple-cascade-path error.
                e.HasOne(x => x.RoleA).WithMany().HasForeignKey(x => x.RoleAId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.RoleB).WithMany().HasForeignKey(x => x.RoleBId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
