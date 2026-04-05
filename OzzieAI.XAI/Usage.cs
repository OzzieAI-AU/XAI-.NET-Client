namespace OzzieAI.XAI
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Contains detailed token usage statistics for a single chat completion request and its response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object is included in every non-streaming <see cref="ChatCompletionResponse"/> 
    /// to help track input/output consumption, estimate costs, manage context windows, 
    /// and debug token-related issues.
    /// </para>
    /// <para>
    /// <strong>Key facts about token counting in Grok API (March 2026):</strong>
    /// <list type="bullet">
    /// <item>All tokens are counted using the model's native tokenizer (usually similar to GPT-like BPE)</item>
    /// <item><see cref="PromptTokens"/> includes:
    ///   <list type="bullet">
    ///   <item>System message (if present)</item>
    ///   <item>All user and assistant messages in history</item>
    ///   <item>Any image tokens (vision inputs are tokenized as fixed-size patches)</item>
    ///   <item>Metadata overhead (roles, formatting, etc.)</item>
    ///   </list>
    /// </item>
    /// <item><see cref="CompletionTokens"/> counts only the generated assistant tokens</item>
    /// <item><see cref="TotalTokens"/> = <see cref="PromptTokens"/> + <see cref="CompletionTokens"/></item>
    /// <item>Image inputs can add hundreds to thousands of tokens depending on resolution/detail level</item>
    /// <item>No separate breakdown for cached/prefixed tokens (unlike some other providers)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Typical usage patterns:
    /// <list type="bullet">
    /// <item>Log <see cref="TotalTokens"/> after each call for monitoring</item>
    /// <item>Check <see cref="PromptTokens"/> to detect context overflow risks</item>
    /// <item>Alert if <see cref="CompletionTokens"/> is close to requested <c>max_tokens</c> (possible truncation)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// var response = await client.ChatAsync(request);
    ///
    /// Console.WriteLine($"Tokens used this call:");
    /// Console.WriteLine($"  Prompt:     {response.Usage.PromptTokens:N0}");
    /// Console.WriteLine($"  Completion: {response.Usage.CompletionTokens:N0}");
    /// Console.WriteLine($"  Total:      {response.Usage.TotalTokens:N0}");
    ///
    /// if (response.Usage.CompletionTokens >= request.MaxTokens)
    /// {
    ///     Console.WriteLine("Warning: Generation likely hit max_tokens limit.");
    /// }
    ///
    /// // Running total example (in a conversation loop)
    /// conversationTotalTokens += response.Usage.TotalTokens;
    /// Console.WriteLine($"Session total tokens: {conversationTotalTokens:N0}");
    /// </code>
    /// </example>
    public class Usage
    {
        /// <summary>
        /// Number of tokens consumed by the input prompt (system + all messages + images).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the dominant cost driver in most conversations.  
        /// Large history, long system instructions, or high-detail images can make this number grow quickly.
        /// </para>
        /// <para>
        /// Typical range: 100–32,000+ (depending on model context window)
        /// </para>
        /// </remarks>
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        /// <summary>
        /// Number of tokens generated in the assistant's reply for this completion.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Controlled by <c>max_tokens</c> parameter (if set).  
        /// If the value equals or approaches your requested limit, generation was likely truncated.
        /// </para>
        /// <para>
        /// Typical range: 10–8192+ (model-dependent max)
        /// </para>
        /// </remarks>
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Total tokens used for this request (prompt + completion).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Simple sum: <c>PromptTokens + CompletionTokens</c>.  
        /// Most billing/cost tracking uses this number.
        /// </para>
        /// <para>
        /// Useful for aggregate session tracking or quota enforcement.
        /// </para>
        /// </remarks>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        // ────────────────────────────────────────────────────────────────────────────────
        // Convenience Helpers
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets a formatted string showing all three token counts.
        /// </summary>
        /// <returns>A human-readable summary, e.g. "Prompt: 1,234 | Completion: 567 | Total: 1,801"</returns>
        public override string ToString() =>
            $"Prompt: {PromptTokens:N0} | Completion: {CompletionTokens:N0} | Total: {TotalTokens:N0}";

        /// <summary>
        /// Gets whether generation was likely truncated due to token limits.
        /// </summary>
        /// <remarks>
        /// Conservative check — assumes you set a <c>max_tokens</c> value.
        /// </remarks>
        [JsonIgnore]
        public bool LikelyTruncated =>
            CompletionTokens > 0 && CompletionTokens >= PromptTokens * 0.8; // rough heuristic

        /// <summary>
        /// Gets the average tokens per word approximation (rough estimate).
        /// </summary>
        [JsonIgnore]
        public double ApproxTokensPerWord =>
            CompletionTokens > 0 ? CompletionTokens / (double)(CompletionTokens / 0.75) : 0;
    }
}