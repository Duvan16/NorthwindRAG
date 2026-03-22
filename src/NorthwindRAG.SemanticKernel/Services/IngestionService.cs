using Microsoft.SemanticKernel.Embeddings;
using NorthwindRAG.Core.Interfaces;
using NorthwindRAG.Core.Models;
using NorthwindRAG.Infrastructure.Data;

namespace NorthwindRAG.SemanticKernel.Services;

public class IngestionService : IIngestionService
{
    private readonly NorthwindRepository _repository;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorStoreService _vectorStore;
    private bool _ingested;

    public IngestionService(
        NorthwindRepository repository,
        ITextEmbeddingGenerationService embeddingService,
        IVectorStoreService vectorStore)
    {
        _repository = repository;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public bool IsIngested => _ingested;

    public async Task IngestAsync(CancellationToken cancellationToken = default)
    {
        var chunks = new List<DocumentChunk>();

        // Products
        var products = await _repository.GetAllProductsAsync();
        foreach (var p in products)
        {
            chunks.Add(new DocumentChunk
            {
                Source = "Products",
                EntityId = p.ProductId.ToString(),
                Text = $"Product: {p.ProductName}. Category: {p.Category?.CategoryName ?? "Unknown"}. " +
                       $"Supplier: {p.Supplier?.CompanyName ?? "Unknown"}. " +
                       $"Price: ${p.UnitPrice:F2}. Units in stock: {p.UnitsInStock}. " +
                       $"Discontinued: {p.Discontinued}. Quantity per unit: {p.QuantityPerUnit}."
            });
        }

        // Customers
        var customers = await _repository.GetAllCustomersAsync();
        foreach (var c in customers)
        {
            chunks.Add(new DocumentChunk
            {
                Source = "Customers",
                EntityId = c.CustomerId,
                Text = $"Customer: {c.CompanyName} (ID: {c.CustomerId}). " +
                       $"Contact: {c.ContactName}, {c.ContactTitle}. " +
                       $"Location: {c.City}, {c.Country}."
            });
        }

        // Employees
        var employees = await _repository.GetAllEmployeesAsync();
        foreach (var e in employees)
        {
            chunks.Add(new DocumentChunk
            {
                Source = "Employees",
                EntityId = e.EmployeeId.ToString(),
                Text = $"Employee: {e.FirstName} {e.LastName}. Title: {e.Title}. " +
                       $"Location: {e.City}, {e.Country}. Notes: {e.Notes}"
            });
        }

        // Suppliers
        var suppliers = await _repository.GetAllSuppliersAsync();
        foreach (var s in suppliers)
        {
            chunks.Add(new DocumentChunk
            {
                Source = "Suppliers",
                EntityId = s.SupplierId.ToString(),
                Text = $"Supplier: {s.CompanyName}. Contact: {s.ContactName}. " +
                       $"Location: {s.City}, {s.Country}."
            });
        }

        // Orders (summary only to keep chunks manageable)
        var orders = await _repository.GetAllOrdersAsync();
        foreach (var o in orders)
        {
            chunks.Add(new DocumentChunk
            {
                Source = "Orders",
                EntityId = o.OrderId.ToString(),
                Text = $"Order #{o.OrderId}. Customer: {o.Customer?.CompanyName ?? o.CustomerId}. " +
                       $"Employee: {o.Employee?.FirstName} {o.Employee?.LastName}. " +
                       $"Order date: {o.OrderDate:yyyy-MM-dd}. " +
                       $"Ship to: {o.ShipCity}, {o.ShipCountry}. " +
                       $"Shipped: {(o.ShippedDate.HasValue ? o.ShippedDate.Value.ToString("yyyy-MM-dd") : "not yet")}."
            });
        }

        // Batch embed all chunks
        const int batchSize = 100;
        for (int i = 0; i < chunks.Count; i += batchSize)
        {
            var batch = chunks.Skip(i).Take(batchSize).ToList();
            var texts = batch.Select(c => c.Text).ToList();
            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(texts, cancellationToken: cancellationToken);

            for (int j = 0; j < batch.Count; j++)
            {
                batch[j].Embedding = embeddings[j];
            }

            await _vectorStore.UpsertAsync(batch, cancellationToken);
        }

        _ingested = true;
    }
}
