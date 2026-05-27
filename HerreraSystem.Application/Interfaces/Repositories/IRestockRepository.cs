using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IRestockRepository
    {
        Task<Restock> CreateAsync(Restock restock);
        Task<int> CountByYearAsync(int year);  // para el correlativo anual
    }
}
