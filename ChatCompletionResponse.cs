namespace xAINetClient
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the full response returned by the Grok chat completions endpoint 
    /// (<c>POST /chat/completions</c>) after processing a chat request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the primary response object when calling <see cref="GrokClient.ChatAsync"/>.
    /// It contains one or more generated message choices, metadata about the request, 
    /// and detailed token usage statistics.
    /// </para>
    /// <para>
    /// <strong>Key characteristics (March 2026):</strong>
    /// <list type="bullet">
    /// <item>Usually contains exactly one choice (<c>n = 1</c> by default)</item>
    /// <item>When <c>n > 1</c>, multiple completions are returned (useful for best-of sampling)</item>
    /// <item>Streaming mode (<c>stream: true</c>) is **not** represented here — this model is for non-streaming responses only</item>
    /// <item>All timestamps are Unix seconds (UTC)</item>
    /// <item>Token counts are accurate and include multimodal input (text + images)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Most common access pattern:
    /// <list type="number">
    /// <item>Take the first (or only) choice: <c>response.Choices[0].Message</c></item>
    /// <item>Read the assistant's reply: <c>response.Choices[0].Message.Content</c></item>
    /// <item>Check <see cref="FinishReason"/> to understand why generation stopped</item>
    /// <item>Inspect <see cref="Usage"/> for cost monitoring or context management</item>
    /// </list>
    /// </para>
    /// <para>
    /// Error cases are thrown as <see cref="GrokApiException"/> before this object is returned.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// var response = await client.ChatAsync(new ChatRequest
    /// {
    ///     Model = "grok-4.20-reasoning",
    ///     Messages = [ new() { Role = "user", Content = "Explain quantum entanglement in simple terms." } ]
    /// });
    ///
    /// if (response.Choices.Count > 0)
    /// {
    ///     var reply = response.Choices[0].Message.Content;
    ///     Console.WriteLine($"Grok says: {reply}");
    ///     Console.WriteLine($"Tokens used: {response.Usage.TotalTokens} (prompt: {response.Usage.PromptTokens})");
    /// }
    ///
    /// // Handle multiple choices (best-of-n)
    /// foreach (var choice in response.Choices)
    /// {
    ///     Console.WriteLine($"Choice {choice.Index}: {choice.FinishReason}");
    /// }
    /// </code>
    /// </example>
    public class ChatCompletionResponse
    {
        /// <summary>
        /// Unique identifier for this specific completion response.
        /// </summary>
        /// <remarks>
        /// Format usually resembles <c>"chatcmpl-abc123..."</c> or similar.  
        /// Useful for logging, debugging, or correlating with usage reports.
        /// </remarks>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// The type of object returned by the API.
        /// </summary>
        /// <remarks>
        /// Always <c>"chat.completion"</c> for non-streaming responses.
        /// </remarks>
        /// <value>Fixed value: <c>"chat.completion"</c></value>
        [JsonPropertyName("object")]
        public string Object { get; set; } = null!;

        /// <summary>
        /// Unix timestamp (seconds, UTC) when this completion was created on the server.
        /// </summary>
        /// <remarks>
        /// Represents when the final token was generated / response finalized.
        /// </remarks>
        [JsonPropertyName("created")]
        public long Created { get; set; }

        /// <summary>
        /// The model identifier that generated this response.
        /// </summary>
        /// <remarks>
        /// Echoes the requested model (or the default if not specified).  
        /// Examples: <c>"grok-4.20-reasoning"</c>, <c>"grok-4-1-fast-reasoning"</c>, etc.
        /// </remarks>
        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;

        /// <summary>
        /// List of generated completion choices.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Length matches the <c>n</c> parameter (default = 1).  
        /// Each <see cref="Choice"/> contains one assistant message and metadata.
        /// </para>
        /// <para>
        /// In almost all practical cases you will use <c>Choices[0]</c>.
        /// </para>
        /// </remarks>
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new();

        /// <summary>
        /// Detailed token usage statistics for this request and response.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Very useful for:
        /// <list type="bullet">
        /// <item>Cost estimation</item>
        /// <item>Context window management</item>
        /// <item>Rate-limit awareness</item>
        /// </list>
        /// </para>
        /// <para>
        /// Counts include all input tokens (system + user + images) + generated tokens.
        /// </para>
        /// </remarks>
        [JsonPropertyName("usage")]
        public Usage Usage { get; set; } = null!;

        // ────────────────────────────────────────────────────────────────────────────────
        // Convenience Helpers
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the first (or only) assistant message content, or null if no choices exist.
        /// </summary>
        [JsonIgnore]
        public string? FirstContent =>
            Choices?.Count > 0 ? Choices[0].Message?.Content?.ToString() : null;

        /// <summary>
        /// Gets whether the response contains at least one valid completion choice.
        /// </summary>
        [JsonIgnore]
        public bool HasChoices => Choices?.Count > 0;

        /// <summary>
        /// Gets the finish reason of the first choice (or null if no choices).
        /// </summary>
        [JsonIgnore]
        public string? FirstFinishReason =>
            Choices?.Count > 0 ? Choices[0].FinishReason : null;
    }
}