namespace NovaAI.Models
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Reply { get; set; } = string.Empty;
        public string Intent { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string Error { get; set; } = string.Empty;
    }

    public class GeminiRequest
    {
        public List<GeminiContent> contents { get; set; } = new();
    }

    public class GeminiContent
    {
        public List<GeminiPart> parts { get; set; } = new();
    }

    public class GeminiPart
    {
        public string text { get; set; } = string.Empty;
    }

    public class GeminiResponse
    {
        public List<GeminiCandidate> candidates { get; set; } = new();
    }

    public class GeminiCandidate
    {
        public GeminiContent content { get; set; } = new();
    }
}