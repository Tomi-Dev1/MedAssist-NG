using MedAssistNG.Api.Mcp;
using MedAssistNG.Api.Models;
using Google.GenAI;
using Google.Cloud.Storage.V1;
using System.Text.Json;

namespace MedAssistNG.Api.Services
{
    public class GeminiService
    {
        private readonly IConfiguration _config;
        private readonly ClinicalKnowledgeTool _knowledge;
        private readonly Client _client;
        private readonly StorageClient? _storageClient;
        private readonly string _modelId = "gemini-3-flash-preview";


        public GeminiService(IConfiguration config, ClinicalKnowledgeTool knowledge)
        {
            _config = config;
            _knowledge = knowledge;

            var apiKey = _config["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("GeminiApiKey is missing from configuration.");
            }

            // 1. Initialize Official Google AI SDK Client (Works with API Key)
            _client = new Client(apiKey: apiKey);

            // 2. Initialize StorageClient (Google Cloud Storage) for compliance
            try {
                var credentialsPath = _config["GoogleCloudCredentialsPath"];
                var storageBuilder = new StorageClientBuilder();
                if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
                {
                    storageBuilder.CredentialsPath = credentialsPath;
                }
                _storageClient = storageBuilder.Build();
            } catch {
                _storageClient = null; // Fallback for local dev
            }
        }

        public async Task<TriageResult> AnalyzeAsync(string symptoms, string? imageBase64, string? imageContentType)
        {
            var clinicalContext = _knowledge.GetCommonCauses(symptoms);
            var prompt = $@"
You are a global clinical triage assistant.
While you can assist patients worldwide, you are currently optimized for primary healthcare in West African contexts (e.g., Nigeria), providing expert reasoning on regional infectious disease patterns.
Using state-of-the-art Gemini 3 multimodal reasoning for high-accuracy triage.

Patient symptoms:
{symptoms}

Additional context from MCP Tool [get_clinical_knowledge]:
{clinicalContext}

Return ONLY valid JSON in this exact structure:

{{
  ""urgency"": ""RED | YELLOW | GREEN"",
  ""clinicalReasoning"": ""Short clinical explanation"",
  ""recommendedAction"": ""Next step recommendation"",
  ""dangerSigns"": [""List"", ""Of"", ""Danger"", ""Signs""],
  ""probableConditions"": [""Possible"", ""Conditions""],
  ""followUpQuestion"": ""If symptoms are vague or more info is needed, ask ONE short clinical follow-up question. Otherwise return null.""
}}

If an image is provided, analyze it carefully. 
Incorporate image findings into urgency classification.
Do not include markdown.
Do not include explanation outside JSON.
";

            // 1. Log image (Compliance & Demo)
            if (!string.IsNullOrWhiteSpace(imageBase64))
            {
                var objectName = $"triage-{DateTime.UtcNow:yyyyMMdd-HHmmss}.jpg";
                var imageBytes = Convert.FromBase64String(imageBase64);

                // Target A: Google Cloud Storage (Compliance)
                if (_storageClient != null)
                {
                    try {
                        var bucketName = _config["GoogleStorageBucket"] ?? "medassist-ng-triage-images";
                        using var stream = new MemoryStream(imageBytes);
                        await _storageClient.UploadObjectAsync(bucketName, objectName, imageContentType, stream);
                    } catch {
                        // Fail silently or fallback to local
                    }
                }

                // Target B: Local Storage (Demo/Video Backup)
                try {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, objectName);
                    await File.WriteAllBytesAsync(filePath, imageBytes);
                } catch {
                    // Local backup failed, but AI should still proceed
                }
            }

            // 2. Prepare SDK Request using strongly-typed initialization
            var partsList = new List<Google.GenAI.Types.Part>
            {
                new Google.GenAI.Types.Part { Text = prompt }
            };

            if (!string.IsNullOrWhiteSpace(imageBase64) && !string.IsNullOrWhiteSpace(imageContentType))
            {
                try {
                    partsList.Add(new Google.GenAI.Types.Part {
                        InlineData = new Google.GenAI.Types.Blob {
                            MimeType = imageContentType,
                            Data = Convert.FromBase64String(imageBase64)
                        }
                    });
                    Console.WriteLine("[GeminiService] Multimodal part attached successfully.");
                } catch (Exception ex) {
                    Console.WriteLine("[GeminiService] Failed to attach image part: " + ex.Message);
                }
            }

            var contents = new List<Google.GenAI.Types.Content>
            {
                new Google.GenAI.Types.Content {
                    Role = "user",
                    Parts = partsList
                }
            };

            // 3. Call Gemini 3 via SDK
            var response = await _client.Models.GenerateContentAsync(_modelId, contents);

            // Accessing the text result safely using dynamic to bypass alpha SDK property name checks
            string? text = null;
            try {
                text = ((dynamic)response).Text; 
            } catch {
                try {
                    // Fallback to manual navigation if .Text property fails
                    var dynamicResponse = (dynamic)response;
                    text = dynamicResponse.Candidates?[0]?.Content?.Parts?[0]?.Text;
                } catch {
                    try {
                        // Try lowercase fallback
                        text = ((dynamic)response).candidates?[0]?.content?.parts?[0]?.text;
                    } catch {
                        text = response?.ToString();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return new TriageResult { ClinicalReasoning = "AI failed to generate a response." };
            }

            // Normalize JSON output
            text = text.Replace("```json", "").Replace("```", "").Trim();

            try {
                var triage = JsonSerializer.Deserialize<TriageResult>(
                    text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                return triage ?? new TriageResult { ClinicalReasoning = "AI failed to generate a result." };
            }
            catch (JsonException) {
                return new TriageResult { ClinicalReasoning = "Invalid AI output: " + text };
            }
        }
    }
}