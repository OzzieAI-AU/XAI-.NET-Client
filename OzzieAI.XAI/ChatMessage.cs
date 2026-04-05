using System.Text.Json.Serialization;

namespace OzzieAI.XAI
{
    /// <summary>
    /// Single message in a chat conversation.
    /// <code>
    /// var file = await client.UploadFileAsync("report.pdf");
    /// var req = new ChatRequest { ..., FileIds = new() { file.Id } };
    /// </code>
    /// </summary>
    /// <summary>
    /// Single message in a chat conversation.
    /// Fully supports all currently available input modes for xAI Grok API:
    /// • Plain text
    /// • Images / Vision (text + image_url)
    /// • Files / Documents (via prompt reference after UploadFileAsync)
    /// 
    /// Audio and video INPUT are NOT supported in chat completions
    /// (they use separate Realtime WebSocket / Imagine APIs).
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Role of the message sender ("system", "user", "assistant").
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        /// <summary>
        /// The content of the message.
        /// 
        /// • string  → normal text message
        /// • List<ContentPart> → multimodal (images, or future parts)
        /// 
        /// System.Text.Json automatically picks the correct JSON format.
        /// </summary>
        [JsonPropertyName("content")]
        public object? Content { get; set; }

        // ─────────────────────────────────────────────────────────────────────
        // Factory Methods – All supported modes in one clean API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Simple text message (any role).</summary>
        public static ChatMessage Text(string role, string text)
        {
            return new ChatMessage { Role = role, Content = text };
        }

        /// <summary>Quick user text message (most common).</summary>
        public static ChatMessage User(string text) => Text("user", text);

        /// <summary>Quick system message.</summary>
        public static ChatMessage System(string text) => Text("system", text);

        /// <summary>Assistant message (for few-shot or history).</summary>
        public static ChatMessage Assistant(string text) => Text("assistant", text);

        /// <summary>
        /// Tool calls requested by the assistant (populated when finish_reason == "tool_calls").
        /// This is the standard field in OpenAI-compatible responses used by xAI Grok.
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<ToolCall>? ToolCalls { get; set; }

        /// <summary>
        /// Convenience property indicating whether this message contains tool calls.
        /// </summary>
        [JsonIgnore]
        public bool HasToolCalls => ToolCalls?.Count > 0;

        /// <summary>
        /// User message with text + image (full vision support).
        /// </summary>
        public static ChatMessage UserWithImage(string text, string imageUrlOrBase64, string detail = "high")
        {
            var parts = new List<ContentPart>
            {
                new() { Type = "text",      Text = text },
                new() { Type = "image_url", ImageUrl = new ImageDetail { Url = imageUrlOrBase64, Detail = detail } }
            };

            return new ChatMessage { Role = "user", Content = parts };
        }

        /// <summary>
        /// User message with reference to an uploaded file (PDF, code, TXT, CSV, etc.).
        /// Uses the real pattern: upload first → get file ID → mention it in prompt.
        /// </summary>
        public static ChatMessage UserWithFile(string text, string fileId, string? fileName = null)
        {
            var enhancedText = string.IsNullOrEmpty(fileName)
                ? $"{text}\n\n(Attached file ID: {fileId})"
                : $"{text}\n\n(Attached file: {fileName} – ID: {fileId})";

            return new ChatMessage { Role = "user", Content = enhancedText };
        }

        /// <summary>
        /// Advanced: Create a message with any custom content parts (future-proof).
        /// </summary>
        public static ChatMessage UserWithParts(params ContentPart[] parts)
        {
            return new ChatMessage
            {
                Role = "user",
                Content = parts.Length == 1 ? parts[0] : parts.ToList()   // single or array
            };
        }
    }
}