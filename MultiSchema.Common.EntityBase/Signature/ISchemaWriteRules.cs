namespace MultiSchema.Common.EntityBase
{
    /// <summary>
    /// aud schema. The app may INSERT and SELECT, never UPDATE or DELETE.
    /// Enforced by database grants and, as a second line, by the DAL's SchemaWriteGuardInterceptor.
    /// </summary>
    public interface IAppendOnly { }

    /// <summary>
    /// lic schema. Written only by the client sysadmin (outside the app); the app only reads.
    /// </summary>
    public interface IAppReadOnly { }

    /// <summary>
    /// rpt schema. Rebuildable projection; safe to truncate and rebuild.
    /// </summary>
    public interface IProjection { }
}
