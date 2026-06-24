using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RestockDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IRestockService
    {
        Task<ServiceResult<RestockResponseDto>> CreateRestockAsync(CreateRestockDto dto);
        Task<PagedResponse<RestockListItemDto>> GetAllAsync(RestockQueryParams queryParams);
        Task<RestockDetailDto?> GetDetailAsync(int id);
        Task<RestockStatisticsDto> GetStatisticsAsync();
    }
}
