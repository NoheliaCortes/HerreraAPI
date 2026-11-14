namespace HerreraSystem.Infrastructure.MongoDB.Settings;

public sealed class MongoDBSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "HerreraAnalytics";
    public string CollectionName { get; set; } = "consumer_surveys";
}
