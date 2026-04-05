using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzzieAI.XAI
{
    /// <summary>
    /// Configuration container for <see cref="GrokClient"/>.
    /// </summary>
    /// <remarks>
    /// All properties are optional except <see cref="ApiKey"/>.
    /// Unset values fall back to xAI API defaults or sensible client-side choices.
    /// </remarks>
    public class GrokClientConfig
    {
        /// <summary>
        /// REQUIRED: Your xAI API key (format: "xai-...").
        /// </summary>
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = "<YOUR API KEY GOES HERE>";

        /// <summary>
        /// Base URL of the xAI API endpoint.
        /// </summary>
        /// <value>Default: "https://api.x.ai/v1"</value>
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; } = "https://api.x.ai/v1";

        /// <summary>
        /// Request timeout in seconds.
        /// </summary>
        /// <value>Default: 100</value>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = (int)TimeSpan.FromMinutes(7).TotalSeconds;

        /// <summary>
        /// Default model to use when not specified in requests.
        /// </summary>
        [JsonPropertyName("model")]
        public string DefaultModel { get; set; } = "grok-code-fast-1";

        /// <summary>
        /// Default temperature setting (0.0–2.0).
        /// </summary>
        [JsonPropertyName("temperature")]
        public float DefaultTemperature { get; set; } = 0.7f;

        /// <summary>
        /// Default maximum tokens to generate.
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int? DefaultMaxTokens { get; set; }

        /// <summary>
        /// Default top_p (nucleus sampling) value.
        /// </summary>
        [JsonPropertyName("top_p")]
        public float? DefaultTopP { get; set; }

        /// <summary>
        /// Default number of completions to generate.
        /// </summary>
        [JsonPropertyName("n")]
        public int? DefaultN { get; set; } = 1;

        /// <summary>
        /// Default streaming behavior (when using streaming endpoints).
        /// </summary>
        [JsonPropertyName("stream")]
        public bool DefaultStream { get; set; } = false;

        /// <summary>
        /// Default frequency penalty to reduce repetition.
        /// </summary>
        [JsonPropertyName("frequency_penalty")]
        public float? DefaultFrequencyPenalty { get; set; }

        /// <summary>
        /// Default presence penalty to encourage new topics.
        /// </summary>
        [JsonPropertyName("presence_penalty")]
        public float? DefaultPresencePenalty { get; set; }

        /// <summary>
        /// Default stop sequences.
        /// </summary>
        [JsonPropertyName("stop")]
        public IList<string>? DefaultStopSequences { get; set; }

        /// <summary>
        /// Whether to store messages server-side for conversation continuation.
        /// </summary>
        [JsonPropertyName("store")]
        public bool? StoreMessages { get; set; } = true;

        /// <summary>
        /// Connect timeout in seconds (separate from overall timeout).
        /// </summary>
        [JsonPropertyName("connecttimeoutseconds")]
        public int ConnectTimeoutSeconds { get; set; } = (int)TimeSpan.FromMinutes(7).TotalSeconds;

        /// <summary>
        /// Maximum automatic retries for transient failures (429, 5xx, network).
        /// </summary>
        [JsonPropertyName("maxretries")]
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Base delay (seconds) for exponential backoff retries.
        /// </summary>
        [JsonPropertyName("retrybackoffbasedelayseconds")]
        public double RetryBackoffBaseDelaySeconds { get; set; } = 0.5;

        /// <summary>
        /// Optional custom headers added to every request.
        /// </summary>
        [JsonPropertyName("headers")]
        public IDictionary<string, string>? CustomHeaders { get; set; }

        /// <summary>
        /// Enables detailed request/response logging (debug only — do NOT use in production).
        /// </summary>
        [JsonPropertyName("debug")]
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Default CTOR:
        /// </summary>
        public GrokClientConfig()
        {
        }

        /// <summary>
        /// Creates a config from environment variables (useful for CI/CD or containerized apps).
        /// </summary>
        /// <returns>A new <see cref="GrokClientConfig"/> populated from env vars.</returns>
        /// <exception cref="InvalidOperationException">Thrown when XAI_API_KEY is missing.</exception>
        public static GrokClientConfig FromEnvironment()
        {
            return new GrokClientConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("XAI_API_KEY")
                    ?? throw new InvalidOperationException("XAI_API_KEY environment variable is required"),
                BaseUrl = Environment.GetEnvironmentVariable("XAI_BASE_URL"),
                DefaultModel = Environment.GetEnvironmentVariable("XAI_DEFAULT_MODEL"),
                TimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("XAI_TIMEOUT_SECONDS"), out var t) ? t : 100,
                // Add more environment variable mappings here as needed
            };
        }

        // ===================================================================
        // ADD THESE METHODS TO YOUR EXISTING GrokClientConfig CLASS
        // ===================================================================

        /// <summary>
        /// Full path to the JSON configuration file stored in the application's root directory
        /// (the folder containing your .exe or the BaseDirectory of the AppContext).
        /// File name: <c>grokconfig.json</c>
        /// </summary>
        private static readonly string ConfigFilePath = Path.Combine(
            AppContext.BaseDirectory, // Works on .NET 6+ (recommended). 
            "grokconfig.json");

        /// <summary>
        /// Saves the current configuration to <c>grokconfig.json</c> in the root directory
        /// with pretty-printed JSON for easy manual editing.
        /// </summary>
        /// <param name="customPath">Optional override. If null, uses the root directory file.</param>
        public void SaveToJson(string? customPath = null)
        {
            string path = customPath ?? ConfigFilePath;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,           // Human-readable
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Matches your [JsonPropertyName] style
            };

            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);

            Console.WriteLine($"✅ Configuration saved to: {path}");
        }

        /// <summary>
        /// Loads the configuration from <c>grokconfig.json</c> in the root directory.
        /// If the file does not exist, returns a fresh config with the default placeholder API key.
        /// </summary>
        /// <param name="customPath">Optional override. If null, uses the root directory file.</param>
        /// <returns>The deserialized config (never null).</returns>
        public static GrokClientConfig LoadFromJson(string? customPath = null)
        {

            string path = customPath ?? ConfigFilePath;

            if (!File.Exists(path))
            {
                return new GrokClientConfig();
            }

            string json = File.ReadAllText(path);

            try
            {
                // Add these options to handle the camelCase in your JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Deserialize<GrokClientConfig>(json, options) ?? new GrokClientConfig();
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error deserializing config: {exc.Message}");
                return new GrokClientConfig();
            }
        }

        /// <summary>
        /// MAIN ENTRY POINT – RECOMMENDED WAY TO GET YOUR CONFIG.
        /// 
        /// 1. Loads (or creates) grokconfig.json from the root directory.
        /// 2. Checks if ApiKey is still the placeholder "<YOUR API KEY GOES HERE>".
        /// 3. If it is, automatically opens the folder in the default file explorer,
        ///    tells the user exactly what to do, and waits for them to edit + save.
        /// 4. Reloads the file and repeats the check until a real key is present.
        /// 
        /// Usage:
        ///     var config = GrokClientConfig.EnsureApiKeyConfigured();
        ///     var client = new GrokClient(config);
        /// </summary>
        /// <returns>A fully configured GrokClientConfig with a valid API key.</returns>
        public static GrokClientConfig EnsureApiKeyConfigured()
        {

            string path = ConfigFilePath;
            GrokClientConfig config = LoadFromJson(path);

            // First-time setup: create the file with defaults
            if (!File.Exists(path))
            {
                config.SaveToJson(path);
            }

            // Keep prompting until the user replaces the placeholder
            while (string.IsNullOrWhiteSpace(config.ApiKey) || config.ApiKey.Trim() == "<YOUR API KEY GOES HERE>")
            {
                // Prompt for user action and open the folder:
                OpenDirectoryAndPromptUser(path);

                // Reload after user claims they saved it
                config = LoadFromJson(path);

                // 
                Thread.Sleep(3000); // Small delay to avoid tight loop if file is still locked or not yet saved
            }

            Console.WriteLine("✅ xAI API key loaded successfully.");
            return config;
        }

        // ===================================================================
        // PRIVATE HELPERS (you can keep them inside the class)
        // ===================================================================

        /// <summary>
        /// Opens the folder containing the config file and gives clear instructions.
        /// Works on Windows, macOS, and Linux.
        /// </summary>
        private static void OpenDirectoryAndPromptUser(string filePath)
        {

            string directory = Path.GetDirectoryName(filePath) ?? ".";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                xAI GROK CLIENT CONFIG SETUP                  ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"📁 Your config file is here: {filePath}");
            sb.AppendLine();
            sb.AppendLine("🔑 Please do the following right now:");
            sb.AppendLine("   1. Replace the line:");
            sb.AppendLine("      \"apiKey\": \"<YOUR API KEY GOES HERE>\"");
            sb.AppendLine("      with your real key (it starts with xai-...)");
            sb.AppendLine("   2. Save the file.");
            sb.AppendLine();

            // Display the popup
            //MessageBox.Show(sb.ToString(), "Config Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Open the folder automatically
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true
                });
                Console.WriteLine("🪟 The folder has been opened in your file explorer.");
            }
            catch
            {
                Console.WriteLine($"📂 Please open this folder manually: {directory}");
            }

            Console.WriteLine();
            Console.WriteLine("Press ENTER after you have edited and saved the file...");
            Console.ReadLine();
        }
    }
}