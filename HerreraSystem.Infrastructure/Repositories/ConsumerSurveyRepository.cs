using HerreraSystem.Application.Common;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Models;
using HerreraSystem.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace HerreraSystem.Infrastructure.Repositories;

public sealed class ConsumerSurveyRepository : IConsumerSurveyRepository
{
    private readonly ConsumerSurveyCollectionProvider _collectionProvider;

    public ConsumerSurveyRepository(ConsumerSurveyCollectionProvider collectionProvider)
        => _collectionProvider = collectionProvider;

    public async Task InsertAsync(ConsumerSurveyRecord survey, CancellationToken cancellationToken = default)
    {
        var document = new ConsumerSurveyDocument
        {
            ReviewerName = survey.ReviewerName,
            ProductName = survey.ProductName,
            Line = survey.Line,
            Flavor = survey.Flavor,
            Presentation = survey.Presentation,
            Municipality = survey.Municipality,
            Department = survey.Department,
            Rating = survey.Rating,
            WouldRecommend = survey.WouldRecommend,
            Sentiment = survey.Sentiment,
            Comment = survey.Comment,
            SurveyDate = survey.SurveyDate,
            Channel = survey.Channel
        };

        try
        {
            await _collectionProvider.Collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ConsumerSurveyStorageException(ex);
        }
    }
}
