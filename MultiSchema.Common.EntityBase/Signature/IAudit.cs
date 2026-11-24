namespace MultiSchema.Common.EntityBase
{
    /// <summary>Standard who/when columns. Values are UTC (required by PostgreSQL timestamptz).</summary>
    public interface IAudit
    {
        bool IsActive { get; set; }
        DateTime CreatedOn { get; set; }
        long? CreatedBy { get; set; }
        DateTime? UpdatedOn { get; set; }
        long? UpdatedBy { get; set; }
    }

    /// <summary>Maker-checker columns for governed (mrm) records.</summary>
    public interface IMakerChecker
    {
        long MakerUserId { get; set; }
        long? CheckerUserId { get; set; }
        string ApprovalStatus { get; set; }
        DateTime? CheckedOn { get; set; }
    }
}
