using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.RestockDtos;
using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IRestockRepository
    {
        Task<Restock> CreateAsync(Restock restock);
        Task<int> CountByYearAsync(int year);
        Task<PagedResponse<RestockListItemDto>> GetAllAsync(RestockQueryParams queryParams);
        Task<RestockDetailDto?> GetDetailAsync(int id);
        Task<RestockStatisticsDto> GetStatisticsAsync();
    }
}
