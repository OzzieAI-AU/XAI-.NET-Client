using System.Text.Json.Serialization;

namespace xAINetClient
{
    /// <summary>
    /// Response from image generation endpoint (/images/generations).
    /// </summary>
    public class ImageGenerationResponse
    {
        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("data")]
        public List<ImageData> Data { get; set; } = new();
    }
}