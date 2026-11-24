namespace MultiSchema.BLL.Common.Signature
{
    public record ModelSummaryDto(long ModelRecordId, string ModelCode, string ModelName, string? RiskTier,
                                  string ApprovalStatus, string? OwnerName, int VersionCount);

    public record CreateModelRequest(string ModelCode, string ModelName, string? RiskTier, long OwnerUserId, long MakerUserId);

    public interface IModelInventoryService
    {
        /// <summary>Cross-schema read: mrm.ModelRecords JOIN sec.Users in one query.</summary>
        Task<IReadOnlyList<ModelSummaryDto>> GetModelsAsync(CancellationToken ct = default);

        /// <summary>Cross-schema write: mrm.ModelRecords + aud.AuditTrail + aud.WorkflowHistory in one transaction.</summary>
        Task<long> CreateModelAsync(CreateModelRequest request, CancellationToken ct = default);

        /// <summary>Rebuild rpt.ModelInventorySummary from mrm + sec.</summary>
        Task<int> RebuildInventoryProjectionAsync(CancellationToken ct = default);
    }
}
