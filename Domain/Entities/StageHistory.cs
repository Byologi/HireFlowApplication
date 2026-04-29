using HireFlow.Domain.Enums;

public class StageHistory
{
    public int Id { get; set; }
    
    public int ApplicationId { get; set; }

    public Application Application { get; set; }

    public ApplicationStage FromStage { get; set; }
    public ApplicationStage ToStage { get; set; }

    public int ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    public string? Comment { get; set; }
}