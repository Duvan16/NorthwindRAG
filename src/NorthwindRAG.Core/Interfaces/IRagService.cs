using NorthwindRAG.Core.DTOs;

namespace NorthwindRAG.Core.Interfaces;

public interface IRagService
{
    Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
