using HerreraSystem.Infrastructure.MongoDB.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HerreraSystem.Infrastructure.MongoDB;

public sealed class ConsumerSurveyCollectionProvider
{
    private readonly Lazy<IMongoCollection<ConsumerSurveyDocument>> _collection;

    public ConsumerSurveyCollectionProvider(IOptions<MongoDBSettings> settings)
    {
        var value = settings.Value;
        _collection = new Lazy<IMongoCollection<ConsumerSurveyDocument>>(() =>
        {
            if (string.IsNullOrWhiteSpace(value.ConnectionString))
                throw new InvalidOperationException("MongoDB connection string is not configured.");

            var client = new MongoClient(value.ConnectionString);
            return client.GetDatabase(value.DatabaseName)
                .GetCollection<ConsumerSurveyDocument>(value.CollectionName);
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public IMongoCollection<ConsumerSurveyDocument> Collection => _collection.Value;
}
