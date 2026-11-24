using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Data;

namespace MultiSchema.DAL.Data.Conventions
{
    /// <summary>
    /// Translates the logical schema on each entity ([Table(Schema = "mrm")]) into the physical form
    /// the current provider needs. Entities and per-schema configuration stay provider-agnostic.
    ///
    ///   Provider     logical mrm.ModelRecords  ->  physical
    ///   SQL Server   mrm.ModelRecords              [mrm].[ModelRecords]        (real schema)
    ///   PostgreSQL   mrm.ModelRecords              "mrm"."ModelRecords"        (real schema)
    ///   Oracle       mrm.ModelRecords              "MRM"."ModelRecords"        (schema = user; Oracle folds unquoted names to upper case)
    ///   MySQL (Database mode)                      `mrm`.`ModelRecords`        (schema = database on the same server)
    ///   MySQL (Prefix mode)                        `mrm_ModelRecords`          (single database, no schema)
    /// </summary>
    public static class SchemaConventions
    {
        public static string? PhysicalSchema(string logicalSchema, enDBInstance db, enMySqlSchemaMode mySqlMode) => db switch
        {
            enDBInstance.ORACLE => logicalSchema.ToUpperInvariant(),
            enDBInstance.MYSQL when mySqlMode == enMySqlSchemaMode.Prefix => null,
            _ => logicalSchema
        };

        public static string PhysicalTable(string logicalSchema, string table, enDBInstance db, enMySqlSchemaMode mySqlMode) =>
            db == enDBInstance.MYSQL && mySqlMode == enMySqlSchemaMode.Prefix ? $"{logicalSchema}_{table}" : table;

        public static void Apply(ModelBuilder modelBuilder, enDBInstance db, enMySqlSchemaMode mySqlMode)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var schema = entityType.GetSchema();
                var table = entityType.GetTableName();
                if (schema is null || table is null)
                    continue;

                entityType.SetSchema(PhysicalSchema(schema, db, mySqlMode));
                entityType.SetTableName(PhysicalTable(schema, table, db, mySqlMode));
            }
        }
    }

    /// <summary>
    /// The model depends on provider + MySQL mode, so include both in the cache key.
    /// (Lets one process host contexts for several providers, e.g. tests or the DDL preview.)
    /// </summary>
    internal sealed class SchemaModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) =>
            (context.GetType(), context.Database.ProviderName, CommonConts.MySqlSchemaMode, designTime);
    }
}
