using System.Text.Json.Serialization;

namespace xAINetClient
{
    /// <summary>
    /// Details for an image referenced in a <see cref="ContentPart"/> of type "image_url".
    /// </summary>
    /// <remarks>
    /// Matches the nested object expected by the Grok API in multimodal messages.
    /// </remarks>
    public class ImageDetail
    {
        /// <summary>
        /// The URL of the image.
        /// </summary>
        /// <remarks>
        /// Can be:
        /// <list type="bullet">
        /// <item>Public HTTPS URL (e.g. "https://example.com/image.jpg")</item>
        /// <item>Base64 data URI (e.g. "data:image/jpeg;base64,/9j/4AAQSkZJRg...")</item>
        /// </list>
        /// Maximum size ~20 MB (API-enforced).
        /// </remarks>
        [JsonPropertyName("url")]
        public string Url { get; set; } = null!;

        /// <summary>
        /// Controls how much detail the model should extract from the image.
        /// </summary>
        /// <remarks>
        /// Allowed values (as of 2026):
        /// <list type="bullet">
        /// <item><c>"low"</c>   — faster, cheaper, lower quality analysis</item>
        /// <item><c>"high"</c>  — slower, more expensive, detailed analysis</item>
        /// <item><c>"auto"</c>  — let the model decide (default/recommended)</item>
        /// </list>
        /// </remarks>
        /// <value>"low", "high", or "auto". Defaults to "auto" if omitted.</value>
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }
    }
}