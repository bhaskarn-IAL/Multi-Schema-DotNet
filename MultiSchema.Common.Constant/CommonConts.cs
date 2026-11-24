namespace MultiSchema.Common.Constant
{
    /// <summary>
    /// Database engine the single <c>MultiSchemaContext</c> talks to.
    /// Same idea as ECL3's <c>enDBInstance</c>, minus the app split (one context covers every schema).
    /// </summary>
    public enum enDBInstance
    {
        SQLSERVER = 1,
        MYSQL = 2,
        POSTGRESQL = 3,
        ORACLE = 4
    }

    /// <summary>
    /// How logical schemas are physically realised on MySQL, which has no schema separate from a database.
    /// </summary>
    public enum enMySqlSchemaMode
    {
        /// <summary>Each logical schema is its own MySQL database on the same server (mrm.ModelRecords). Real per-schema GRANTs.</summary>
        Database = 1,
        /// <summary>Everything in one MySQL database, table names prefixed (mrm_ModelRecords). Use when only one database is allowed.</summary>
        Prefix = 2
    }

    public static class CommonConts
    {
        /// <summary>Set once at startup from configuration (Database:Provider).</summary>
        public static enDBInstance DBInstance = enDBInstance.SQLSERVER;

        /// <summary>Only used when <see cref="DBInstance"/> is MYSQL.</summary>
        public static enMySqlSchemaMode MySqlSchemaMode = enMySqlSchemaMode.Database;

        /// <summary>Logical name of the one context. Connection strings are looked up as {prefix}{name}.</summary>
        public const string MULTISCHEMA_DBCONTEXT = "MULTISCHEMADBCONTEXT";

        public static int CommandTimeoutSeconds = 120;
    }
}
