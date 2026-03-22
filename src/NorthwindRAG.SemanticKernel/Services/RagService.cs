using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using NorthwindRAG.Core.DTOs;
using NorthwindRAG.Core.Interfaces;
using NorthwindRAG.SemanticKernel.Plugins;

namespace NorthwindRAG.SemanticKernel.Services;

public class RagService : IRagService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorStoreService _vectorStore;
    private readonly string _systemPrompt;

    public RagService(
        Kernel kernel,
        IChatCompletionService chatCompletion,
        ITextEmbeddingGenerationService embeddingService,
        IVectorStoreService vectorStore)
    {
        _kernel = kernel;
        _chatCompletion = chatCompletion;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;

        var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", "SystemPrompt.txt");
        _systemPrompt = File.Exists(promptPath)
            ? File.ReadAllText(promptPath)
            : "You are a Northwind database assistant.";
    }

    public async Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Embed the question
        var questionEmbeddings = await _embeddingService.GenerateEmbeddingsAsync(
            [request.Message], cancellationToken: cancellationToken);
        var queryEmbedding = questionEmbeddings[0];

        // 2. Vector search — top 5 relevant chunks
        var relevantChunks = await _vectorStore.SearchAsync(queryEmbedding, topK: 5, cancellationToken);

        // 3. Build augmented prompt
        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("RELEVANT CONTEXT FROM NORTHWIND DATABASE:");
        contextBuilder.AppendLine();
        foreach (var chunk in relevantChunks)
        {
            contextBuilder.AppendLine($"[{chunk.Source}] {chunk.Text}");
        }

        var augmentedPrompt = $"{contextBuilder}\n\nUser question: {request.Message}";

        // 4. Build chat history with system prompt
        var history = new ChatHistory(_systemPrompt);
        history.AddUserMessage(augmentedPrompt);

        // 5. Call GPT with optional function calling
        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await _chatCompletion.GetChatMessageContentAsync(
            history, settings, _kernel, cancellationToken);

        return new ChatResponse
        {
            Answer = result.Content ?? "I could not generate a response.",
            Sources = relevantChunks.Select(c => c.Source).Distinct().ToList(),
            ConversationId = request.ConversationId ?? Guid.NewGuid().ToString()
        };
    }
}
