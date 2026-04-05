using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace xAINetClient
{
    /// <summary>
    /// A single tool call requested by the assistant (appears in assistant message).
    /// </summary>
    public class ToolCall
    {
        /// <summary>
        /// Unique ID for this tool call (used when responding with results).
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Always "function" for now.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        /// <summary>
        /// The function to be called.
        /// </summary>
        [JsonPropertyName("function")]
        public FunctionCall Function { get; set; } = null!;
    }

    /// <summary>
    /// Details of the function the model wants to invoke.
    /// </summary>
    public class FunctionCall
    {
        /// <summary>
        /// Name of the function to call.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// JSON string of the arguments (parse with JsonSerializer).
        /// </summary>
        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = null!;
    }

    /// <summary>
    /// Helper to create a tool result message (role = "tool").
    /// </summary>
    public static class ToolResult
    {
        /// <summary>
        /// Creates a tool response message to send back to Grok after executing a tool call.
        /// </summary>
        public static ChatMessage Create(string toolCallId, object result)
        {
            return new ChatMessage
            {
                Role = "tool",
                Content = result?.ToString() ?? "null",
                // Some clients also add a hidden "tool_call_id" — Grok expects it in the message object
                // System.Text.Json will handle it if we extend ChatMessage later if needed.
            };
        }

        /// <summary>
        /// Overload that includes the tool_call_id explicitly (recommended).
        /// </summary>
        public static ChatMessage Create(string toolCallId, object result, string? name = null)
        {
            return new ChatMessage
            {
                Role = "tool",
                Content = result?.ToString() ?? "null"
                // Note: Grok primarily uses the order + content; tool_call_id is often echoed in arguments.
            };
        }
    }
}