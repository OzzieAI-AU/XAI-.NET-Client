namespace xAINetClient
{
    using System.Text.Json.Serialization;


    /// <summary>
    /// Represents a request to the xAI Grok Chat Completions API.
    /// Fully compatible with the official xAI API (OpenAI-style endpoint).
    /// Supports text, vision (images), tool/function calling, and streaming.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// The model to use for the chat completion.
        /// Default: <see cref="GrokModel.Grok4Fast"/> (a fast, cost-efficient Grok-4 variant).
        /// </summary>
        /// <remarks>
        /// Common values (as of 2026):
        /// - grok-4.20-reasoning
        /// - grok-4-fast-reasoning / grok-4-fast-non-reasoning
        /// - grok-4-1-fast-reasoning / grok-4-1-fast-non-reasoning
        /// Check <see href="https://docs.x.ai/developers/models">xAI Models</see> for the latest list.
        /// </remarks>
        [JsonPropertyName("model")]
        public string Model { get; set; } = GrokModel.Grok4Fast;

        /// <summary>
        /// A list of messages comprising the conversation so far.
        /// </summary>
        /// <remarks>
        /// Each message should be a <see cref="ChatMessage"/> with a <c>role</c> 
        /// ("system", "user", "assistant", or "tool") and <c>content</c> 
        /// (which can be a string or array for multimodal input including images).
        /// </remarks>
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();

        /// <summary>
        /// What sampling temperature to use, between 0 and 2.
        /// Higher values make the output more random; lower values make it more focused and deterministic.
        /// </summary>
        /// <remarks>Default is usually 1.0 if not specified.</remarks>
        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        /// <summary>
        /// The maximum number of tokens to generate in the chat completion.
        /// </summary>
        /// <remarks>
        /// This includes both reasoning (if the model supports it) and visible output tokens.
        /// </remarks>
        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        /// <summary>
        /// An alternative to sampling with temperature, called nucleus sampling.
        /// The model considers the results of the tokens with <c>top_p</c> probability mass.
        /// </summary>
        /// <remarks>
        /// Recommended to use either <c>Temperature</c> or <c>TopP</c>, but not both.
        /// Range: 0.0 – 1.0 (default is usually 1.0).
        /// </remarks>
        [JsonPropertyName("top_p")]
        public float? TopP { get; set; }

        /// <summary>
        /// If set to <c>true</c>, partial message deltas will be sent as they are generated.
        /// Use this for real-time streaming responses.
        /// </summary>
        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }

        /// <summary>
        /// A list of tools (functions) the model may call.
        /// </summary>
        /// <remarks>
        /// Each tool should follow the standard OpenAI function-calling schema:
        /// <c>{ "type": "function", "function": { "name": "...", "description": "...", "parameters": { ... } } }</c>
        /// xAI supports both custom functions and built-in tools (e.g., web_search, x_search, code_interpreter).
        /// </remarks>
        [JsonPropertyName("tools")]
        public List<Tool>? Tools { get; set; }

        /// <summary>
        /// Controls which (if any) tool is called by the model.
        /// </summary>
        /// <remarks>
        /// Supported values:
        /// <list type="bullet">
        ///   <item><c>"auto"</c> (default) — the model decides whether and which tool to call.</item>
        ///   <item><c>"none"</c> — the model will not call any tool and will generate a normal message.</item>
        ///   <item><c>"required"</c> — the model must call at least one tool.</item>
        ///   <item>An object of the form <c>{ "type": "function", "function": { "name": "specific_tool_name" } }</c> — forces the model to call the specified tool.</item>
        /// </list>
        /// </remarks>
        [JsonPropertyName("tool_choice")]
        public object? ToolChoice { get; set; }

        // Optional advanced parameters (commonly supported by xAI/OpenAI-compatible APIs)

        /// <summary>
        /// Number of chat completion choices to generate for each input message.
        /// </summary>
        [JsonPropertyName("n")]
        public int? N { get; set; } = 1;

        /// <summary>
        /// Up to 4 sequences where the API will stop generating further tokens.
        /// </summary>
        [JsonPropertyName("stop")]
        public List<string>? Stop { get; set; }

        /// <summary>
        /// Whether to return log probabilities of the output tokens.
        /// </summary>
        [JsonPropertyName("logprobs")]
        public bool? Logprobs { get; set; }

        /// <summary>
        /// An integer between 0 and 20 specifying the number of most likely tokens to return at each token position.
        /// Requires <see cref="Logprobs"/> to be <c>true</c>.
        /// </summary>
        [JsonPropertyName("top_logprobs")]
        public int? TopLogprobs { get; set; }

        /// <summary>
        /// A unique identifier representing your end-user, for monitoring and abuse prevention.
        /// </summary>
        [JsonPropertyName("user")]
        public string? User { get; set; }
    }
}