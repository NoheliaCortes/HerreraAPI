using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IBatchRepository
    {
        Task<Batch> CreateAsync(Batch batch);
        Task<int> CountByYearAsync(int year);
        Task<string> BuildBatchCodeAsync(int productId, int year, int correlative);

        Task<Batch?> GetByIdAsync(int id);
    }
}
