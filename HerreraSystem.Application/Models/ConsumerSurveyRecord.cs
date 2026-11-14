namespace HerreraSystem.Application.Models;

public sealed class ConsumerSurveyRecord
{
    public string ReviewerName { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string Line { get; init; } = string.Empty;
    public string Flavor { get; init; } = string.Empty;
    public string Presentation { get; init; } = string.Empty;
    public string Municipality { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public int Rating { get; init; }
    public bool WouldRecommend { get; init; }
    public string Sentiment { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string SurveyDate { get; init; } = string.Empty;
    public string Channel { get; init; } = "Formulario web";
}
