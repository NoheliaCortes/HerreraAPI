using System.Globalization;
using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ConsumerSurveyDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Application.Models;

namespace HerreraSystem.Application.Services;

public sealed class ConsumerSurveyService : IConsumerSurveyService
{
    private const string FormChannel = "Formulario web";
    private readonly IConsumerSurveyRepository _repository;
    private readonly INicaraguaDateTimeService _dateTimeService;

    public ConsumerSurveyService(
        IConsumerSurveyRepository repository,
        INicaraguaDateTimeService dateTimeService)
    {
        _repository = repository;
        _dateTimeService = dateTimeService;
    }

    public async Task<ServiceResult<bool>> RegisterAsync(
        CreateConsumerSurveyDto dto,
        CancellationToken cancellationToken = default)
    {
        var survey = new ConsumerSurveyRecord
        {
            ReviewerName = dto.ReviewerName.Trim(),
            ProductName = dto.ProductName.Trim(),
            Line = dto.Line.Trim(),
            Flavor = dto.Flavor.Trim(),
            Presentation = dto.Presentation.Trim(),
            Municipality = dto.Municipality.Trim(),
            Department = dto.Department.Trim(),
            Rating = dto.Rating!.Value,
            WouldRecommend = dto.WouldRecommend!.Value,
            Sentiment = dto.Sentiment.Trim(),
            Comment = dto.Comment.Trim(),
            SurveyDate = string.IsNullOrWhiteSpace(dto.SurveyDate)
                ? _dateTimeService.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : dto.SurveyDate.Trim(),
            Channel = FormChannel
        };

        try
        {
            await _repository.InsertAsync(survey, cancellationToken);
            return ServiceResult<bool>.Ok(true);
        }
        catch (ConsumerSurveyStorageException)
        {
            return ServiceResult<bool>.Fail("No fue posible registrar la encuesta en este momento.");
        }
    }
}
