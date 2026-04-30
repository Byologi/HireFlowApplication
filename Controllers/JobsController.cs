using HireFlow.DTOs;
using HireFlow.Services;
using HireFlow.Services.Applications;
using HireFlow.Services.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
            IApplicationService? applicationService;
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
        
        // ✅ APPLY (correct route)
        [HttpPost("{jobId}/applications")]
        public async Task<IActionResult> ApplyToJob(int jobId, CreateApplicationDto dto)
        {
            var result = await _applicationService.ApplyAsync(jobId, dto);
            return Ok(result);
        }

        // ✅ LIST APPLICATIONS FOR JOB
        [HttpGet("{jobId}/applications")]
        public async Task<IActionResult> GetApplicationsForJob(
            int jobId,
            [FromQuery] string? stage)
        {
            var result = await _applicationService.GetByJobAsync(jobId, stage);
            return Ok(result);
        }
    }
}