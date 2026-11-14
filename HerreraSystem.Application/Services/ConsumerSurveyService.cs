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

    public async Task<ServiceResult<PagedResponse<ConsumerSurveyReviewDto>>> GetReviewsAsync(
        ConsumerSurveyQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        try
        {
            var result = await _repository.GetPagedAsync(page, pageSize, cancellationToken);
            var response = new PagedResponse<ConsumerSurveyReviewDto>
            {
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                Data = result.Data.Select(survey => new ConsumerSurveyReviewDto
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
                }).ToList()
            };

            return ServiceResult<PagedResponse<ConsumerSurveyReviewDto>>.Ok(response);
        }
        catch (ConsumerSurveyStorageException)
        {
            return ServiceResult<PagedResponse<ConsumerSurveyReviewDto>>.Fail(
                "No fue posible obtener las reseñas en este momento.");
        }
    }
}
