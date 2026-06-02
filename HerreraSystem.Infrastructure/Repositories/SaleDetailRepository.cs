using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class SaleDetailRepository : ISaleDetailRepository
    {
        private readonly HerreraSystemContext _context;

        public SaleDetailRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(SaleDetail detail)
        {
            _context.SaleDetails.Add(detail);
            await _context.SaveChangesAsync();
        }
    }
}
