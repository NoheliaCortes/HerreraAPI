using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace HerreraSystem.Application.DTOs.ConsumerSurveyDtos;

public sealed class CreateConsumerSurveyDto : IValidatableObject
{
    [Required, StringLength(100)]
    public string ReviewerName { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Line { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Flavor { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Presentation { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Municipality { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, Range(1, 5)]
    public int? Rating { get; set; }

    [Required]
    public bool? WouldRecommend { get; set; }

    [Required, StringLength(100)]
    public string Sentiment { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Comment { get; set; } = string.Empty;

    [StringLength(10)]
    public string? SurveyDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(SurveyDate) &&
            !DateOnly.TryParseExact(SurveyDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            yield return new ValidationResult("surveyDate debe tener formato yyyy-MM-dd y ser una fecha válida.", new[] { nameof(SurveyDate) });
        }
    }
}
