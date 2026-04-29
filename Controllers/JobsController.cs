using HireFlow.DTOs;
using HireFlow.Services;
using HireFlow.Services.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(CreateJobDto dto)
        {
            var result = await _jobService.CreateJobAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] string? status)
        {
            var result = await _jobService.GetJobsAsync(status);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var result = await _jobService.GetJobByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}