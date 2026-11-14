using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.ConsumerSurveyDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers;

[Route("api/consumer-surveys")]
[ApiController]
public sealed class ConsumerSurveysController : ControllerBase
{
    private readonly IConsumerSurveyService _consumerSurveyService;

    public ConsumerSurveysController(IConsumerSurveyService consumerSurveyService)
        => _consumerSurveyService = consumerSurveyService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ConsumerSurveyReviewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReviews(
        [FromQuery] ConsumerSurveyQueryParams query,
        CancellationToken cancellationToken)
    {
        var result = await _consumerSurveyService.GetReviewsAsync(query, cancellationToken);
        if (!result.Success)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail(result.ErrorMessage ?? "No fue posible obtener las reseñas."));

        return Ok(ApiResponse<PagedResponse<ConsumerSurveyReviewDto>>.Ok(result.Data!));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Register(
        [FromBody] CreateConsumerSurveyDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _consumerSurveyService.RegisterAsync(dto, cancellationToken);
        if (!result.Success)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail(result.ErrorMessage ?? "No fue posible registrar la encuesta."));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<object>.Ok(new { registered = true }, "Encuesta registrada correctamente."));
    }
}
