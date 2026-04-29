using HireFlow.DTOs;
using HireFlow.Services.Applications;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // ✅ APPLY (correct route)
        [HttpPost("api/jobs/{jobId}/applications")]
        public async Task<IActionResult> ApplyToJob(int jobId, CreateApplicationDto dto)
        {
            var result = await _applicationService.ApplyAsync(jobId, dto);
            return Ok(result);
        }

        // ✅ LIST APPLICATIONS FOR JOB
        [HttpGet("api/jobs/{jobId}/applications")]
        public async Task<IActionResult> GetApplicationsForJob(
            int jobId,
            [FromQuery] string? stage)
        {
            var result = await _applicationService.GetByJobAsync(jobId, stage);
            return Ok(result);
        }

        // ✅ GET SINGLE APPLICATION
        [HttpGet("api/applications/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _applicationService.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ✅ UPDATE STAGE
        [HttpPatch("api/applications/{id}/stage")]
        public async Task<IActionResult> UpdateStage(int id, UpdateStageDto dto)
        {
            if (!Request.Headers.TryGetValue("X-Team-Member-Id", out var headerValue))
                return BadRequest("Missing X-Team-Member-Id header");

            if (!int.TryParse(headerValue, out var teamMemberId))
                return BadRequest("Invalid X-Team-Member-Id");

            var result = await _applicationService.UpdateStageAsync(id, dto, teamMemberId);

            return Ok(result);
        }
    }
}