namespace MedAssistNG.Api.Models
{
    public class TriageResult
    {
        public string? Urgency { get; set; }
        public string? ClinicalReasoning { get; set; }
        public string? RecommendedAction { get; set; }
        public List<string> DangerSigns { get; set; } = new();
        public List<string> ProbableConditions { get; set; } = new();
        public string? FollowUpQuestion { get; set; }
    }
}
