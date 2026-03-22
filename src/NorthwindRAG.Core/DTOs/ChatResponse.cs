namespace NorthwindRAG.Core.DTOs;

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new();
    public string? ConversationId { get; set; }
}
