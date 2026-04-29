public class StageHistoryDto
{
    public string FromStage { get; set; }
    public string ToStage { get; set; }
    public string ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }
}