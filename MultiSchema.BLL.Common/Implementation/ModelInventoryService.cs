using Microsoft.EntityFrameworkCore;
using MultiSchema.BLL.Common.Signature;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Data;
using MultiSchema.DAL.Models.Aud;
using MultiSchema.DAL.Models.Mrm;
using MultiSchema.DAL.Models.Rpt;
using System.Text.Json;

namespace MultiSchema.BLL.Common.Implementation
{
    public class ModelInventoryService : IModelInventoryService
    {
        private readonly MultiSchemaContext _db;

        public ModelInventoryService(MultiSchemaContext db) => _db = db;

        public async Task<IReadOnlyList<ModelSummaryDto>> GetModelsAsync(CancellationToken ct = default) =>
            await _db.ModelRecords
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderBy(m => m.ModelCode)
                .Select(m => new ModelSummaryDto(
                    m.ModelRecordId, m.ModelCode, m.ModelName, m.RiskTier, m.ApprovalStatus,
                    m.Owner != null ? m.Owner.DisplayName : null,   // sec.Users
                    m.Versions.Count))                             // mrm.ModelVersions
                .ToListAsync(ct);

        public async Task<long> CreateModelAsync(CreateModelRequest request, CancellationToken ct = default)
        {
            var model = new ModelRecord
            {
                ModelCode = request.ModelCode,
                ModelName = request.ModelName,
                RiskTier = request.RiskTier,
                OwnerUserId = request.OwnerUserId,
                MakerUserId = request.MakerUserId,
                CreatedBy = request.MakerUserId,
                ApprovalStatus = "PENDING"
            };

            // Explicit transaction because we need the generated id for the audit rows (two SaveChanges).
            // Same connection, same database -> a plain local transaction spans mrm and aud. No DTC.
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.ModelRecords.Add(model);
            await _db.SaveChangesAsync(ct);

            _db.AuditTrail.Add(new AuditTrail
            {
                SourceSchema = DbSchema.Mrm,
                EntityName = nameof(ModelRecord),
                EntityKey = model.ModelRecordId.ToString(),
                Action = "INSERT",
                NewValuesJson = JsonSerializer.Serialize(request),
                UserId = request.MakerUserId
            });
            _db.WorkflowHistory.Add(new WorkflowHistory
            {
                EntityName = nameof(ModelRecord),
                EntityKey = model.ModelRecordId.ToString(),
                FromStatus = null,
                ToStatus = model.ApprovalStatus,
                ActedByUserId = request.MakerUserId,
                Remarks = "Created (maker)"
            });
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return model.ModelRecordId;
        }

        public async Task<int> RebuildInventoryProjectionAsync(CancellationToken ct = default)
        {
            var rows = await _db.ModelRecords
                .AsNoTracking()
                .Select(m => new ModelInventorySummary
                {
                    ModelRecordId = m.ModelRecordId,
                    ModelCode = m.ModelCode,
                    ModelName = m.ModelName,
                    RiskTier = m.RiskTier,
                    OwnerName = m.Owner != null ? m.Owner.DisplayName : null,
                    ApprovalStatus = m.ApprovalStatus,
                    VersionCount = m.Versions.Count,
                    LatestVersion = m.Versions.OrderByDescending(v => v.ModelVersionId).Select(v => v.VersionLabel).FirstOrDefault(),
                    RefreshedOn = DateTime.UtcNow
                })
                .ToListAsync(ct);

            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            await _db.ModelInventorySummary.ExecuteDeleteAsync(ct);   // rpt is rebuildable
            _db.ModelInventorySummary.AddRange(rows);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return rows.Count;
        }
    }
}
