namespace MedAssistNG.Client.Models
{
    public class ImagePayload
    {
        public string Base64 { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}