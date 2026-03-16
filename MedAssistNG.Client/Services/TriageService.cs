using System.Net.Http.Json;

namespace MedAssistNG.Client.Services
{
    public class TriageService
    {
        private readonly HttpClient _http;

        public TriageService(HttpClient http)
        {
            _http = http;
        }

        public async Task<TriageResponse?> AnalyzeAsync()
        {
            var response = await _http.PostAsJsonAsync("api/triage", new { });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TriageResponse>();
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine(error);
            return null;
        }
    }

    public class TriageResponse
    {
        public string? Urgency { get; set; }
        public string? ProbableCondition { get; set; }
        public string? clinicalReasoning { get; set; }
        public string? RecommendedAction { get; set; }
        public string[]? DangerSigns { get; set; }
    }
}
