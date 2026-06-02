using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly HerreraSystemContext _context;

        public SaleRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<Sale> CreateAsync(Sale sale)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task<int> CountByYearAsync(int year)
        {
            return await _context.Sales
                .Where(s => s.SaleDate.HasValue && s.SaleDate.Value.Year == year)
                .CountAsync();
        }

    }
}
