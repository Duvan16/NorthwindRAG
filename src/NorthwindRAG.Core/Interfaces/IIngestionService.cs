namespace NorthwindRAG.Core.Interfaces;

public interface IIngestionService
{
    Task IngestAsync(CancellationToken cancellationToken = default);
    bool IsIngested { get; }
}
