using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MultiSchema.Common.EntityBase;

namespace MultiSchema.DAL.Data.Interceptors
{
    /// <summary>
    /// App-side guard for the schema write rules. The database grants are the real control
    /// (they also cover ExecuteUpdate/ExecuteDelete and raw SQL, which bypass SaveChanges);
    /// this just fails fast with a clear message before a round-trip.
    ///   aud (IAppendOnly)  : Added allowed; Modified / Deleted rejected.
    ///   lic (IAppReadOnly) : any change rejected.
    /// </summary>
    public sealed class SchemaWriteGuardInterceptor : SaveChangesInterceptor
    {
        public static readonly SchemaWriteGuardInterceptor Instance = new();

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Guard(eventData.Context);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Guard(eventData.Context);
            return ValueTask.FromResult(result);
        }

        private static void Guard(DbContext? context)
        {
            if (context is null) return;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                switch (entry.Entity)
                {
                    case IAppReadOnly when entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted:
                        throw new SchemaWriteViolationException(
                            $"{entry.Metadata.ClrType.Name} lives in the lic schema and is read-only for the application ({entry.State}).");

                    case IAppendOnly when entry.State is EntityState.Modified or EntityState.Deleted:
                        throw new SchemaWriteViolationException(
                            $"{entry.Metadata.ClrType.Name} lives in the aud schema and is append-only ({entry.State} is not allowed).");
                }
            }
        }
    }

    public sealed class SchemaWriteViolationException : InvalidOperationException
    {
        public SchemaWriteViolationException(string message) : base(message) { }
    }
}
