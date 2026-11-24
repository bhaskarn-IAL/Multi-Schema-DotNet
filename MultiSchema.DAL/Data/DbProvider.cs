using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Data.Conventions;

namespace MultiSchema.DAL.Data
{
    /// <summary>Provider wiring for the single context. Same switch as ECL3Context.OnConfiguring, plus PostgreSQL.</summary>
    public static class DbProvider
    {
        public const string MigrationsHistoryTable = "__EFMigrationsHistory";

        public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder optionsBuilder, enDBInstance instance, string connectionString)
        {
            // The migrations history table belongs to the product machinery, so it goes into "app".
            var historySchema = SchemaConventions.PhysicalSchema(DbSchema.App, instance, CommonConts.MySqlSchemaMode);
            var historyTable = SchemaConventions.PhysicalTable(DbSchema.App, MigrationsHistoryTable, instance, CommonConts.MySqlSchemaMode);
            var timeout = CommonConts.CommandTimeoutSeconds;

            switch (instance)
            {
                case enDBInstance.MYSQL:
                    optionsBuilder.UseMySQL(connectionString, x =>
                    {
                        x.MigrationsHistoryTable(historyTable, historySchema);
                        x.CommandTimeout(timeout);
                    });
                    break;

                case enDBInstance.ORACLE:
                    optionsBuilder.UseOracle(connectionString, x =>
                    {
                        x.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                        x.MigrationsHistoryTable(historyTable, historySchema);
                        x.CommandTimeout(timeout);
                    });
                    break;

                case enDBInstance.POSTGRESQL:
                    optionsBuilder.UseNpgsql(connectionString, x =>
                    {
                        x.MigrationsHistoryTable(historyTable, historySchema);
                        x.CommandTimeout(timeout);
                    });
                    break;

                default:
                    optionsBuilder.UseSqlServer(connectionString, x =>
                    {
                        x.MigrationsHistoryTable(historyTable, historySchema);
                        x.CommandTimeout(timeout);
                    });
                    break;
            }
            return optionsBuilder;
        }

        public static enDBInstance? FromProviderName(string? providerName) => providerName switch
        {
            "Microsoft.EntityFrameworkCore.SqlServer" => enDBInstance.SQLSERVER,
            "Npgsql.EntityFrameworkCore.PostgreSQL" => enDBInstance.POSTGRESQL,
            "MySql.EntityFrameworkCore" => enDBInstance.MYSQL,
            "Oracle.EntityFrameworkCore" => enDBInstance.ORACLE,
            _ => null
        };

        public static enDBInstance Parse(string? value) => (value ?? "").Trim().ToUpperInvariant() switch
        {
            "SQLSERVER" or "MSSQL" => enDBInstance.SQLSERVER,
            "POSTGRESQL" or "POSTGRES" or "PGSQL" or "NPGSQL" => enDBInstance.POSTGRESQL,
            "MYSQL" => enDBInstance.MYSQL,
            "ORACLE" => enDBInstance.ORACLE,
            _ => throw new ArgumentException($"Unknown database provider '{value}'. Use SqlServer, PostgreSQL, MySql or Oracle.")
        };
    }
}
