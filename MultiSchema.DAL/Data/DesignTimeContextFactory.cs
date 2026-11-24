using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MultiSchema.Common.Constant;

namespace MultiSchema.DAL.Data
{
    /// <summary>
    /// Used by `dotnet ef`. Provider and connection come from environment variables so the same
    /// context can produce migrations for any engine:
    ///   $env:MULTISCHEMA_PROVIDER = "SqlServer"   # SqlServer | PostgreSQL | MySql | Oracle
    ///   $env:MULTISCHEMA_CONNECTION = "..."       # migration (schema owner) login, not the runtime app login
    ///   $env:MULTISCHEMA_MYSQL_MODE = "Database"  # Database | Prefix (MySQL only)
    /// </summary>
    public sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<MultiSchemaContext>
    {
        public MultiSchemaContext CreateDbContext(string[] args)
        {
            CommonConts.DBInstance = DbProvider.Parse(Environment.GetEnvironmentVariable("MULTISCHEMA_PROVIDER") ?? "SqlServer");
            if (Enum.TryParse<enMySqlSchemaMode>(Environment.GetEnvironmentVariable("MULTISCHEMA_MYSQL_MODE"), true, out var mode))
                CommonConts.MySqlSchemaMode = mode;

            var cs = Environment.GetEnvironmentVariable("MULTISCHEMA_CONNECTION")
                     ?? "Server=localhost;Database=MultiSchemaDev;Trusted_Connection=True;TrustServerCertificate=True";

            var options = DbProvider.Configure(new DbContextOptionsBuilder<MultiSchemaContext>(), CommonConts.DBInstance, cs);
            return new MultiSchemaContext((DbContextOptions<MultiSchemaContext>)options.Options);
        }
    }
}
