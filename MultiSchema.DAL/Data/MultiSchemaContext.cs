using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Base;
using MultiSchema.DAL.Data.Conventions;
using MultiSchema.DAL.Data.Interceptors;
using MultiSchema.DAL.Models.App;
using MultiSchema.DAL.Models.Aud;
using MultiSchema.DAL.Models.Dat;
using MultiSchema.DAL.Models.Lic;
using MultiSchema.DAL.Models.Mrm;
using MultiSchema.DAL.Models.Rpt;
using MultiSchema.DAL.Models.Sec;

namespace MultiSchema.DAL.Data
{
    /// <summary>
    /// ONE DbContext over ONE database that holds seven schemas (mrm, sec, aud, lic, app, dat, rpt).
    ///
    /// Why one context works:
    ///  - EF Core keys every table by (schema, table). Entities declare their schema via [Table(Schema = ...)].
    ///  - All schemas share one connection, so a single SaveChanges is a single local transaction across
    ///    schemas (e.g. write mrm.ModelRecords + aud.AuditTrail atomically) and cross-schema joins/FKs work.
    ///  - Schema isolation (who may read/write what) is enforced by the DATABASE through per-schema grants
    ///    (see Scripts/*/Schema), not by splitting contexts. SchemaWriteGuardInterceptor is a second line.
    ///
    /// Per-schema mapping lives in the partial files MultiSchemaContext.{Mrm,Sec,Aud,Lic,App,Dat,Rpt}.cs.
    /// Provider differences (Oracle upper-case users, MySQL databases/prefixes) are handled once in
    /// <see cref="SchemaConventions"/>, so model classes never change per provider.
    /// </summary>
    public partial class MultiSchemaContext : DbContext
    {
        #region mrm - governed business records
        public virtual DbSet<ModelRecord> ModelRecords { get; set; } = null!;
        public virtual DbSet<ModelVersion> ModelVersions { get; set; } = null!;
        public virtual DbSet<LookupCategory> LookupCategories { get; set; } = null!;
        public virtual DbSet<Lookup> Lookups { get; set; } = null!;
        #endregion

        #region sec - identity, roles, permissions, SoD
        public virtual DbSet<AppUser> Users { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Permission> Permissions { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;
        public virtual DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public virtual DbSet<SodRule> SodRules { get; set; } = null!;
        #endregion

        #region aud - append-only evidence
        public virtual DbSet<AuditTrail> AuditTrail { get; set; } = null!;
        public virtual DbSet<WorkflowHistory> WorkflowHistory { get; set; } = null!;
        public virtual DbSet<LicenceEvidence> LicenceEvidence { get; set; } = null!;
        #endregion

        #region lic - licence (app read-only)
        public virtual DbSet<LicenceInstallation> LicenceInstallations { get; set; } = null!;
        public virtual DbSet<LicenceCertificate> LicenceCertificates { get; set; } = null!;
        public virtual DbSet<LicenceEntitlement> LicenceEntitlements { get; set; } = null!;
        #endregion

        #region app - product system tables
        public virtual DbSet<ProductMetadata> ProductMetadata { get; set; } = null!;
        public virtual DbSet<RuntimeJob> RuntimeJobs { get; set; } = null!;
        #endregion

        #region dat - bulk feeds / landing
        public virtual DbSet<FeedBatch> FeedBatches { get; set; } = null!;
        public virtual DbSet<FeedLandingRow> FeedLandingRows { get; set; } = null!;
        #endregion

        #region rpt - read models
        public virtual DbSet<ModelInventorySummary> ModelInventorySummary { get; set; } = null!;
        #endregion

        public string ConnnectionString { get; private set; } = string.Empty;

        /// <summary>ECL3-style: <c>using var db = new MultiSchemaContext();</c></summary>
        public MultiSchemaContext()
        {
            ConnnectionString = new DBConnection(CommonConts.MULTISCHEMA_DBCONTEXT).ConnectionString;
            Database.SetCommandTimeout(TimeSpan.FromSeconds(CommonConts.CommandTimeoutSeconds));
        }

        /// <summary>DI / tests / design-time. If the options carry no provider, OnConfiguring falls back to DBConnection.</summary>
        public MultiSchemaContext(DbContextOptions<MultiSchemaContext> options) : base(options)
        {
        }

        /// <summary>The engine this instance is actually running on (from the configured provider, not just the global flag).</summary>
        public enDBInstance DbInstance => DbProvider.FromProviderName(Database.ProviderName) ?? CommonConts.DBInstance;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(SchemaWriteGuardInterceptor.Instance);
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, SchemaModelCacheKeyFactory>();

            if (optionsBuilder.IsConfigured)
                return;

            if (string.IsNullOrEmpty(ConnnectionString))
                ConnnectionString = new DBConnection(CommonConts.MULTISCHEMA_DBCONTEXT).ConnectionString;

            DbProvider.Configure(optionsBuilder, CommonConts.DBInstance, ConnnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var db = DbInstance;

            ConfigureMrm(modelBuilder, db);
            ConfigureSec(modelBuilder, db);
            ConfigureAud(modelBuilder, db);
            ConfigureLic(modelBuilder, db);
            ConfigureApp(modelBuilder, db);
            ConfigureDat(modelBuilder, db);
            ConfigureRpt(modelBuilder, db);

            // Must run last: rewrites logical schema names to what the provider needs.
            SchemaConventions.Apply(modelBuilder, db, CommonConts.MySqlSchemaMode);
        }
    }
}
