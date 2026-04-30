using HireFlow.DTOs;

namespace HireFlow.Services.Applications
{
    public interface IApplicationService
    {
        Task<ApplicationResponseDto> ApplyAsync(int jobId, CreateApplicationDto dto);

        Task<List<ApplicationResponseDto>> GetByJobAsync(int jobId, string? stage);

        Task<ApplicationDetailDto?> GetByIdAsync(int id);
        
        Task<ApplicationResponseDto> UpdateStageAsync(int applicationId, UpdateStageDto dto, int teamMemberId);
        
        Task<NoteResponseDto> AddNoteAsync(int applicationId, CreateNoteDto dto, int teamMemberId);

        Task<List<NoteResponseDto>> GetNotesAsync(int applicationId);
        
        Task UpdateCultureFitScoreAsync(int applicationId, ScoreUpdateDto dto, int teamMemberId);

        Task UpdateInterviewScoreAsync(int applicationId, ScoreUpdateDto dto, int teamMemberId);

        Task UpdateAssessmentScoreAsync(int applicationId, ScoreUpdateDto dto, int teamMemberId);
        

    }
}