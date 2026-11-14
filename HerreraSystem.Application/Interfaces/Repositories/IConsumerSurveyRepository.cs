using HerreraSystem.Application.Models;

namespace HerreraSystem.Application.Interfaces.Repositories;

public interface IConsumerSurveyRepository
{
    Task InsertAsync(ConsumerSurveyRecord survey, CancellationToken cancellationToken = default);
}
