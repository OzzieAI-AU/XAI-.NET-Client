namespace OzzieAI.XAI
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a single content part in a multimodal <see cref="ChatMessage"/> 
    /// for the xAI Grok API chat completions endpoint.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Grok supports two content formats for each message:
    /// </para>
    /// <list type="bullet">
    /// <item>A simple <see langword="string"/> → plain text-only message</item>
    /// <item>A <see cref="List{ContentPart}"/> → multimodal message (text + images, extensible in future)</item>
    /// </list>
    /// <para>
    /// This class models **one** part inside that list.
    /// As of March 2026, the only officially supported part types are:
    /// </para>
    /// <list type="bullet">
    /// <item><c>"text"</c> — plain text content</item>
    /// <item><c>"image_url"</c> — reference to an image (remote URL or base64 data URI)</item>
    /// </list>
    /// <para>
    /// <strong>Important limitations (current API):</strong>
    /// <list type="bullet">
    /// <item>No native audio or video input parts in chat completions</item>
    /// <item>No custom tool/result parts (unlike tool calling responses)</item>
    /// <item>Maximum ~20 MB per image (API enforced)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Serialization behavior (System.Text.Json):
    /// <list type="bullet">
    /// <item><c>Type = "text"</c> → only <see cref="Text"/> is serialized</item>
    /// <item><c>Type = "image_url"</c> → only <see cref="ImageUrl"/> is serialized</item>
    /// </list>
    /// The API ignores extraneous properties.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// // Multimodal user message: text + two images
    /// var message = new ChatMessage
    /// {
    ///     Role = "user",
    ///     Content = new List&lt;ContentPart&gt;
    ///     {
    ///         ContentPart.Text("What do these two photos have in common?"),
    ///         ContentPart.Image("https://example.com/photo1.jpg", "high"),
    ///         ContentPart.Image("data:image/jpeg;base64,/9j/4AAQSkZJRgABAQE...==", "auto")
    ///     }
    /// };
    /// </code>
    /// </example>
    public class ContentPart
    {
        /// <summary>
        /// The type identifier of this content part.
        /// </summary>
        /// <remarks>
        /// Must be exactly one of:
        /// <list type="bullet">
        /// <item><c>"text"</c> — requires <see cref="Text"/> to be set</item>
        /// <item><c>"image_url"</c> — requires <see cref="ImageUrl"/> to be set</item>
        /// </list>
        /// Any other value will almost certainly be rejected by the Grok API.
        /// </remarks>
        /// <value>Typically <c>"text"</c> or <c>"image_url"</c>.</value>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// The plain text payload (used when <see cref="Type"/> = <c>"text"</c>).
        /// </summary>
        /// <remarks>
        /// Required for text parts.  
        /// Should be <see langword="null"/> for image parts (API ignores it anyway).
        /// </remarks>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Image reference object (used when <see cref="Type"/> = <c>"image_url"</c>).
        /// </summary>
        /// <remarks>
        /// Required for image parts.  
        /// Must contain a valid <c>url</c>; <c>detail</c> is optional.  
        /// Should be <see langword="null"/> for text parts.
        /// </remarks>
        [JsonPropertyName("image_url")]
        public ImageDetail? ImageUrl { get; set; }

        // ────────────────────────────────────────────────────────────────────────
        // Factory Methods — Recommended way to create parts safely
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a text-only content part.
        /// </summary>
        /// <param name="text">The text content (cannot be null).</param>
        /// <returns>A fully configured text <see cref="ContentPart"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public static ContentPart CreateText(string text) => new()
        {
            Type = "text",
            Text = text ?? throw new ArgumentNullException(nameof(text))
        };

        /// <summary>
        /// Creates an image content part from a URL or base64 data URI.
        /// </summary>
        /// <param name="imageUrl">
        /// HTTP/HTTPS URL or <c>data:image/...</c> base64 URI (cannot be null).
        /// </param>
        /// <param name="detail">
        /// Detail level hint: <c>"low"</c>, <c>"high"</c>, or <c>"auto"</c> (default).
        /// </param>
        /// <returns>A fully configured image <see cref="ContentPart"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageUrl"/> is null.</exception>
        public static ContentPart Image(string imageUrl, string detail = "auto") => new()
        {
            Type = "image_url",
            ImageUrl = new ImageDetail
            {
                Url = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl)),
                Detail = detail
            }
        };
    }
}