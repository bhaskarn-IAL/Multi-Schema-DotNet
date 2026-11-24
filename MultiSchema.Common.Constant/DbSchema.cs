namespace MultiSchema.Common.Constant
{
    /// <summary>
    /// The seven logical schemas. Entities declare one of these in their [Table] attribute;
    /// <c>SchemaConventions</c> in the DAL translates it to whatever the active provider needs.
    /// </summary>
    public static class DbSchema
    {
        /// <summary>All governed business records and their configuration. App R/W, maker-checker, audited.</summary>
        public const string Mrm = "mrm";

        /// <summary>Identity, roles, permissions, SoD. Client IAM / audit team gets read access only here.</summary>
        public const string Sec = "sec";

        /// <summary>Ledger, audit trail, workflow history, licence evidence. Append-only: insert + select, never update/delete.</summary>
        public const string Aud = "aud";

        /// <summary>Licence installation, certificate, entitlements. Written by client sysadmin only; app reads.</summary>
        public const string Lic = "lic";

        /// <summary>ModelMint system tables: product metadata and runtime machinery. Seeded by releases, purgeable.</summary>
        public const string App = "app";

        /// <summary>Bulk feed data and landing tables. Own writers, storage, retention and restore order.</summary>
        public const string Dat = "dat";

        /// <summary>Read models. Rebuildable projections; the only schema client BI tools can read.</summary>
        public const string Rpt = "rpt";

        public static readonly IReadOnlyList<string> All = new[] { Mrm, Sec, Aud, Lic, App, Dat, Rpt };
    }
}
