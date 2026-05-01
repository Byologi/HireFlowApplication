using HireFlow.DTOs;
using HireFlow.Services.Applications;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/applications")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // ✅ APPLY (correct route)
        [HttpPost("/api/jobs/{jobId}/applications")]
        public async Task<IActionResult> ApplyToJob(int jobId, CreateApplicationDto dto)
        {
            var result = await _applicationService.ApplyAsync(jobId, dto);
            return Ok(result);
        }

        // ✅ LIST APPLICATIONS FOR JOB
        [HttpGet("/api/jobs/{jobId}/applications")]
        public async Task<IActionResult> GetApplicationsForJob(
            int jobId,
            [FromQuery] string? stage)
        {
            var result = await _applicationService.GetByJobAsync(jobId, stage);
            return Ok(result);
        }

        // ✅ GET SINGLE APPLICATION
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _applicationService.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ✅ UPDATE STAGE
        [HttpPatch("{id}/stage")]
        public async Task<IActionResult> UpdateStage(int id, UpdateStageDto dto)
        {
            if (!Request.Headers.TryGetValue("X-Team-Member-Id", out var headerValue))
                return BadRequest("Missing X-Team-Member-Id header");

            if (!int.TryParse(headerValue, out var teamMemberId))
                return BadRequest("Invalid X-Team-Member-Id");

            var result = await _applicationService.UpdateStageAsync(id, dto, teamMemberId);

            return Ok(result);
        }
        
        [HttpPost("{id}/notes")]
        public async Task<IActionResult> AddNote(int id, CreateNoteDto dto)
        {
            if (!Request.Headers.TryGetValue("X-Team-Member-Id", out var header))
                return BadRequest("Missing X-Team-Member-Id");

            var teamMemberId = int.Parse(header);

            var result = await _applicationService.AddNoteAsync(id, dto, teamMemberId);

            return Ok(result);
        }
        
        [HttpGet("{id}/notes")]
        public async Task<IActionResult> GetNotes(int id)
        {
            var result = await _applicationService.GetNotesAsync(id);
            return Ok(result);
        }
        
        [HttpPut("{id}/scores/culture-fit")]
        public async Task<IActionResult> CultureFit(int id, ScoreUpdateDto dto)
        {
            var memberId = int.Parse(Request.Headers["X-Team-Member-Id"]);

            await _applicationService.UpdateCultureFitScoreAsync(id, dto, memberId);

            return Ok("Culture fit score updated");
        }
        
        [HttpPut("{id}/scores/interview")]
        public async Task<IActionResult> Interview(int id, ScoreUpdateDto dto)
        {
            var memberId = int.Parse(Request.Headers["X-Team-Member-Id"]);

            await _applicationService.UpdateInterviewScoreAsync(id, dto, memberId);

            return Ok("Interview score updated");
        }
        
        [HttpPut("{id}/scores/assessment")]
        public async Task<IActionResult> Assessment(int id, ScoreUpdateDto dto)
        {
            var memberId = int.Parse(Request.Headers["X-Team-Member-Id"]);

            await _applicationService.UpdateAssessmentScoreAsync(id, dto, memberId);

            return Ok("Assessment score updated");
        }
    }
}