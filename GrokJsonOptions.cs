namespace xAINetClient
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Static container providing consistent <see cref="JsonSerializerOptions"/> configurations 
    /// used throughout the GrokClient library for all JSON serialization and deserialization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class centralizes JSON options to ensure uniform behavior across:
    /// <list type="bullet">
    /// <item>Request serialization (<see cref="HttpClient.PostAsJsonAsync"/>)</item>
    /// <item>Response deserialization (<see cref="HttpContent.ReadFromJsonAsync{T}"/>)</item>
    /// <item>Configuration file read/write (<see cref="GrokClientConfig"/> save/load)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Chosen defaults (optimized for xAI Grok API):</strong>
    /// <list type="bullet">
    /// <item><c>PropertyNamingPolicy = CamelCase</c> — matches the API's wire format (e.g. <c>max_tokens</c>, <c>finish_reason</c>)</item>
    /// <item><c>PropertyNameCaseInsensitive = true</c> — tolerant of minor case variations in API responses or user configs</item>
    /// <item><c>DefaultIgnoreCondition = WhenWritingNull</c> — reduces payload size by omitting null properties in requests</item>
    /// <item><c>WriteIndented = false</c> — compact output for network efficiency (pretty-print only for human-readable config files)</item>
    /// </list>
    /// </para>
    /// <para>
    /// These options are intentionally strict and performant — no loose parsing, no extra converters unless explicitly needed.
    /// If the API ever introduces breaking changes (e.g. snake_case → PascalCase), only this single location needs adjustment.
    /// </para>
    /// <para>
    /// Usage pattern (already followed in the client):
    /// <list type="bullet">
    /// <item><c>PostAsJsonAsync(..., GrokJsonOptions.Default)</c></item>
    /// <item><c>ReadFromJsonAsync&lt;T&gt;(..., GrokJsonOptions.Default)</c></item>
    /// </list>
    /// </para>
    /// </remarks>
    internal static class GrokJsonOptions
    {
        /// <summary>
        /// The default <see cref="JsonSerializerOptions"/> instance used for all API communication 
        /// and most internal serialization tasks.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>CamelCase property names (matches Grok API contract)</item>
        /// <item>Case-insensitive property matching (robust against minor API changes)</item>
        /// <item>Null values omitted when writing (smaller payloads)</item>
        /// <item>Compact (non-indented) output (network-efficient)</item>
        /// </list>
        /// This instance is read-only and thread-safe — safe to reuse across all requests.
        /// </remarks>
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,

            // Optional: add more custom settings here in the future if needed
            // NumberHandling         = JsonNumberHandling.AllowReadingFromString,
            // Converters.Add(new CustomConverter())
        };

        // ────────────────────────────────────────────────────────────────────────────────
        // Optional specialized options (add as needed)
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Pretty-print options — used only for human-readable configuration file output 
        /// (e.g. <see cref="GrokClientConfig.SaveToJson"/>).
        /// </summary>
        /// <remarks>
        /// Same rules as <see cref="Default"/>, but with indentation enabled for readability.
        /// </remarks>
        public static readonly JsonSerializerOptions Pretty = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}