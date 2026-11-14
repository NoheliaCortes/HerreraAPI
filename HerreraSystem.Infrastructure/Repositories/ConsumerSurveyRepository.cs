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

    public async Task<PagedResponse<ConsumerSurveyRecord>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = _collectionProvider.Collection;
            var filter = Builders<ConsumerSurveyDocument>.Filter.Empty;
            var totalCount = await collection.CountDocumentsAsync(
                filter,
                cancellationToken: cancellationToken);

            var totalPagesLong = totalCount == 0
                ? 0
                : (totalCount + pageSize - 1) / pageSize;
            var totalPages = (int)Math.Min(totalPagesLong, int.MaxValue);
            var skipLong = ((long)page - 1) * pageSize;
            var documents = skipLong > int.MaxValue || totalCount == 0
                ? new List<ConsumerSurveyDocument>()
                : await collection.Find(filter)
                    .Sort(Builders<ConsumerSurveyDocument>.Sort
                        .Descending(document => document.SurveyDate)
                        .Descending("_id"))
                    .Skip((int)skipLong)
                    .Limit(pageSize)
                    .ToListAsync(cancellationToken);

            return new PagedResponse<ConsumerSurveyRecord>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = (int)Math.Min(totalCount, int.MaxValue),
                TotalPages = totalPages,
                Data = documents.Select(document => new ConsumerSurveyRecord
                {
                    ReviewerName = document.ReviewerName,
                    ProductName = document.ProductName,
                    Line = document.Line,
                    Flavor = document.Flavor,
                    Presentation = document.Presentation,
                    Municipality = document.Municipality,
                    Department = document.Department,
                    Rating = document.Rating,
                    WouldRecommend = document.WouldRecommend,
                    Sentiment = document.Sentiment,
                    Comment = document.Comment,
                    SurveyDate = document.SurveyDate,
                    Channel = document.Channel
                }).ToList()
            };
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
