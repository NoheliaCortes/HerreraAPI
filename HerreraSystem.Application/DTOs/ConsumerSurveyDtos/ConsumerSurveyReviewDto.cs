namespace HerreraSystem.Application.DTOs.ConsumerSurveyDtos;

public sealed class ConsumerSurveyReviewDto
{
    public string ReviewerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Line { get; set; } = string.Empty;
    public string Flavor { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool WouldRecommend { get; set; }
    public string Sentiment { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string SurveyDate { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
}
