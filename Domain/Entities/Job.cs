namespace HireFlow.Domain.Entities
{
    public class Job
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Status { get; set; } = "open";

        public List<Application> Applications { get; set; } = new();
    }
}