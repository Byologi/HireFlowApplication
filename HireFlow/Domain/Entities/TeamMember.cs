using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public TeamMemberRole Role { get; set; }
    }
}