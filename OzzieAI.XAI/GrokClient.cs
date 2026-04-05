namespace OzzieAI.XAI
{


    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// High-level client for interacting with the xAI Grok API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides a clean, strongly-typed wrapper around the Grok chat completions endpoint.
    /// It handles authentication, request construction, error handling, JSON serialization, 
    /// and common convenience patterns.
    /// </para>
    /// <para>
    /// Key features:
    /// <list type="bullet">
    ///   <item>Automatic Bearer token authentication</item>
    ///   <item>Configurable base URL, timeout, default model, and retry behavior</item>
    ///   <item>Strongly-typed request/response models</item>
    ///   <item>Lenient JSON parsing (case-insensitive, skips comments)</item>
    ///   <item>Exception wrapping for API errors (<see cref="GrokApiException"/>)</item>
    ///   <item>Disposal of underlying <see cref="HttpClient"/></item>
    /// </list>
    /// </para>
    /// <para>
    /// Designed for easy integration into agent systems, console apps, or UI backends.
    /// Supports both full <see cref="ChatRequest"/> objects and quick single-message overloads.
    /// </para>
    /// <para>
    /// Thread-safe for concurrent use after construction (HttpClient is reused safely).
    /// </para>
    /// </remarks>
    public class GrokClient : IDisposable
    {

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private bool _disposed;

        /// <summary>
        /// Gets the active configuration used by this client instance.
        /// </summary>
        public GrokClientConfig GrokConfig { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrokClient"/> class.
        /// </summary>
        /// <param name="config">Full client configuration (API key is required).</param>
        /// <exception cref="ArgumentException">Thrown when API key is missing or invalid.</exception>
        public GrokClient(GrokClientConfig config)
        {

            if (string.IsNullOrWhiteSpace(config.ApiKey))
                throw new ArgumentException("API key is required", nameof(config.ApiKey));

            GrokConfig = config;

            // Apply default model override if provided
            if (!string.IsNullOrWhiteSpace(config.DefaultModel))
                GrokModel.CurrentModel = config.DefaultModel;

            _apiKey = config.ApiKey;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(config.BaseUrl.EndsWith("/") ? config.BaseUrl : config.BaseUrl + "/"),
                Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds > 0 ? config.TimeoutSeconds : 100)
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Convenience constructor using only required API key and optional overrides.
        /// </summary>
        /// <param name="apiKey">Your xAI API key.</param>
        /// <param name="baseUrl">Optional custom base URL (defaults to xAI production).</param>
        /// <param name="timeoutSeconds">Request timeout in seconds (default 100).</param>
        public GrokClient(string apiKey, string? baseUrl = null, int timeoutSeconds = 100) : this(new GrokClientConfig
        {
            ApiKey = apiKey,
            BaseUrl = baseUrl,
            TimeoutSeconds = timeoutSeconds
        })
        {
        }

        /// <summary>
        /// Use this example for video using Grok:
        /// </summary>
        public async void GrokVideoUsageExample()
        {
            try
            {
                var config = new GrokClientConfig();
                var client = new GrokClient(config);

                Console.WriteLine("Starting video generation...");
                var finalResult = await client.GenerateVideoAndWaitAsync(
                    prompt: "A majestic eagle soaring over snow-capped mountains at golden hour, cinematic drone shot, dramatic lighting",
                    durationSeconds: 8,
                    aspectRatio: "16:9"
                );

                if (finalResult.Status == "completed" && !string.IsNullOrEmpty(finalResult.Url))
                {
                    Console.WriteLine($"Video ready! Download from: {finalResult.Url}");
                    // You can download it here or display in UI
                }
                else if (finalResult.Status == "failed")
                {
                    Console.WriteLine($"Generation failed: {finalResult.Error ?? "Unknown error"}");
                }
            }
            catch (GrokApiException ex)
            {
                Console.WriteLine($"API error {ex.StatusCode}: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Sends a chat completion request to the Grok API and returns the full response.
        /// </summary>
        /// <param name="request">The complete chat request object (model, messages, parameters).</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>The deserialized <see cref="ChatCompletionResponse"/>.</returns>
        /// <exception cref="GrokApiException">Thrown on non-success HTTP status codes.</exception>
        /// <exception cref="InvalidOperationException">Thrown if deserialization returns null.</exception>
        public async Task<ChatCompletionResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new GrokApiException((int)response.StatusCode, errorContent,
                    $"Grok API error {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
                GrokJsonOptions.Default,
                cancellationToken);

            return result ?? throw new InvalidOperationException("Deserialization returned null");
        }

        /// <summary>
        /// Convenience overload: sends a single user message and gets a response.
        /// </summary>
        /// <param name="userMessage">The message/content to send to Grok.</param>
        /// <param name="model">Optional model override.</param>
        /// <param name="temperature">Optional temperature override.</param>
        /// <param name="maxTokens">Optional max tokens override.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The full chat completion response.</returns>
        public Task<ChatCompletionResponse> ChatAsync(string userMessage, string? model = null, float? temperature = null, int? maxTokens = null, CancellationToken ct = default)
        {
            var req = new ChatRequest
            {
                Model = model ?? GrokModel.GrokBeta,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "user", Content = userMessage }
                },
                Temperature = temperature,
                MaxTokens = maxTokens
            };

            return ChatAsync(req, ct);
        }

        /// <summary>
        /// Sends a chat request with a text prompt and a single image (vision/multimodal).
        /// </summary>
        /// <remarks>
        /// This is a convenient high-level method for vision tasks using Grok's multimodal capabilities.
        /// The image can be supplied as a publicly accessible URL or as a base64-encoded string.
        /// 
        /// Uses sensible defaults from <see cref="GrokConfig"/> and falls back gracefully 
        /// to <see cref="GrokModel.Grok4Fast"/> if no vision model is configured.
        /// </remarks>
        /// <param name="prompt">The text prompt to accompany the image.</param>
        /// <param name="imageUrlOrBase64">Either a public image URL or a base64-encoded image string (with or without data URI prefix).</param>
        /// <param name="model">Optional model override. Defaults to the configured vision model, then <see cref="GrokModel.Grok4Fast"/>.</param>
        /// <param name="detail">Image detail level: "low", "high", or "auto". Default is "high".</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>The <see cref="ChatCompletionResponse"/> from the xAI API.</returns>
        /// <exception cref="ArgumentNullException">Thrown if prompt or imageUrlOrBase64 is null.</exception>
        public Task<ChatCompletionResponse> ChatWithImageAsync(string prompt, string imageUrlOrBase64, string? model = null, string detail = "high", CancellationToken ct = default)
        {

            var msg = ChatMessage.UserWithImage(prompt, imageUrlOrBase64, detail);

            var req = new ChatRequest
            {
                Model = model ?? GrokModel.GrokVision ?? GrokConfig.DefaultModel ?? GrokModel.Grok4Fast,
                Messages = new List<ChatMessage> { msg },
                Temperature = GrokConfig.DefaultTemperature,
                MaxTokens = GrokConfig.DefaultMaxTokens
            };

            return ChatAsync(req, ct);
        }


        /// <summary>
        /// Returns a well-formatted, ready-to-use C# code example demonstrating 
        /// how to perform tool calling with the xAI Grok API using <see cref="ChatWithToolsAsync"/>.
        /// </summary>
        /// <remarks>
        /// This method is intended for documentation, README files, or interactive examples.
        /// It shows the recommended pattern for one-round tool calling and includes 
        /// a basic multi-turn loop structure for more complex scenarios.
        /// </remarks>
        /// <returns>A formatted C# code snippet as a string.</returns>
        public string ProvideToolCallingExample()
        {
            var sb = new StringBuilder();

            sb.AppendLine("var client = new GrokClient(config);");
            sb.AppendLine();
            sb.AppendLine("// 1. Define tools");
            sb.AppendLine("var tools = new List<Tool>");
            sb.AppendLine("{");
            sb.AppendLine("    new Tool");
            sb.AppendLine("    {");
            sb.AppendLine("        Function = new FunctionDefinition");
            sb.AppendLine("        {");
            sb.AppendLine("            Name = \"get_weather\",");
            sb.AppendLine("            Description = \"Get current weather for a city\",");
            sb.AppendLine("            Parameters = new");
            sb.AppendLine("            {");
            sb.AppendLine("                type = \"object\",");
            sb.AppendLine("                properties = new");
            sb.AppendLine("                {");
            sb.AppendLine("                    city = new { type = \"string\", description = \"City name\" },");
            sb.AppendLine("                    units = new { type = \"string\", enum = new[] { \"celsius\", \"fahrenheit\" } }");
            sb.AppendLine("                },");
            sb.AppendLine("                required = new[] { \"city\" }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("};");
            sb.AppendLine();
            sb.AppendLine("// 2. First call");
            sb.AppendLine("var response = await client.ChatWithToolsAsync(");
            sb.AppendLine("    \"What's the weather in Sydney right now?\",");
            sb.AppendLine("    tools);");
            sb.AppendLine();
            sb.AppendLine("// 3. Check if model wants to call tools");
            sb.AppendLine("var choice = response.Choices[0];");
            sb.AppendLine("if (choice.Message.Content is List<ContentPart> parts ||");
            sb.AppendLine("    choice.Message.Content == null) // sometimes content is null when tool_calls present");
            sb.AppendLine("{");
            sb.AppendLine("    // In practice, tool_calls are inside the assistant message");
            sb.AppendLine("    // You may need to extend ChatMessage to have a ToolCalls property for full support.");
            sb.AppendLine("    // For now, many implementations parse the raw JSON or add:");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("// Recommended pattern (simple loop):");
            sb.AppendLine("while (true)");
            sb.AppendLine("{");
            sb.AppendLine("    var resp = await client.ChatAsync(currentRequest);");
            sb.AppendLine("    var msg = resp.Choices[0].Message;");
            sb.AppendLine("    if (msg.Content != null)");
            sb.AppendLine("    {");
            sb.AppendLine("        Console.WriteLine(\"Final answer: \" + msg.Content);");
            sb.AppendLine("        break;");
            sb.AppendLine("    }");
            sb.AppendLine("    // Handle tool calls (you'll need to extract ToolCall list from message)");
            sb.AppendLine("    // Execute tools → add ToolResult messages to history");
            sb.AppendLine("    // Continue the conversation");
            sb.AppendLine("}");

            return sb.ToString();
        }


        /// <summary>
        /// Sends a chat request that includes tools and automatically handles one round of tool calling.
        /// </summary>
        /// <remarks>
        /// This method performs **one round** of tool calling.
        /// It calls the model once and returns the response (which may contain tool_calls).
        /// 
        /// For multi-turn tool use (when the model needs several steps of tool calling),
        /// call this method in a loop until <c>response.Choices[0].Message.ToolCalls</c> 
        /// is null or empty.
        /// </remarks>
        /// <param name="request">The chat request containing messages, tools, tool_choice, etc.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>The <see cref="ChatCompletionResponse"/> returned by the xAI API.</returns>
        /// <exception cref="ArgumentException">Thrown if the request does not contain any tools.</exception>
        public async Task<ChatCompletionResponse> ChatWithToolsAsync(ChatRequest request, CancellationToken ct = default)
        {

            if (request.Tools == null || request.Tools.Count == 0)
            {
                throw new ArgumentException(
                    "The request must contain at least one tool when using ChatWithToolsAsync.",
                    nameof(request));
            }

            return await ChatAsync(request, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// High-level convenience overload to quickly send a user message with tools.
        /// </summary>
        /// <remarks>
        /// This is a simple helper that creates a <see cref="ChatRequest"/> internally 
        /// and delegates to the main <see cref="ChatWithToolsAsync(ChatRequest, CancellationToken)"/> method.
        /// 
        /// For multi-turn tool conversations, call this in a loop or use the request-based overload.
        /// </remarks>
        /// <param name="userMessage">The user's input message.</param>
        /// <param name="tools">List of tools the model is allowed to call.</param>
        /// <param name="model">Optional model name. Defaults to <see cref="GrokModel.Grok4Fast"/>.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>The <see cref="ChatCompletionResponse"/> returned by the xAI API.</returns>
        public async Task<ChatCompletionResponse> ChatWithToolsAsync(string userMessage, List<Tool> tools, string? model = null, CancellationToken ct = default)
        {

            ArgumentNullException.ThrowIfNull(userMessage, nameof(userMessage));
            ArgumentNullException.ThrowIfNull(tools, nameof(tools));

            if (tools.Count == 0)
            {
                throw new ArgumentException("At least one tool must be provided.", nameof(tools));
            }

            var request = new ChatRequest
            {
                Model = model ?? GrokModel.Grok4Fast,
                Messages = { ChatMessage.User(userMessage) },
                Tools = tools,
                ToolChoice = "auto"
            };

            return await ChatWithToolsAsync(request, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads a file and sends a chat request that properly references it.
        /// The xAI API will automatically provide document search/context when relevant.
        /// </summary>
        public async Task<ChatCompletionResponse> ChatWithFileAsync(string prompt, string filePath, string? model = null, CancellationToken ct = default)
        {

            var file = await UploadFileAsync(filePath, purpose: "assistants", ct: ct);

            // Modern way: Create a user message that references the file ID
            // xAI backend handles context injection + file_search tool automatically
            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = new List<ContentPart>
                {
                    ContentPart.CreateText($"{prompt}\n\nPlease analyze the attached file: {file.FileName} (ID: {file.Id})")
                    // In the future xAI may support a native "file" content part; for now text reference + ID works well
                }
            };

            var req = new ChatRequest
            {
                Model = model ?? GrokConfig.DefaultModel ?? GrokModel.Grok4Fast,
                Messages = new List<ChatMessage> { userMessage },
                Tools = new List<Tool> // Optional: explicitly enable file search if you want
                {
                    new Tool { Function = new FunctionDefinition { Name = "file_search" } } // if needed
                },
                Temperature = GrokConfig.DefaultTemperature,
                MaxTokens = GrokConfig.DefaultMaxTokens
            };

            return await ChatAsync(req, ct);
        }

        /// <summary>
        /// Uploads a file to the xAI Grok Files API so it can be referenced in chat completions.
        /// Supported formats (as of March 2026): PDF, TXT, CSV, JSON, MD, DOCX, XLSX, and many more.
        /// </summary>
        /// <param name="filePath">Local path to the file to upload.</param>
        /// <param name="purpose">Purpose of the file. Use "assistants" for chat context (recommended).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><see cref="FileObject"/> containing the file ID and metadata.</returns>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="GrokApiException"/>
        public async Task<FileObject> UploadFileAsync(string filePath, string purpose = "assistants", CancellationToken ct = default)
        {

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            using var content = new MultipartFormDataContent();
            var fileBytes = await File.ReadAllBytesAsync(filePath, ct);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

            content.Add(fileContent, "file", Path.GetFileName(filePath));
            content.Add(new StringContent(purpose), "purpose");

            // Official xAI endpoint (March 2026)
            var response = await _httpClient.PostAsync("v1/files", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                throw new GrokApiException((int)response.StatusCode, err, $"File upload failed: {err}");
            }

            return await response.Content.ReadFromJsonAsync<FileObject>(GrokJsonOptions.Default, ct)
                ?? throw new InvalidOperationException("File upload response was null");
        }

        /// <summary>
        /// Generates one or more images from a text prompt using Grok Imagine.
        /// <code>
        /// var client = new GrokClient(config);
        /// var result = await client.GenerateImageAsync("A cyberpunk fox hacker in neon Tokyo rain, ultra detailed, cinematic");
        /// foreach (var img in result.Data)
        /// {
        /// Console.WriteLine($"Generated image: {img.Url}");
        /// // Download or display img.Url (temporary — cache if needed)
        /// }
        /// </code>
        /// </summary>
        /// <param name="prompt">Text description of the desired image(s).</param>
        /// <param name="n">Number of images to generate (1–10, default 1).</param>
        /// <param name="size">Resolution/aspect, e.g. "1024x1024", "1792x1024" (check docs for supported).</param>
        /// <param name="model">Optional model override (defaults to grok-imagine-image).</param>
        /// <param name="responseFormat">"url" (default, temporary link) or "b64_json".</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response with image URLs or base64 data.</returns>
        public async Task<ImageGenerationResponse> GenerateImageAsync(string prompt, int n = 1, string? size = "1024x1024", string? model = null, string responseFormat = "url", CancellationToken ct = default)
        {

            if (n < 1 || n > 10) throw new ArgumentOutOfRangeException(nameof(n), "Must be 1–10");

            var requestBody = new
            {
                model = model ?? GrokModel.GrokImagineImage,
                prompt,
                n,
                size,
                response_format = responseFormat
            };

            var response = await _httpClient.PostAsJsonAsync("images/generations", requestBody, ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                throw new GrokApiException((int)response.StatusCode, err, $"Image generation failed: {err}");
            }

            return await response.Content.ReadFromJsonAsync<ImageGenerationResponse>(GrokJsonOptions.Default, ct)
                ?? throw new InvalidOperationException("Image generation response was null");
        }

        /// <summary>
        /// Starts asynchronous video generation using Grok Imagine Video.
        /// Returns a job ID immediately; the video is generated in the background.
        /// Typical duration: 5–30 seconds of video, generation time 30s–several minutes.
        /// </summary>
        /// <param name="prompt">Detailed text description of the desired video scene/animation.</param>
        /// <param name="model">Optional model override. Defaults to grok-imagine-video.</param>
        /// <param name="durationSeconds">Desired video length in seconds (usually 5–15s supported).</param>
        /// <param name="aspectRatio">Optional aspect ratio, e.g. "16:9", "9:16", "1:1" (check docs for supported values).</param>
        /// <param name="resolution">Optional resolution preset, e.g. "720p", "1080p" (if supported).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>VideoGenerationStartResponse containing job ID and initial status.</returns>
        /// <exception cref="GrokApiException">Thrown on API errors (rate limit, invalid prompt, etc.).</exception>
        /// <exception cref="ArgumentOutOfRangeException">For invalid duration or parameters.</exception>
        public async Task<VideoGenerationStartResponse> StartVideoGenerationAsync(
                    string prompt,
                    string? model = null,
                    int? durationSeconds = 5,
                    string? aspectRatio = "16:9",
                    string? resolution = null,
                    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt is required", nameof(prompt));

            if (durationSeconds.HasValue && (durationSeconds < 3 || durationSeconds > 30))
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration should typically be 3–30 seconds");

            var requestBody = new
            {
                model = model ?? GrokModel.GrokImagineVideo,
                prompt = prompt.Trim(),
                duration = durationSeconds,
                aspect_ratio = aspectRatio,
                resolution,
                // You can add more fields later: style_preset, motion_intensity, seed, etc.
            };

            var response = await _httpClient.PostAsJsonAsync("video/generations", requestBody, GrokJsonOptions.Default, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new GrokApiException(
                    (int)response.StatusCode,
                    errorContent,
                    $"Video generation start failed ({response.StatusCode}): {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<VideoGenerationStartResponse>(
                GrokJsonOptions.Default,
                ct);

            return result ?? throw new InvalidOperationException("Video generation start response was null");
        }

        /// <summary>
        /// Polls the status of a running video generation job.
        /// Call this repeatedly (e.g. every 5–15 seconds) until status is "completed" or "failed".
        /// </summary>
        /// <param name="jobId">The job ID returned from StartVideoGenerationAsync.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Current status, progress, and final URL if completed.</returns>
        /// <exception cref="GrokApiException">Thrown on API errors.</exception>
        public async Task<VideoGenerationStatusResponse> GetVideoGenerationStatusAsync(string requestId, CancellationToken ct = default)
        {

            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("Request ID is required", nameof(requestId));

            var response = await _httpClient.GetAsync($"v1/videos/{requestId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new GrokApiException(
                    (int)response.StatusCode,
                    errorContent,
                    $"Failed to get video status ({response.StatusCode}): {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<VideoGenerationStatusResponse>(
                GrokJsonOptions.Default,
                ct);

            return result ?? throw new InvalidOperationException("Video status response was null");
        }

        /// <summary>
        /// Convenience method: Starts video generation and automatically polls until completion or failure.
        /// Blocks until the video is ready or an error occurs.
        /// </summary>
        /// <param name="pollIntervalSeconds">How often to check status (default 10s).</param>
        /// <param name="maxWaitMinutes">Maximum time to wait before timeout (default 10 min).</param>
        /// <returns>The final status response (with URL if successful).</returns>
        public async Task<VideoGenerationStatusResponse> GenerateVideoAndWaitAsync(
                    string prompt,
                    string? model = null,
                    int? durationSeconds = 5,
                    string? aspectRatio = "16:9",
                    string? resolution = null,
                    int pollIntervalSeconds = 10,
                    int maxWaitMinutes = 10,
                    CancellationToken ct = default)
        {
            var start = await StartVideoGenerationAsync(prompt, model, durationSeconds, aspectRatio, resolution, ct);

            if (start.Status == "completed")
                return new VideoGenerationStatusResponse { Id = start.Id, Status = "completed", Url = /* assume immediate url if sync */ null };

            string requestId = start.Id;   // assuming your VideoGenerationStartResponse has Id = request_id

            var startTime = DateTime.UtcNow;
            var maxDuration = TimeSpan.FromMinutes(maxWaitMinutes);

            while (!ct.IsCancellationRequested)
            {
                if (DateTime.UtcNow - startTime > maxDuration)
                    throw new TimeoutException($"Video timed out after {maxWaitMinutes} min (req {requestId})");

                var status = await GetVideoGenerationStatusAsync(requestId, ct);

                if (status.Status is "completed" or "failed" or "error")
                    return status;

                await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds), ct);
            }
            throw new OperationCanceledException("Polling canceled.");
        }


        /// <summary>
        /// Starts asynchronous video generation using Grok Imagine Video.
        /// Returns a request ID immediately; poll /v1/videos/{request_id} for status.
        /// Typical generation time: 30s–3min depending on load.
        /// Supports text-to-video and image-to-video (via sourceImageUrl).
        /// </summary>
        /// <param name="prompt">Required text description of the video scene.</param>
        /// <param name="model">Optional model override (defaults to grok-imagine-video).</param>
        /// <param name="durationSeconds">Video length in seconds (1–15; default 5).</param>
        /// <param name="aspectRatio">Aspect ratio e.g. "16:9", "9:16", "1:1" (default "16:9").</param>
        /// <param name="resolution">"720p" (default) or "480p".</param>
        /// <param name="sourceImageUrl">Optional image URL for image-to-video animation.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>VideoGenerationStartResponse with request_id.</returns>
        /// <exception cref="ArgumentException">Prompt missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Invalid duration.</exception>
        /// <exception cref="GrokApiException">API errors (rate limit, invalid params, etc.).</exception>
        public async Task<VideoGenerationStartResponse> StartVideoGenerationAsync(
                    string prompt,
                    string? model = null,
                    int? durationSeconds = 5,
                    string? aspectRatio = "16:9",
                    string? resolution = "720p",
                    string? sourceImageUrl = null,
                    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt is required", nameof(prompt));

            if (durationSeconds.HasValue && (durationSeconds < 1 || durationSeconds > 15))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationSeconds),
                    durationSeconds,
                    "Duration must be between 1 and 15 seconds (received {durationSeconds}).");
            }

            var requestBody = new
            {
                model = model ?? GrokModel.GrokImagineVideo,
                prompt = prompt.Trim(),
                duration = durationSeconds,
                aspect_ratio = aspectRatio,
                resolution,
                image_url = sourceImageUrl  // optional for image-to-video
                                            // audio: false  // uncomment if/when API supports disabling native audio
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v1/videos/generations",
                requestBody,
                GrokJsonOptions.Default,
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new GrokApiException(
                    (int)response.StatusCode,
                    errorContent,
                    $"Video generation start failed ({response.StatusCode}): {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<VideoGenerationStartResponse>(
                GrokJsonOptions.Default,
                ct)
                ?? throw new InvalidOperationException("Video start response deserialization returned null");

            return result;
        }


        // Add to GrokClient
        private static readonly JsonSerializerOptions FileOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Releases the underlying <see cref="HttpClient"/> resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}