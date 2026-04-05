namespace OzzieAI.XAI
{

    /// <summary>
    /// Static class containing known Grok model identifiers.
    /// </summary>
    public static class GrokModel
    {

        /// <summary>
        /// Current default / recommended Grok model (beta as of late 2025).
        /// </summary>
        public static string Grok4Fast { get; set; } = "grok-4-1-fast-reasoning";   // good balance
        public static string Grok4 { get; set; } = "grok-4.20-reasoning";      // flagship
        public static string GrokVision { get; set; } = "grok-4-vision-latest";     // or grok-2-vision-latest, depending on availability
        public static string Grok420Reasoning { get; set; } = "grok-4.20-0309-reasoning";     // flagship
        public static string Grok420NonReasoning { get; set; } = "grok-4.20-0309-non-reasoning";
        public static string Grok41FastReasoning { get; set; } = "grok-4-1-fast-reasoning";      // cost-effective
        public static string GrokImagineImage { get; set; } = "grok-imagine-image";
        public static string GrokImagineImagePro { get; set; } = "grok-imagine-image-pro";       // premium
        public static string GrokImagineVideo { get; set; } = "grok-imagine-video";

        public static string CurrentModel { get; set; } = Grok4Fast;

        // Optional aliases for backward compat
        [Obsolete("Use Grok420Reasoning instead")]
        public static string GrokBeta => Grok420Reasoning;

    }
}