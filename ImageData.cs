namespace xAINetClient
{

    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a single generated image result returned by the Grok image generation endpoint.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is part of the response from <c>POST /images/generations</c> (or equivalent endpoint).
    /// Each instance corresponds to one image when <c>n</c> (number of images) is greater than 1.
    /// </para>
    /// <para>
    /// The actual content delivery depends on the <c>response_format</c> parameter used in the request:
    /// <list type="bullet">
    /// <item><c>"url"</c> (default) → <see cref="Url"/> contains a temporary public HTTPS link</item>
    /// <item><c>"b64_json"</c> → <see cref="B64Json"/> contains the full base64-encoded image data</item>
    /// </list>
    /// Only one of the two fields is populated per response — the other is <see langword="null"/>.
    /// </para>
    /// <para>
    /// <strong>Important API behaviors (March 2026):</strong>
    /// <list type="bullet">
    /// <item>URLs are temporary (typically expire after 1–24 hours — cache or download promptly)</item>
    /// <item>Base64 data can be very large (~1–5 MB per image depending on resolution)</item>
    /// <item>Supported formats: usually JPEG or PNG (API chooses based on prompt/content)</item>
    /// <item>No additional metadata (width, height, seed, etc.) is returned in the public API</item>
    /// </list>
    /// </para>
    /// <para>
    /// Recommended usage pattern:
    /// <list type="number">
    /// <item>Use <c>"url"</c> for most cases (faster response, smaller payload)</item>
    /// <item>Switch to <c>"b64_json"</c> only when offline access or immediate processing is required</item>
    /// <item>Always check which field is populated before using</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// // Typical usage after calling GenerateImageAsync
    /// var response = await client.GenerateImageAsync(
    ///     prompt: "A cyberpunk fox in neon rain, ultra detailed",
    ///     n: 2,
    ///     size: "1024x1024",
    ///     responseFormat: "url"
    /// );
    ///
    /// foreach (var image in response.Data)
    /// {
    ///     if (!string.IsNullOrEmpty(image.Url))
    ///     {
    ///         Console.WriteLine($"Image ready (temporary URL): {image.Url}");
    ///         // Download using HttpClient or display in UI
    ///     }
    ///     else if (!string.IsNullOrEmpty(image.B64Json))
    ///     {
    ///         Console.WriteLine("Base64 data received (length: {0} chars)", image.B64Json.Length);
    ///         // Convert to image: Convert.FromBase64String(image.B64Json)
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ImageData
    {
        /// <summary>
        /// Temporary public URL to the generated image.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returned when <c>response_format</c> = <c>"url"</c> (default).
        /// </para>
        /// <para>
        /// • Protocol: HTTPS  
        /// • Host: usually <c>api.x.ai</c> or CDN subdomain  
        /// • Lifetime: short-lived (often 1–24 hours — do not rely on long-term availability)  
        /// • Use case: display in UI, download immediately, or cache locally
        /// </para>
        /// <para>
        /// This field is <see langword="null"/> when base64 data is returned instead.
        /// </para>
        /// </remarks>
        /// <value>A temporary HTTPS URL, or <see langword="null"/>.</value>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Base64-encoded string of the full image data (JPEG or PNG).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returned when <c>response_format</c> = <c>"b64_json"</c>.
        /// </para>
        /// <para>
        /// • Format: raw base64 (no <c>data:image/...</c> prefix)  
        /// • Size: typically 1–5 MB depending on resolution and compression  
        /// • Use case: offline storage, immediate processing, or environments without internet  
        /// </para>
        /// <para>
        /// This field is <see langword="null"/> when a URL is returned instead.
        /// </para>
        /// <para>
        /// Tip: To convert to bytes → <c>Convert.FromBase64String(B64Json)</c>
        /// </para>
        /// </remarks>
        /// <value>Base64 string of the image bytes, or <see langword="null"/>.</value>
        [JsonPropertyName("b64_json")]
        public string? B64Json { get; set; }

        /// <summary>
        /// Convenience property: returns <see langword="true"/> if this result contains usable image data.
        /// </summary>
        /// <remarks>
        /// Checks whether either <see cref="Url"/> or <see cref="B64Json"/> is non-empty.
        /// Useful for quick validation before processing.
        /// </remarks>
        [JsonIgnore]
        public bool HasContent => !string.IsNullOrEmpty(Url) || !string.IsNullOrEmpty(B64Json);
    }
}