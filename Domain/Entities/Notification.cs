namespace HireFlow.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Type { get; set; } = null!; // Hired / Rejected
        public DateTime SentAt { get; set; }
    }
}