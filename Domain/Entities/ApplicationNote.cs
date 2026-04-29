public class ApplicationNote
{
    public int Id { get; set; }
    
    public int ApplicationId { get; set; }

    public Application Application { get; set; }

    public string Type { get; set; }
    public string Description { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}