using MongoDB.Bson.Serialization.Attributes;

namespace HerreraSystem.Infrastructure.MongoDB;

[BsonIgnoreExtraElements]
public sealed class ConsumerSurveyDocument
{
    [BsonElement("reviewerName")] public string ReviewerName { get; set; } = string.Empty;
    [BsonElement("productName")] public string ProductName { get; set; } = string.Empty;
    [BsonElement("line")] public string Line { get; set; } = string.Empty;
    [BsonElement("flavor")] public string Flavor { get; set; } = string.Empty;
    [BsonElement("presentation")] public string Presentation { get; set; } = string.Empty;
    [BsonElement("municipality")] public string Municipality { get; set; } = string.Empty;
    [BsonElement("department")] public string Department { get; set; } = string.Empty;
    [BsonElement("rating")] public int Rating { get; set; }
    [BsonElement("wouldRecommend")] public bool WouldRecommend { get; set; }
    [BsonElement("sentiment")] public string Sentiment { get; set; } = string.Empty;
    [BsonElement("comment")] public string Comment { get; set; } = string.Empty;
    [BsonElement("surveyDate")] public string SurveyDate { get; set; } = string.Empty;
    [BsonElement("channel")] public string Channel { get; set; } = "Formulario web";
}
