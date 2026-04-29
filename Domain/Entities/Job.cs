public class Job
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public JobStatus Status { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}