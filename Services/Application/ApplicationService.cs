using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.DTOs;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Services.Applications
{
    public class ApplicationService : IApplicationService
    {
        private readonly HireFlowDbContext _context;
        
        private bool IsValidTransition(ApplicationStage from, ApplicationStage to)
        {
            return (from, to) switch
            {
                (ApplicationStage.Applied, ApplicationStage.Screening) => true,
                (ApplicationStage.Screening, ApplicationStage.Rejected) => true,
                _ => false
            };
        }

        public ApplicationService(HireFlowDbContext context)
        {
            _context = context;
        }

        // 🔥 1. APPLY TO JOB
        public async Task<ApplicationResponseDto> ApplyAsync(int jobId, CreateApplicationDto dto)
        {
            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);

            if (!jobExists)
                throw new Exception("Job not found");

            // duplicate check
            var exists = await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.CandidateEmail == dto.CandidateEmail);

            if (exists)
                throw new Exception("You already applied for this job");

            var application = new Application
            {
                JobId = jobId,
                CandidateName = dto.CandidateName,
                CandidateEmail = dto.CandidateEmail,
                CurrentStage = ApplicationStage.Applied
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Map(application);
        }

       

        public async Task<List<ApplicationResponseDto>> GetByJobAsync(int jobId, string? stage)
        {
            var query = _context.Applications
                .Where(a => a.JobId == jobId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(stage))
            {
                if (Enum.TryParse<ApplicationStage>(stage, true, out var parsedStage))
                {
                    query = query.Where(a => a.CurrentStage == parsedStage);
                }
                else
                {
                    throw new Exception("Invalid stage filter");
                }
            }

            var applications = await query
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            return applications.Select(Map).ToList();
        }

        // 🔥 3. GET SINGLE APPLICATION
        public async Task<ApplicationDetailDto?> GetByIdAsync(int id)
        {
            var application = await _context.Applications
                .Include(a => a.Notes)
                .Include(a => a.StageHistories)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return null;

            return new ApplicationDetailDto
            {
                Id = application.Id,
                JobId = application.JobId,
                CandidateName = application.CandidateName,
                CandidateEmail = application.CandidateEmail,
                Stage = application.CurrentStage.ToString(),

                Notes = application.Notes.Select(n => new ApplicationNoteDto
                {
                    Type = n.Type.ToString(),
                    Description = n.Description,
                    CreatedByName = $"User {n.CreatedBy}", // temporary (we fix later with TeamMember join)
                    CreatedAt = n.CreatedAt
                }).ToList(),

                StageHistories = application.StageHistories.Select(s => new StageHistoryDto
                {
                    FromStage = s.FromStage.ToString(),
                    ToStage = s.ToStage.ToString(),
                    ChangedByName = $"User {s.ChangedBy}", // temporary placeholder
                    ChangedAt = s.ChangedAt,
                    Comment = s.Comment
                }).ToList()
            };
        }

        public async Task<ApplicationResponseDto> UpdateStageAsync(int applicationId, UpdateStageDto dto, int teamMemberId)
        {
            var application = await _context.Applications.FindAsync(applicationId);

            if (application == null)
                throw new Exception("Application not found");

            // Parse new stage
            if (!Enum.TryParse<ApplicationStage>(dto.Stage, true, out var newStage))
                throw new Exception("Invalid stage");

            var currentStage = application.CurrentStage;

            // 🔥 VALIDATE TRANSITION
            if (!IsValidTransition(currentStage, newStage))
                throw new Exception($"Invalid transition from {currentStage} to {newStage}");

            // 🔥 SAVE HISTORY
            var history = new StageHistory
            {
                ApplicationId = application.Id,
                FromStage = currentStage,
                ToStage = newStage,
                ChangedBy = teamMemberId,
                ChangedAt = DateTime.UtcNow,
                Comment = dto.Comment
            };

            _context.StageHistories.Add(history);

            // 🔥 UPDATE APPLICATION
            application.CurrentStage = newStage;

            await _context.SaveChangesAsync();

            return new ApplicationResponseDto
            {
                Id = application.Id,
                JobId = application.JobId,
                CandidateName = application.CandidateName,
                CandidateEmail = application.CandidateEmail,
                Stage = application.CurrentStage.ToString()
            };
        }

        // 🔧 MAPPER
        private static ApplicationResponseDto Map(Application application)
        {
            return new ApplicationResponseDto
            {
                Id = application.Id,
                JobId = application.JobId,
                CandidateName = application.CandidateName,
                CandidateEmail = application.CandidateEmail,
                Stage = application.CurrentStage.ToString()
            };
        }
    }
}