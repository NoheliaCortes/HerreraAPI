using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface ISaleDetailRepository
    {
        Task CreateAsync(SaleDetail detail);
    }
}
