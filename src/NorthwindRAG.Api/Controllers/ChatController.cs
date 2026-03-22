using Microsoft.AspNetCore.Mvc;
using NorthwindRAG.Core.DTOs;
using NorthwindRAG.Core.Interfaces;

namespace NorthwindRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly IIngestionService _ingestionService;

    public ChatController(IRagService ragService, IIngestionService ingestionService)
    {
        _ragService = ragService;
        _ingestionService = ingestionService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (!_ingestionService.IsIngested)
            return BadRequest(new { error = "Data not yet ingested. Call POST /api/ingestion/run first." });

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message cannot be empty." });

        var response = await _ragService.AskAsync(request, cancellationToken);
        return Ok(response);
    }
}
