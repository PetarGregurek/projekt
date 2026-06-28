using System.ComponentModel.DataAnnotations;

namespace BoardGameReviews.Models
{
    public class AiFormPromptViewModel
    {
        [Required]
        public string EntityType { get; set; } = string.Empty;

        public string Title { get; set; } = "AI assistant";

        public string Placeholder { get; set; } = "Describe the data you want to enter...";
    }

    public class AiFormRequest
    {
        [Required]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Prompt { get; set; } = string.Empty;
    }

    public class AiFormFieldValue
    {
        public string Field { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? DisplayValue { get; set; }
    }

    public class AiFormSuggestion
    {
        public List<AiFormFieldValue> Fields { get; set; } = new();
        public string? Notes { get; set; }
    }
}
