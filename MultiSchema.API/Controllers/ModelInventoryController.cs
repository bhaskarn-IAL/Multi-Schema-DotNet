using Microsoft.AspNetCore.Mvc;
using MultiSchema.BLL.Common.Signature;
using MultiSchema.DAL.Data.Interceptors;

namespace MultiSchema.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelInventoryController : ControllerBase
    {
        private readonly IModelInventoryService _service;

        public ModelInventoryController(IModelInventoryService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ModelSummaryDto>>> Get(CancellationToken ct) =>
            Ok(await _service.GetModelsAsync(ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateModelRequest request, CancellationToken ct)
        {
            try
            {
                var id = await _service.CreateModelAsync(request, ct);
                return CreatedAtAction(nameof(Get), new { id }, new { ModelRecordId = id });
            }
            catch (SchemaWriteViolationException ex)
            {
                return Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
            }
        }

        [HttpPost("rebuild-projection")]
        public async Task<IActionResult> RebuildProjection(CancellationToken ct) =>
            Ok(new { Rows = await _service.RebuildInventoryProjectionAsync(ct) });
    }
}
