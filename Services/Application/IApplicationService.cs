using HireFlow.DTOs;

namespace HireFlow.Services.Applications
{
    public interface IApplicationService
    {
        Task<ApplicationResponseDto> ApplyAsync(int jobId, CreateApplicationDto dto);

        Task<List<ApplicationResponseDto>> GetByJobAsync(int jobId, string? stage);

        Task<ApplicationDetailDto?> GetByIdAsync(int id);
        
        Task<ApplicationResponseDto> UpdateStageAsync(int applicationId, UpdateStageDto dto, int teamMemberId);
    }
}