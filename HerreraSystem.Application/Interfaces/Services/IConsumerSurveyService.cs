using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ConsumerSurveyDtos;

namespace HerreraSystem.Application.Interfaces.Services;

public interface IConsumerSurveyService
{
    Task<ServiceResult<bool>> RegisterAsync(CreateConsumerSurveyDto dto, CancellationToken cancellationToken = default);
}
