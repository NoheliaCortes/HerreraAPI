using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class RestockRepository:IRestockRepository
    {
        private readonly HerreraSystemContext _context;

        public RestockRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<Restock> CreateAsync(Restock restock)
        {
            _context.Restocks.Add(restock);
            await _context.SaveChangesAsync();
            return restock;
        }

        public async Task<int> CountByYearAsync(int year)
        {
            return await _context.Restocks
                .Where(r => r.RestockDate.HasValue && r.RestockDate.Value.Year == year)
                .CountAsync();
        }
    }
}
