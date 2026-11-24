using MultiSchema.Common.Constant;
using MultiSchema.Common.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSchema.DAL.Models.Lic
{
    // lic: written only by the client sysadmin. The app's DB login has SELECT only,
    // and SchemaWriteGuardInterceptor rejects any tracked change before it reaches the DB.

    [Table("LicenceInstallations", Schema = DbSchema.Lic)]
    public class LicenceInstallation : IAppReadOnly
    {
        [Key]
        public int LicenceInstallationId { get; set; }

        [Required, MaxLength(100)]
        public string LicenceKey { get; set; } = null!;

        [Required, MaxLength(200)]
        public string LicensedTo { get; set; } = null!;

        public DateTime InstalledOn { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }

        public ICollection<LicenceCertificate> Certificates { get; set; } = new List<LicenceCertificate>();
        public ICollection<LicenceEntitlement> Entitlements { get; set; } = new List<LicenceEntitlement>();
    }

    [Table("LicenceCertificates", Schema = DbSchema.Lic)]
    public class LicenceCertificate : IAppReadOnly
    {
        [Key]
        public int LicenceCertificateId { get; set; }

        public int LicenceInstallationId { get; set; }
        public LicenceInstallation? Installation { get; set; }

        [Required, MaxLength(128)]
        public string Thumbprint { get; set; } = null!;

        /// <summary>PEM / base64 certificate body.</summary>
        public string CertificateBody { get; set; } = null!;

        public DateTime NotAfter { get; set; }
    }

    [Table("LicenceEntitlements", Schema = DbSchema.Lic)]
    public class LicenceEntitlement : IAppReadOnly
    {
        [Key]
        public int LicenceEntitlementId { get; set; }

        public int LicenceInstallationId { get; set; }
        public LicenceInstallation? Installation { get; set; }

        [Required, MaxLength(100)]
        public string FeatureCode { get; set; } = null!;

        public int? SeatLimit { get; set; }
        public bool IsEnabled { get; set; }
    }
}
