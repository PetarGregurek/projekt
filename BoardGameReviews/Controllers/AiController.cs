using BoardGameReviews.Data;
using BoardGameReviews.Models;
using BoardGameReviews.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameReviews.Controllers
{
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public class AiController : Controller
    {
        private readonly IAiFormAssistant _assistant;

        public AiController(IAiFormAssistant assistant) => _assistant = assistant;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suggest([FromBody] AiFormRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Enter a clear AI prompt before generating data." });
            }

            try
            {
                var suggestion = await _assistant.GenerateSuggestionAsync(request, cancellationToken);
                return Json(suggestion);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "This entity is not supported for AI input." });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
            }
        }
    }
}
