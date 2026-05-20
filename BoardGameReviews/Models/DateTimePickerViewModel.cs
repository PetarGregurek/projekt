namespace BoardGameReviews.Models
{
    public class DateTimePickerViewModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DateTime? Value { get; set; }
        public bool IsRequired { get; set; }
        public string RequiredErrorMessage { get; set; } = "This field is required.";
        public string? ValidationMessage { get; set; }
    }
}
