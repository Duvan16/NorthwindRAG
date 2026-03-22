using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using NorthwindRAG.Core.Interfaces;
using NorthwindRAG.Core.Models;
using Qdrant.Client;

namespace NorthwindRAG.SemanticKernel.Services;

public class VectorStoreService : IVectorStoreService
{
    private readonly QdrantVectorStore _vectorStore;
    private VectorStoreCollection<Guid, DocumentChunk>? _collection;
    private bool _initialized;

    public VectorStoreService(QdrantClient qdrantClient)
    {
        _vectorStore = new QdrantVectorStore(qdrantClient, ownsClient: false);
    }

    public bool IsInitialized => _initialized;

    private async Task<VectorStoreCollection<Guid, DocumentChunk>> GetCollectionAsync()
    {
        if (_collection == null)
        {
            _collection = _vectorStore.GetCollection<Guid, DocumentChunk>("northwind-chunks");
            await _collection.EnsureCollectionExistsAsync();
        }
        return _collection;
    }

    public async Task UpsertAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();
        await collection.UpsertAsync(chunks, cancellationToken);
        _initialized = true;
    }

    public async Task<List<DocumentChunk>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int topK = 5, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();
        var chunks = new List<DocumentChunk>();
        await foreach (var result in collection.SearchAsync(queryEmbedding, topK, cancellationToken: cancellationToken).WithCancellation(cancellationToken))
        {
            chunks.Add(result.Record);
        }
        return chunks;
    }
}
