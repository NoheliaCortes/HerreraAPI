using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface ISaleRepository
    {
        Task<Sale> CreateAsync(Sale sale);
        Task<int> CountByYearAsync(int year);
    }
}
