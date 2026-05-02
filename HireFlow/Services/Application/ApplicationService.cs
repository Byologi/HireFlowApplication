using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.DTOs;
using HireFlow.Infrastructure.Data;
using HireFlow.Services.BackgroundQueue;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Services.Applications
{
    public class ApplicationService : IApplicationService
    {
        private readonly HireFlowDbContext _context;
        private readonly IBackgroundTaskQueue _queue;

        public ApplicationService(HireFlowDbContext context, IBackgroundTaskQueue queue)
        {
            _context = context;
            _queue = queue;
        }

       
        // APPLY TO JOB
        public async Task<ApplicationResponseDto> ApplyAsync(int jobId, CreateApplicationDto dto)
        {
            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == jobId);

            if (!jobExists)
                throw new Exception("Job not found");

            var duplicate = await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.CandidateEmail == dto.CandidateEmail);

            if (duplicate)
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
        
        // GET BY JOB
        public async Task<List<ApplicationResponseDto>> GetByJobAsync(int jobId, string? stage)
        {
            var query = _context.Applications
                .Where(a => a.JobId == jobId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(stage))
            {
                if (Enum.TryParse<ApplicationStage>(stage, true, out var parsed))
                {
                    query = query.Where(a => a.CurrentStage == parsed);
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
        
        // GET FULL PROFILE
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
                    CreatedByName = $"User {n.CreatedBy}",
                    CreatedAt = n.CreatedAt
                }).ToList(),

                StageHistories = application.StageHistories.Select(s => new StageHistoryDto
                {
                    FromStage = s.FromStage.ToString(),
                    ToStage = s.ToStage.ToString(),
                    ChangedByName = $"User {s.ChangedBy}",
                    ChangedAt = s.ChangedAt,
                    Comment = s.Comment
                }).ToList()
            };
        }
        
        // UPDATE STAGE
        public async Task<ApplicationResponseDto> UpdateStageAsync(
            int applicationId,
            UpdateStageDto dto,
            int teamMemberId)
        {
            var application = await _context.Applications.FindAsync(applicationId);

            if (application == null)
                throw new Exception("Application not found");

            if (!Enum.TryParse<ApplicationStage>(dto.Stage, true, out var newStage))
                throw new Exception("Invalid stage");

            var current = application.CurrentStage;

            if (!IsValidTransition(current, newStage))
                throw new Exception($"Invalid transition {current} → {newStage}");

            var history = new StageHistory
            {
                ApplicationId = application.Id,
                FromStage = current,
                ToStage = newStage,
                ChangedBy = teamMemberId,
                ChangedAt = DateTime.UtcNow,
                Comment = dto.Comment
            };

            _context.StageHistories.Add(history);

            application.CurrentStage = newStage;

            await _context.SaveChangesAsync();

            // BACKGROUND NOTIFICATION TRIGGER
            if (newStage == ApplicationStage.Hired || newStage == ApplicationStage.Rejected)
            {
                var appId = application.Id;
                var email = application.CandidateEmail;
                var stage = newStage.ToString();

                _queue.QueueTask(async sp =>
                {
                    var db = sp.GetRequiredService<HireFlowDbContext>();

                    Console.WriteLine($"[NOTIFICATION EMAIL] Sent to {email} - Status: {stage}");

                    db.Notifications.Add(new Notification
                    {
                        ApplicationId = appId,
                        Type = stage,
                        SentAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();
                });
            }

            return Map(application);
        }
        
        // ADD NOTE
        public async Task<NoteResponseDto> AddNoteAsync(
            int applicationId,
            CreateNoteDto dto,
            int teamMemberId)
        {
            var application = await _context.Applications.FindAsync(applicationId);

            if (application == null)
                throw new Exception("Application not found");

            if (!Enum.TryParse<NoteType>(dto.Type, true, out var type))
                throw new Exception("Invalid note type");

            var note = new ApplicationNote
            {
                ApplicationId = applicationId,
                Type = type,
                Description = dto.Description,
                CreatedBy = teamMemberId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ApplicationNotes.Add(note);

            await _context.SaveChangesAsync();

            var member = await _context.TeamMembers.FindAsync(teamMemberId);

            return new NoteResponseDto
            {
                Type = note.Type.ToString(),
                Description = note.Description,
                CreatedByName = member?.Name ?? "Unknown",
                CreatedAt = note.CreatedAt
            };
        }
        
        // GET NOTES
        public async Task<List<NoteResponseDto>> GetNotesAsync(int applicationId)
        {
            var notes = await _context.ApplicationNotes
                .Where(n => n.ApplicationId == applicationId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var result = new List<NoteResponseDto>();

            foreach (var n in notes)
            {
                var member = await _context.TeamMembers.FindAsync(n.CreatedBy);

                result.Add(new NoteResponseDto
                {
                    Type = n.Type.ToString(),
                    Description = n.Description,
                    CreatedByName = member?.Name ?? "Unknown",
                    CreatedAt = n.CreatedAt
                });
            }

            return result;
        }
        
        // SCORE LOGIC (READY FOR NEXT STEP)
        private async Task UpsertScore(
            int applicationId,
            string type,
            ScoreUpdateDto dto,
            int teamMemberId)
        {
            if (dto.Score < 1 || dto.Score > 5)
                throw new Exception("Score must be between 1 and 5");

            var existing = await _context.ApplicationScores
                .FirstOrDefaultAsync(s => s.ApplicationId == applicationId && s.Type == type);

            if (existing == null)
            {
                existing = new ApplicationScore
                {
                    ApplicationId = applicationId,
                    Type = type
                };

                _context.ApplicationScores.Add(existing);
            }

            existing.Score = dto.Score;
            existing.Comment = dto.Comment;
            existing.UpdatedBy = teamMemberId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        
        // MAPPER
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
        
        public async Task UpdateCultureFitScoreAsync(int applicationId, ScoreUpdateDto dto, int teamMemberId)
        {
            await UpsertScore(applicationId, "culture-fit", dto, teamMemberId);
        }

        public async Task UpdateInterviewScoreAsync(int applicationId, ScoreUpdateDto dto, int teamMemberId)
        {
            await UpsertScore(applicationId, "interview", dto, teamMemberId);
        }

        public async Task UpdateAssessmentScoreAsync(int applicationId, ScoreUpdateDto dto, int teamMemberId)
        {
            await UpsertScore(applicationId, "assessment", dto, teamMemberId);
        }
        
        // VALIDATION
        private bool IsValidTransition(ApplicationStage from, ApplicationStage to)
        {
            return (from, to) switch
            {
                (ApplicationStage.Applied, ApplicationStage.Screening) => true,
                (ApplicationStage.Screening, ApplicationStage.Interview) => true,
                (ApplicationStage.Interview, ApplicationStage.Hired) => true,
                (ApplicationStage.Interview, ApplicationStage.Rejected) => true,
                (ApplicationStage.Screening, ApplicationStage.Rejected) => true,
                _ => false
            };
        }
        
    }
}