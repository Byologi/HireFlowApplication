using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities
{
    public class Application
    {
        public Guid Id { get; set; }

        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;

        public string CandidateName { get; set; } = null!;
        public string CandidateEmail { get; set; } = null!;
        public string? CoverLetter { get; set; }

        public ApplicationStage CurrentStage { get; set; } = ApplicationStage.Applied;

        // Scores (nullable until set)
        public int? CultureFitScore { get; set; }
        public int? InterviewScore { get; set; }
        public int? AssessmentScore { get; set; }
    }
}