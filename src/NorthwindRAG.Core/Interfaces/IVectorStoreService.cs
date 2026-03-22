using NorthwindRAG.Core.Models;

namespace NorthwindRAG.Core.Interfaces;

public interface IVectorStoreService
{
    Task UpsertAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    Task<List<DocumentChunk>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int topK = 5, CancellationToken cancellationToken = default);
    bool IsInitialized { get; }
}
