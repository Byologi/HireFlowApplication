using HireFlow.Domain.Enums;

public class ApplicationNote
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public Application Application { get; set; }

    public NoteType Type { get; set; }   // MUST BE ENUM

    public string Description { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}