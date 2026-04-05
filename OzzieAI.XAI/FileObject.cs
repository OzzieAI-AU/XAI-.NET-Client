namespace OzzieAI.XAI
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents metadata for a file that was uploaded to the xAI Grok API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Important note (March 2026):</strong>
    /// The public xAI Grok API does **not** currently expose a <c>/files</c> upload endpoint 
    /// or file-object referencing system similar to OpenAI's Assistants API or Files API.
    /// </para>
    /// <para>
    /// <strong>Status (March 2026):</strong> The xAI Files API is now live! 
    /// Upload via <c>POST /v1/files</c> and reference the returned <c>id</c> in chat messages.
    /// </para>
    /// <para>
    /// This class is included here as:
    /// <list type="bullet">
    /// <item>A placeholder / forward-compatible model in case xAI adds file upload support later</item>
    /// <item>A reference shape that matches common patterns in similar LLM APIs</item>
    /// <item>A reminder that file handling is **not** yet available in the public API</item>
    /// </list>
    /// </para>
    /// <para>
    /// Current recommended alternatives for sending documents/content to Grok:
    /// <list type="bullet">
    /// <item>Copy-paste relevant text excerpts directly into the chat prompt (Grok 4.20 has very large context)</item>
    /// <item>Use vision for images/charts/screenshots via <c>image_url</c> content parts</item>
    /// <item>Wait for official file upload / RAG / document search features (not yet announced publicly)</item>
    /// </list>
    /// </para>
    /// <para>
    /// If/when xAI implements file uploads, this model is structured to match typical expected fields:
    /// <c>id</c> for referencing, <c>filename</c>, size in bytes, creation timestamp, and purpose category.
    /// </para>
    /// </remarks>
    public class FileObject
    {
        /// <summary>
        /// Unique identifier for the uploaded file.
        /// </summary>
        /// <remarks>
        /// This ID would be used to reference the file in future chat requests 
        /// (e.g. in a hypothetical <c>file_ids</c> array or tool call).
        /// Currently unused — no file upload endpoint exists.
        /// </remarks>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Original name of the file as provided during upload.
        /// </summary>
        /// <remarks>
        /// Preserves the client-side filename (e.g. "Q4-financial-report.pdf").
        /// Sanitized by the API if needed.
        /// </remarks>
        [JsonPropertyName("filename")]
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        /// <remarks>
        /// Useful for validating upload success or enforcing client-side limits.
        /// </remarks>
        [JsonPropertyName("bytes")]
        public long Bytes { get; set; }

        /// <summary>
        /// Unix timestamp (seconds) when the file was successfully uploaded.
        /// </summary>
        /// <remarks>
        /// Represents server-side creation time.
        /// </remarks>
        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// The intended purpose/category of the file.
        /// </summary>
        /// <remarks>
        /// In APIs that support it (e.g. OpenAI), common values include:
        /// <list type="bullet">
        /// <item><c>"assistants"</c> — for use in chat / assistants context</item>
        /// <item><c>"fine-tune"</c> — training data</item>
        /// <item><c>"vision"</c> — image analysis</item>
        /// </list>
        /// In Grok's current API this field does not exist — defaulted here for compatibility.
        /// </remarks>
        /// <value>Usually <c>"assistants"</c> when supported.</value>
        [JsonPropertyName("purpose")]
        public string Purpose { get; set; } = "assistants";

        // ────────────────────────────────────────────────────────────────────────────────
        // Convenience / Helper Properties
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets a human-readable file size string (e.g. "2.4 MB").
        /// </summary>
        [JsonIgnore]
        public string SizeDisplay
        {
            get
            {
                if (Bytes < 1024) return $"{Bytes} bytes";
                if (Bytes < 1024 * 1024) return $"{Bytes / 1024.0:F1} KB";
                if (Bytes < 1024L * 1024 * 1024) return $"{Bytes / (1024.0 * 1024):F1} MB";
                return $"{Bytes / (1024.0 * 1024 * 1024):F2} GB";
            }
        }

        /// <summary>
        /// Gets whether this appears to be a valid file object.
        /// </summary>
        [JsonIgnore]
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Id) &&
            !string.IsNullOrWhiteSpace(FileName) &&
            Bytes >= 0;
    }
}