using HireFlow.Domain.Enums;

namespace HireFlow.Domain.Entities
{
    public class TeamMember
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public TeamMemberRole Role { get; set; }
    }
}