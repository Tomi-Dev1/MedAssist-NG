namespace MedAssistNG.Api.Models
{
    public class SymptomRequest
    {
        public string Symptoms { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
        // Added content-type to allow server to pass correct mimeType to Gemini
        public string? ImageContentType { get; set; }
    }
}
