using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Data;

namespace MultiSchema.API.Controllers
{
    /// <summary>Diagnostics: shows how the single context maps onto schemas for the configured provider.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SchemaController : ControllerBase
    {
        private readonly MultiSchemaContext _db;
        private readonly IWebHostEnvironment _env;

        public SchemaController(MultiSchemaContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        /// <summary>Entity -> physical schema.table for the active provider. No DB round-trip.</summary>
        [HttpGet("map")]
        public IActionResult Map() => Ok(new
        {
            Provider = _db.DbInstance.ToString(),
            MySqlSchemaMode = _db.DbInstance == enDBInstance.MYSQL ? CommonConts.MySqlSchemaMode.ToString() : null,
            Tables = _db.Model.GetEntityTypes()
                .Select(e => new { Entity = e.ClrType.Name, Schema = e.GetSchema(), Table = e.GetTableName() })
                .OrderBy(x => x.Schema).ThenBy(x => x.Table)
        });

        /// <summary>CREATE script EF would run for the active provider (Development only). No DB round-trip.</summary>
        [HttpGet("ddl")]
        [Produces("text/plain")]
        public IActionResult Ddl() =>
            _env.IsDevelopment() ? Content(_db.Database.GenerateCreateScript(), "text/plain") : NotFound();

        /// <summary>Connectivity + row counts per schema (proves one connection reaches all schemas).</summary>
        [HttpGet("health")]
        public async Task<IActionResult> Health(CancellationToken ct)
        {
            if (!await _db.Database.CanConnectAsync(ct))
                return StatusCode(503, new { Connected = false });

            return Ok(new
            {
                Connected = true,
                mrm = await _db.ModelRecords.CountAsync(ct),
                sec = await _db.Users.CountAsync(ct),
                aud = await _db.AuditTrail.CountAsync(ct),
                lic = await _db.LicenceInstallations.CountAsync(ct),
                app = await _db.ProductMetadata.CountAsync(ct),
                dat = await _db.FeedBatches.CountAsync(ct),
                rpt = await _db.ModelInventorySummary.CountAsync(ct)
            });
        }
    }
}
