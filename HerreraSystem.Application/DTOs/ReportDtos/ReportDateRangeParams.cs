using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.ReportDtos;

public class ReportDateRangeParams : IValidatableObject
{
    [Required]
    public DateTime? StartDate { get; set; }

    [Required]
    public DateTime? EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date > EndDate.Value.Date)
            yield return new ValidationResult("startDate debe ser menor o igual que endDate", new[] { nameof(StartDate), nameof(EndDate) });
    }
}
