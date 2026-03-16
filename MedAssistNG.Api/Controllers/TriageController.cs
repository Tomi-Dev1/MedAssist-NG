using Microsoft.AspNetCore.Mvc;
using MedAssistNG.Api.Models;
using MedAssistNG.Api.Services;

namespace MedAssistNG.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriageController : ControllerBase
    {
        private readonly GeminiService _gemini;

        public TriageController(GeminiService gemini)
        {
            _gemini = gemini;
        }

        [HttpPost]
        public async Task<IActionResult> Analyze([FromBody] SymptomRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Symptoms))
            {
                return BadRequest("Symptoms are required.");
            }

            var result = await _gemini.AnalyzeAsync(
                request.Symptoms,
                request.ImageBase64,
                request.ImageContentType
            );

            return Ok(result);
        }
    }
}