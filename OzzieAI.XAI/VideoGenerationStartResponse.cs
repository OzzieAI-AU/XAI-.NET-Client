namespace OzzieAI.XAI
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the immediate response returned when starting an asynchronous video generation job 
    /// via the Grok Imagine Video endpoint (<c>POST /v1/videos/generations</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Video generation is fully asynchronous: this object is returned <strong>immediately</strong> 
    /// after a successful request, usually containing only a <c>request_id</c> (and sometimes minimal metadata).
    /// The actual video creation happens in the background and can take 30 seconds to several minutes.
    /// </para>
    /// <para>
    /// To check progress and retrieve the final video URL, repeatedly poll the status endpoint:
    /// <c>GET /v1/videos/{request_id}</c> until <c>status</c> becomes <c>"completed"</c> or <c>"failed"</c>.
    /// </para>
    /// <para>
    /// <strong>Current API behavior notes (March 2026):</strong>
    /// <list type="bullet">
    /// <item>Most responses contain only <c>request_id</c> — other fields (<c>status</c>, <c>prompt</c>, etc.) 
    /// are often absent or <see langword="null"/> at start time.</item>
    /// <item><c>status</c> is rarely present here; it becomes meaningful only during polling.</item>
    /// <item><c>expires_at</c> (if returned) indicates when the job or result may be purged.</item>
    /// <item>No final video URL is included in this response — it appears only in the polling result.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Recommended pattern:
    /// <list type="number">
    /// <item>Call <see cref="GrokClient.StartVideoGenerationAsync"/> → get this object</item>
    /// <item>Extract <see cref="Id"/> (request_id)</item>
    /// <item>Poll <see cref="GrokClient.GetVideoGenerationStatusAsync"/> until completion</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// var startResponse = await client.StartVideoGenerationAsync(
    ///     prompt: "A futuristic cityscape at night with flying cars, cinematic",
    ///     durationSeconds: 8,
    ///     aspectRatio: "16:9"
    /// );
    ///
    /// Console.WriteLine($"Video job started. Request ID: {startResponse.Id}");
    ///
    /// // Now poll until done (use GenerateVideoAndWaitAsync for convenience)
    /// var finalStatus = await client.GenerateVideoAndWaitAsync(/* same params */);
    /// if (finalStatus.Status == "completed")
    /// {
    ///     Console.WriteLine($"Video ready: {finalStatus.Url}");
    /// }
    /// </code>
    /// </example>
    public class VideoGenerationStartResponse
    {
        /// <summary>
        /// Unique identifier for this video generation job (request ID).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the **only reliably present field** in most start responses.
        /// </para>
        /// <para>
        /// Use this value to poll for status updates via <c>GET /v1/videos/{request_id}</c>.
        /// </para>
        /// <para>
        /// Format: usually a string like <c>"req_abc123xyz"</c> or UUID-like.
        /// </para>
        /// </remarks>
        /// <value>The request/job identifier (never null on success).</value>
        [JsonPropertyName("request_id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Current status of the generation job at the time of starting.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Usually absent or <c>"pending"</c>/<c>"queued"</c> in the start response.
        /// </para>
        /// <para>
        /// Meaningful values are seen only during polling:
        /// <list type="bullet">
        /// <item><c>"pending"</c> / <c>"queued"</c></item>
        /// <item><c>"processing"</c> / <c>"running"</c></item>
        /// <item><c>"completed"</c></item>
        /// <item><c>"failed"</c></item>
        /// <item><c>"cancelled"</c> (rare)</item>
        /// </list>
        /// </para>
        /// <para>
        /// This field is often <see langword="null"/> in the initial response.
        /// </para>
        /// </remarks>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Unix timestamp (seconds) when the generation job was created.
        /// </summary>
        /// <remarks>
        /// Usually present and matches the time of the API call.
        /// </remarks>
        /// <value>Seconds since Unix epoch (January 1, 1970 UTC).</value>
        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// Optional Unix timestamp when this job/result is expected to expire.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If present, indicates when the temporary video URL or job data may be deleted.
        /// </para>
        /// <para>
        /// Typical lifetime after completion: 1–48 hours (download promptly).
        /// </para>
        /// <para>
        /// Often <see langword="null"/> in the start response.
        /// </para>
        /// </remarks>
        [JsonPropertyName("expires_at")]
        public long? ExpiresAt { get; set; }

        /// <summary>
        /// The prompt text that was submitted to generate this video.
        /// </summary>
        /// <remarks>
        /// Echoed back from the request.  
        /// Frequently absent in the start response (to reduce payload size).
        /// </remarks>
        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }

        /// <summary>
        /// The model identifier used for this generation job.
        /// </summary>
        /// <remarks>
        /// Echoed back from the request (usually <c>"grok-imagine-video"</c>).  
        /// May be absent in minimal start responses.
        /// </remarks>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        // ────────────────────────────────────────────────────────────────────────────────
        // Convenience Properties
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets a value indicating whether this response contains a valid request ID.
        /// </summary>
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrWhiteSpace(Id);
    }
}