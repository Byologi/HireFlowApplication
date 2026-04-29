using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;

public class Application
{
    public int Id { get; set; }

    public int JobId { get; set; }
    public Job Job { get; set; }

    public string CandidateName { get; set; }
    public string CandidateEmail { get; set; }

    public ApplicationStage CurrentStage { get; set; }

    public ICollection<ApplicationNote> Notes { get; set; } = new List<ApplicationNote>();
    public ICollection<StageHistory> StageHistories { get; set; } = new List<StageHistory>();
}