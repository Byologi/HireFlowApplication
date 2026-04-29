using HireFlow.DTOs;

namespace HireFlow.Services
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateJobAsync(CreateJobDto dto);
        Task<List<JobResponseDto>> GetJobsAsync(string? status);
        Task<JobResponseDto?> GetJobByIdAsync(int id);
    }
}