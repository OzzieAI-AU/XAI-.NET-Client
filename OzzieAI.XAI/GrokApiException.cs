namespace OzzieAI.XAI
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The exception thrown when the xAI Grok API returns a non-success HTTP status code 
    /// (anything other than 2xx) during a request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the primary error type surfaced by <see cref="GrokClient"/> methods 
    /// (<see cref="ChatAsync"/>, <see cref="GenerateImageAsync"/>, video generation, etc.).
    /// It wraps both the HTTP status code and — when available — the raw error response body 
    /// returned by the API, making debugging significantly easier.
    /// </para>
    /// <para>
    /// <strong>Common status codes & their typical meanings (Grok API, March 2026):</strong>
    /// <list type="bullet">
    /// <item><c>400</c> — Bad Request (invalid parameters, malformed JSON, missing required fields)</item>
    /// <item><c>401</c> — Unauthorized (invalid or missing API key)</item>
    /// <item><c>403</c> — Forbidden (insufficient permissions, quota exceeded, region restriction)</item>
    /// <item><c>429</c> — Too Many Requests (rate limit hit — most common transient error)</item>
    /// <item><c>500–504</c> — Server-side errors (temporary — retry recommended)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The <see cref="ErrorContent"/> property often contains structured JSON with more details, 
    /// such as:
    /// <list type="bullet">
    /// <item><c>{"error": {"message": "Rate limit exceeded", "type": "requests_per_minute", "code": "rate_limit_exceeded"}}</c></item>
    /// <item><c>{"error": {"message": "Invalid model: grok-beta", "type": "invalid_request_error"}}</c></item>
    /// </list>
    /// You can parse <see cref="ErrorContent"/> manually or with <c>JsonSerializer</c> for richer error handling.
    /// </para>
    /// <para>
    /// Recommended handling pattern:
    /// <list type="number">
    /// <item>Catch <see cref="GrokApiException"/></item>
    /// <item>Log <see cref="StatusCode"/> and <see cref="ErrorContent"/></item>
    /// <item>Implement retry logic for 429 and 5xx errors (exponential backoff)</item>
    /// <item>Show user-friendly messages for common codes (e.g., "API rate limit reached — try again soon")</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// try
    /// {
    ///     var response = await client.ChatAsync(request);
    ///     // ... handle success
    /// }
    /// catch (GrokApiException ex)
    /// {
    ///     Console.WriteLine($"Grok API error {ex.StatusCode}: {ex.Message}");
    ///
    ///     if (ex.StatusCode == 429)
    ///     {
    ///         Console.WriteLine("Rate limit hit. Waiting before retry...");
    ///         // await Task.Delay(...); retry
    ///     }
    ///     else if (ex.StatusCode >= 500)
    ///     {
    ///         Console.WriteLine("Server-side issue — retrying later may help.");
    ///     }
    ///
    ///     if (!string.IsNullOrEmpty(ex.ErrorContent))
    ///     {
    ///         Console.WriteLine($"Raw error details: {ex.ErrorContent}");
    ///         // Optionally parse JSON for structured fields
    ///     }
    /// }
    /// </code>
    /// </example>
    public class GrokApiException : Exception
    {
        /// <summary>
        /// The HTTP status code returned by the Grok API.
        /// </summary>
        /// <remarks>
        /// Always set and reliable.  
        /// Use this value first when deciding how to handle the error (retry, user message, abort, etc.).
        /// </remarks>
        public int StatusCode { get; }

        /// <summary>
        /// The raw response body returned by the API (usually JSON containing error details).
        /// </summary>
        /// <remarks>
        /// <para>
        /// May be <see langword="null"/> or empty if:
        /// <list type="bullet">
        /// <item>The server returned no body (rare)</item>
        /// <item>Network-level failure before response body was read</item>
        /// </list>
        /// </para>
        /// <para>
        /// When present, typically contains structured JSON with <c>error</c> object:
        /// <c>{ "error": { "message": "...", "type": "...", "code": "..." } }</c>
        /// </para>
        /// <para>
        /// Recommended: log this value in full for debugging.
        /// </para>
        /// </remarks>
        public string? ErrorContent { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrokApiException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code from the API response.</param>
        /// <param name="errorContent">The raw error response body (may be null or empty).</param>
        /// <param name="message">A descriptive error message (usually includes status and summary).</param>
        public GrokApiException(int statusCode, string? errorContent, string message)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorContent = errorContent;
        }

        /// <summary>
        /// Initializes a new instance with inner exception information.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="errorContent">The raw error body.</param>
        /// <param name="message">Descriptive message.</param>
        /// <param name="innerException">The exception that caused this one (e.g., network error).</param>
        public GrokApiException(int statusCode, string? errorContent, string message, Exception? innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorContent = errorContent;
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Convenience Helpers
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets whether this error is likely transient and retryable (429 or 5xx).
        /// </summary>
        [JsonIgnore]
        public bool IsRetryable =>
            StatusCode == 429 ||
            (StatusCode >= 500 && StatusCode <= 599);

        /// <summary>
        /// Gets a short, user-friendly error category string.
        /// </summary>
        [JsonIgnore]
        public string ErrorCategory => StatusCode switch
        {
            400 => "Invalid Request",
            401 => "Authentication Failed",
            403 => "Forbidden / Quota Exceeded",
            429 => "Rate Limit Exceeded",
            >= 500 and <= 599 => "Server Error",
            _ => "Unexpected Error"
        };
    }
}