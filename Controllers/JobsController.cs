using HireFlow.Infrastructure.Data;
using HireFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly HireFlowDbContext _context;

        public JobsController(HireFlowDbContext context)
        {
            _context = context;
        }

        // POST: create job
        [HttpPost]
        public async Task<IActionResult> CreateJob(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return Ok(job);
        }

        // GET: list jobs
        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] string? status)
        {
            var query = _context.Jobs.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(j => j.Status.ToString() == status);
            }

            var jobs = await query.ToListAsync();
            return Ok(jobs);
        }

        // GET: single job
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }
    }
}