using HerreraSystem.Application.Models;
using HerreraSystem.Application.Common;

namespace HerreraSystem.Application.Interfaces.Repositories;

public interface IConsumerSurveyRepository
{
    Task InsertAsync(ConsumerSurveyRecord survey, CancellationToken cancellationToken = default);
    Task<PagedResponse<ConsumerSurveyRecord>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
