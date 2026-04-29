public class ApplicationDetailDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string CandidateName { get; set; }
    public string CandidateEmail { get; set; }
    public string Stage { get; set; }

    public List<ApplicationNoteDto> Notes { get; set; }
    public List<StageHistoryDto> StageHistories { get; set; }
}