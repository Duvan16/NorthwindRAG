using Microsoft.Extensions.VectorData;

namespace NorthwindRAG.Core.Models;

public class DocumentChunk
{
    [VectorStoreKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [VectorStoreData(IsIndexed = true)]
    public string Text { get; set; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string Source { get; set; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string EntityId { get; set; } = string.Empty;

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
