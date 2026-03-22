using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using NorthwindRAG.Core.Interfaces;
using NorthwindRAG.Core.Options;
using NorthwindRAG.Infrastructure.Data;
using NorthwindRAG.SemanticKernel.Plugins;
using NorthwindRAG.SemanticKernel.Services;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);

// Options
var openAIOptions = builder.Configuration
    .GetSection(OpenAIOptions.SectionName)
    .Get<OpenAIOptions>() ?? new OpenAIOptions();

var connectionString = builder.Configuration.GetConnectionString("Northwind")
    ?? throw new InvalidOperationException("Northwind connection string not found.");

// EF Core
builder.Services.AddDbContext<NorthwindDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<NorthwindRepository>();

// Use default HttpClient — standalone .NET can reach OpenAI fine.
var openAiHttpClient = new HttpClient()
{
    DefaultRequestVersion = HttpVersion.Version11,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
};

// Semantic Kernel
builder.Services.AddKernel()
    .AddOpenAIChatCompletion(openAIOptions.ChatModel, openAIOptions.ApiKey, httpClient: openAiHttpClient)
    .AddOpenAITextEmbeddingGeneration(openAIOptions.EmbeddingModel, openAIOptions.ApiKey, httpClient: openAiHttpClient)
    .Plugins.AddFromObject(new NorthwindQueryPlugin(connectionString));

// Qdrant vector database
var qdrantHost = builder.Configuration.GetValue<string>("Qdrant:Host") ?? "localhost";
var qdrantPort = builder.Configuration.GetValue<int>("Qdrant:Port", 6334);
builder.Services.AddSingleton(_ => new QdrantClient(qdrantHost, qdrantPort));
builder.Services.AddSingleton<IVectorStoreService, VectorStoreService>();
builder.Services.AddSingleton<IIngestionService>(sp =>
{
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    var embeddingService = sp.GetRequiredService<Microsoft.SemanticKernel.Embeddings.ITextEmbeddingGenerationService>();
    var vectorStore = sp.GetRequiredService<IVectorStoreService>();

    // Create a scope to resolve the scoped NorthwindRepository
    var scope = scopeFactory.CreateScope();
    var repo = scope.ServiceProvider.GetRequiredService<NorthwindRepository>();
    return new IngestionService(repo, embeddingService, vectorStore);
});
builder.Services.AddScoped<IRagService>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    var chatCompletion = sp.GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();
    var embeddingService = sp.GetRequiredService<Microsoft.SemanticKernel.Embeddings.ITextEmbeddingGenerationService>();
    var vectorStore = sp.GetRequiredService<IVectorStoreService>();
    return new RagService(kernel, chatCompletion, embeddingService, vectorStore);
});

// CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "NorthwindRAG API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
