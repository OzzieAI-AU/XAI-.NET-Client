namespace xAINetClient
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the current status and result of an asynchronous video generation job 
    /// when polling the Grok Imagine Video status endpoint (<c>GET /v1/videos/{request_id}</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the primary response object returned when polling the status of a video job started 
    /// via <see cref="StartVideoGenerationAsync"/>. You should call this endpoint repeatedly 
    /// (every 5–15 seconds) until <see cref="Status"/> reaches a terminal state (<c>"completed"</c> or <c>"failed"</c>).
    /// </para>
    /// <para>
    /// <strong>Typical lifecycle and field availability (March 2026):</strong>
    /// <list type="bullet">
    /// <item>Early stages (<c>"pending"</c> / <c>"queued"</c>): only <see cref="Id"/> and <see cref="Status"/> are usually present</item>
    /// <item>During generation (<c>"processing"</c> / <c>"running"</c>): <see cref="Progress"/> may appear (0.0–1.0)</item>
    /// <item>Terminal states:
    ///   <list type="bullet">
    ///   <item><c>"completed"</c> → <see cref="Url"/> contains the final temporary video MP4 link</item>
    ///   <item><c>"failed"</c> → <see cref="Error"/> contains a human-readable error message or code</item>
    ///   </list>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Important notes:</strong>
    /// <list type="bullet">
    /// <item><see cref="Url"/> and <see cref="ThumbnailUrl"/> are temporary (usually expire 1–48 hours after completion — download promptly)</item>
    /// <item>Video includes native audio (no toggle documented to disable it)</item>
    /// <item><see cref="Progress"/> is not always provided — some jobs jump straight to completed</item>
    /// <item>No width/height/metadata returned — only the file URL</item>
    /// </list>
    /// </para>
    /// <para>
    /// Recommended usage: Use <see cref="GrokClient.GenerateVideoAndWaitAsync"/> convenience method 
    /// to handle polling automatically, or implement your own loop checking <see cref="IsTerminal"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// // Polling example (simplified)
    /// string requestId = startResponse.Id;
    /// VideoGenerationStatusResponse status;
    ///
    /// do
    /// {
    ///     status = await client.GetVideoGenerationStatusAsync(requestId);
    ///     Console.WriteLine($"Status: {status.Status} | Progress: {status.Progress:P0}");
    ///
    ///     if (status.IsCompleted)
    ///     {
    ///         Console.WriteLine($"Video ready: {status.Url}");
    ///         break;
    ///     }
    ///     if (status.IsFailed)
    ///     {
    ///         Console.WriteLine($"Failed: {status.Error}");
    ///         break;
    ///     }
    ///
    ///     await Task.Delay(8000); // ~8 seconds
    /// }
    /// while (!status.IsTerminal);
    /// </code>
    /// </example>
    public class VideoGenerationStatusResponse
    {
        /// <summary>
        /// The unique request/job identifier (matches the ID from <see cref="VideoGenerationStartResponse"/>).
        /// </summary>
        /// <remarks>
        /// Always present. Used to correlate polling responses with the original request.
        /// </remarks>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Current status of the video generation job.
        /// </summary>
        /// <remarks>
        /// Common values (case-sensitive):
        /// <list type="bullet">
        /// <item><c>"pending"</c> / <c>"queued"</c> — waiting to start</item>
        /// <item><c>"processing"</c> / <c>"running"</c> — generating</item>
        /// <item><c>"completed"</c> — success (URL available)</item>
        /// <item><c>"failed"</c> — error occurred (see <see cref="Error"/>)</item>
        /// <item><c>"cancelled"</c> (rare)</item>
        /// </list>
        /// Terminal states: <c>"completed"</c>, <c>"failed"</c>, <c>"cancelled"</c>.
        /// </remarks>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        /// <summary>
        /// Estimated progress of video generation (0.0 to 1.0).
        /// </summary>
        /// <remarks>
        /// <para>
        /// • May be absent (<see langword="null"/>) in early or fast jobs  
        /// • When present: 0.0 = just started, 1.0 = almost done  
        /// • Not guaranteed to be linear or updated every poll
        /// </para>
        /// </remarks>
        /// <value>Progress fraction, or <see langword="null"/> if not reported.</value>
        [JsonPropertyName("progress")]
        public float? Progress { get; set; }

        /// <summary>
        /// Temporary public URL to the completed video file (MP4).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only populated when <see cref="Status"/> = <c>"completed"</c>.  
        /// Expires typically 1–48 hours after completion — download immediately.
        /// </para>
        /// <para>
        /// Format: direct HTTPS link to MP4 with native audio.
        /// </para>
        /// </remarks>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Optional URL to a thumbnail/preview image of the generated video.
        /// </summary>
        /// <remarks>
        /// May be <see langword="null"/> even on success.  
        /// Useful for UI previews before downloading the full video.
        /// </remarks>
        [JsonPropertyName("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Error message or code if the job failed.
        /// </summary>
        /// <remarks>
        /// Only meaningful when <see cref="Status"/> = <c>"failed"</c>.  
        /// Examples: "invalid prompt", "rate limit exceeded", "internal error", etc.
        /// </remarks>
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>
        /// Unix timestamp (seconds) when the job reached "completed" status.
        /// </summary>
        /// <remarks>
        /// Absent until completion.  
        /// Useful for calculating generation time.
        /// </remarks>
        [JsonPropertyName("completed_at")]
        public long? CompletedAt { get; set; }

        /// <summary>
        /// Actual or requested duration of the generated video in seconds.
        /// </summary>
        /// <remarks>
        /// Usually matches the requested <c>duration</c> on success.  
        /// May be absent or differ slightly if the API adjusted it.
        /// </remarks>
        [JsonPropertyName("duration_seconds")]
        public int? DurationSeconds { get; set; }

        // ────────────────────────────────────────────────────────────────────────────────
        // Convenience Properties
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets a value indicating whether the job has reached a terminal state.
        /// </summary>
        [JsonIgnore]
        public bool IsTerminal =>
            Status?.ToLowerInvariant() is "completed" or "failed" or "cancelled";

        /// <summary>
        /// Gets a value indicating whether the job completed successfully with a video URL.
        /// </summary>
        [JsonIgnore]
        public bool IsCompleted =>
            Status?.ToLowerInvariant() == "completed" && !string.IsNullOrEmpty(Url);

        /// <summary>
        /// Gets a value indicating whether the job failed.
        /// </summary>
        [JsonIgnore]
        public bool IsFailed =>
            Status?.ToLowerInvariant() == "failed" || !string.IsNullOrEmpty(Error);
    }
}