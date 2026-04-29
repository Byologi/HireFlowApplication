namespace HireFlow.DTOs
{
    public class ApplicationResponseDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string Stage { get; set; }
    }
}