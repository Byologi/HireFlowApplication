using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.DTOs;
using HireFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Services.Jobs
{
    public class JobService : IJobService
    {
        private readonly HireFlowDbContext _context;

        public JobService(HireFlowDbContext context)
        {
            _context = context;
        }

        public async Task<JobResponseDto> CreateJobAsync(CreateJobDto dto)
        {
            var job = new Job
            {
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                Status = Enum.Parse<JobStatus>(dto.Status, true)
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return Map(job);
        }

        public async Task<List<JobResponseDto>> GetJobsAsync(string? status)
        {
            var query = _context.Jobs.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(j => j.Status.ToString() == status);
            }

            var jobs = await query.ToListAsync();

            return jobs.Select(Map).ToList();
        }

        public async Task<JobResponseDto?> GetJobByIdAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return null;

            return Map(job);
        }

        private static JobResponseDto Map(Job job)
        {
            return new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                Status = job.Status.ToString()
            };
        }
    }
}