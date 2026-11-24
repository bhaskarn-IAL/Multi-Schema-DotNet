using Microsoft.Extensions.Configuration;
using MultiSchema.Common.Constant;

namespace MultiSchema.DAL.Base
{
    /// <summary>
    /// Resolves the connection string for a logical context name, per provider.
    /// Mirrors ECL3's DBConnection naming ("MYSQL-ECL3DBCONTEXT", "ORACLE-ECL3DBCONTEXT"),
    /// but reads from IConfiguration (ConnectionStrings section) instead of the encrypted .Settings XML.
    /// Swap <see cref="ReadConnectionString"/> for ReadSysPath if you want the ECL3 file-based store.
    ///
    /// One connection string per provider is enough: all seven schemas live in the same database.
    /// </summary>
    public class DBConnection
    {
        private static readonly object lockObject = new();
        private static readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Set once at startup (Program.cs / design-time factory).</summary>
        public static IConfiguration? Configuration { get; set; }

        public string ConnectionString { get; private set; }
        public string DBContextName { get; private set; }

        public DBConnection(string DBContext)
        {
            DBContextName = ProviderPrefix(CommonConts.DBInstance) + DBContext;
            lock (lockObject)
            {
                if (!_cache.TryGetValue(DBContextName, out var cs))
                {
                    cs = ReadConnectionString(DBContextName);
                    _cache[DBContextName] = cs;
                }
                ConnectionString = cs;
            }
        }

        public DBConnection(string ConnectionString, string DBContextName)
        {
            this.ConnectionString = ConnectionString;
            this.DBContextName = DBContextName;
        }

        public static string ProviderPrefix(enDBInstance instance) => instance switch
        {
            enDBInstance.SQLSERVER => "",
            enDBInstance.MYSQL => "MYSQL-",
            enDBInstance.POSTGRESQL => "PGSQL-",
            enDBInstance.ORACLE => "ORACLE-",
            _ => ""
        };

        private static string ReadConnectionString(string name)
        {
            if (Configuration is null)
                throw new InvalidOperationException("DBConnection.Configuration is not set. Assign it at startup before creating a context.");

            return Configuration.GetConnectionString(name)
                ?? throw new InvalidOperationException($"Connection string '{name}' not found in ConnectionStrings.");
        }
    }
}
