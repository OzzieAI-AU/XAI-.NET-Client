using System.Text.Json.Serialization;

namespace xAINetClient
{
    /// <summary>
    /// Represents a tool (function) that can be called by Grok.
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// Must be "function" (only type currently supported by Grok).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        /// <summary>
        /// The function definition.
        /// </summary>
        [JsonPropertyName("function")]
        public FunctionDefinition Function { get; set; } = null!;
    }

    /// <summary>
    /// Defines a single callable function/tool for the Grok model.
    /// </summary>
    public class FunctionDefinition
    {
        /// <summary>
        /// Name of the function (must be unique, alphanumeric + underscores).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Optional description (highly recommended — helps the model understand when to call it).
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// JSON Schema describing the parameters the function accepts.
        /// Use <see cref="ParameterSchema"/> helper or write raw schema.
        /// </summary>
        [JsonPropertyName("parameters")]
        public object? Parameters { get; set; }

        /// <summary>
        /// Optional strict mode (Grok respects this for better adherence).
        /// </summary>
        [JsonPropertyName("strict")]
        public bool? Strict { get; set; }
    }
}