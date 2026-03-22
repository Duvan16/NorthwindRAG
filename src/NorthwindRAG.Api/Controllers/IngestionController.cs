using Microsoft.AspNetCore.Mvc;
using NorthwindRAG.Core.Interfaces;

namespace NorthwindRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly IIngestionService _ingestionService;
    private readonly ILogger<IngestionController> _logger;

    public IngestionController(IIngestionService ingestionService, ILogger<IngestionController> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        if (_ingestionService.IsIngested)
            return Ok(new { message = "Ingestion already completed. Vector store is ready." });

        _logger.LogInformation("Starting Northwind data ingestion...");
        await _ingestionService.IngestAsync(cancellationToken);
        _logger.LogInformation("Ingestion completed.");

        return Ok(new { message = "Ingestion completed successfully." });
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new
        {
            isIngested = _ingestionService.IsIngested,
            status = _ingestionService.IsIngested ? "ready" : "not_ingested"
        });
    }
}
