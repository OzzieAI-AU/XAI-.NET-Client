namespace xAINetClient
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents one individual completion choice within a <see cref="ChatCompletionResponse"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When you request multiple completions via the <c>n</c> parameter (default = 1), the API returns 
    /// multiple <see cref="Choice"/> objects — each containing one independently generated assistant message.
    /// </para>
    /// <para>
    /// In the vast majority of real-world use cases:
    /// <list type="bullet">
    /// <item><c>n = 1</c> → exactly one <see cref="Choice"/> (index 0)</item>
    /// <item>You almost always use <c>Choices[0]</c> as the primary / best reply</item>
    /// <item>Multiple choices are mainly useful for:
    ///   <list type="bullet">
    ///   <item>Best-of-n sampling (pick the highest quality reply)</item>
    ///   <item>Exploring creative variation</item>
    ///   <item>Evaluation / A/B testing</item>
    ///   </list>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Key fields and their meaning:</strong>
    /// <list type="bullet">
    /// <item><see cref="Index"/> — position in the returned list (0-based, always sequential)</item>
    /// <item><see cref="Message"/> — the actual assistant reply (role = "assistant", content = generated text or multimodal parts)</item>
    /// <item><see cref="FinishReason"/> — why generation stopped (critical for understanding truncation or limits)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Common <see cref="FinishReason"/> values (as observed in Grok API):
    /// <list type="bullet">
    /// <item><c>"stop"</c> — natural end (model decided to finish)</item>
    /// <item><c>"length"</c> — hit <c>max_tokens</c> limit</item>
    /// <item><c>"content_filter"</c> — blocked by safety filter (rare)</item>
    /// <item><c>"tool_calls"</c> — model wants to call tools/functions (if tool calling enabled)</item>
    /// <item><c>null</c> — incomplete / streaming partial (not applicable here)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// var response = await client.ChatAsync(request);
    ///
    /// if (response.HasChoices)
    /// {
    ///     var firstChoice = response.Choices[0];
    ///     Console.WriteLine($"Reply (index {firstChoice.Index}):");
    ///     Console.WriteLine(firstChoice.Message.Content);
    ///
    ///     if (firstChoice.FinishReason == "length")
    ///     {
    ///         Console.WriteLine("Warning: Generation was truncated due to max_tokens limit.");
    ///     }
    ///     else if (firstChoice.FinishReason == "tool_calls")
    ///     {
    ///         Console.WriteLine("Model requested tool calls — handle accordingly.");
    ///     }
    /// }
    ///
    /// // Showing all choices (if n > 1)
    /// foreach (var choice in response.Choices)
    /// {
    ///     Console.WriteLine($"Choice {choice.Index} ({choice.FinishReason}): {choice.Message.Content}");
    /// }
    /// </code>
    /// </example>
    public class Choice
    {
        /// <summary>
        /// Zero-based index of this choice in the list of returned completions.
        /// </summary>
        /// <remarks>
        /// Always sequential starting from 0.  
        /// Matches the order in which the API generated the choices.
        /// </remarks>
        /// <value>0 for the first (primary) choice, 1 for the second, etc.</value>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>
        /// The assistant message generated for this choice.
        /// </summary>
        /// <remarks>
        /// <para>
        /// • <c>Role</c> is always <c>"assistant"</c>  
        /// • <c>Content</c> is usually a <see langword="string"/>, but may be a <see cref="List{ContentPart}"/> 
        ///   in future multimodal output scenarios  
        /// • Contains the actual reply text the model produced
        /// </para>
        /// <para>
        /// This is typically what you display to the user or feed back into the next message.
        /// </para>
        /// </remarks>
        [JsonPropertyName("message")]
        public ChatMessage Message { get; set; } = null!;

        /// <summary>
        /// The reason the model stopped generating tokens for this choice.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Most common values:
        /// <list type="bullet">
        /// <item><c>"stop"</c> — normal completion (recommended)</item>
        /// <item><c>"length"</c> — reached <c>max_tokens</c> (consider increasing limit)</item>
        /// <item><c>"tool_calls"</c> — model wants to invoke tools (if enabled)</item>
        /// <item><c>"content_filter"</c> — blocked by safety system</item>
        /// </list>
        /// </para>
        /// <para>
        /// <c>null</c> is rare in non-streaming responses — usually indicates an incomplete or errored generation.
        /// </para>
        /// </remarks>
        /// <value>One of the standard finish reason strings, or <see langword="null"/>.</value>
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        // ────────────────────────────────────────────────────────────────────────────────
        // Convenience Helpers
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the content of the assistant message (string or list of parts), or null if missing.
        /// </summary>
        [JsonIgnore]
        public object? Content => Message?.Content;

        /// <summary>
        /// Gets whether this choice completed normally (finish reason is "stop" or similar).
        /// </summary>
        [JsonIgnore]
        public bool IsNormalCompletion =>
            FinishReason == "stop" || FinishReason == null; // null is rare but often safe

        /// <summary>
        /// Convenience property indicating whether this message contains tool calls.
        /// </summary>
        [JsonIgnore]
        public bool HasToolCalls => Message?.HasToolCalls == true;

        /// <summary>
        /// Gets whether generation was truncated due to token limit.
        /// </summary>
        [JsonIgnore]
        public bool WasTruncated => FinishReason == "length";
    }
}