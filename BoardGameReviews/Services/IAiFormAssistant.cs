using BoardGameReviews.Models;

namespace BoardGameReviews.Services
{
    public interface IAiFormAssistant
    {
        Task<AiFormSuggestion> GenerateSuggestionAsync(AiFormRequest request, CancellationToken cancellationToken = default);
    }
}
