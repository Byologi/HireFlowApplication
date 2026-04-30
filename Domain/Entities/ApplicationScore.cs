public class ApplicationScore
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public Application Application { get; set; }

    public string Type { get; set; } // culture-fit, interview, assessment

    public int Score { get; set; }   // 1–5

    public string Comment { get; set; }

    public int UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    public ICollection<ApplicationScore> Scores { get; set; } = new List<ApplicationScore>();
}