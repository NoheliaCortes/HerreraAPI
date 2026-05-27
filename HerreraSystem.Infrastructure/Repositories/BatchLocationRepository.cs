using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class BatchLocationRepository: IBatchLocationRepository
    {
        private readonly HerreraSystemContext _context;

        public BatchLocationRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(BatchLocation batchLocation)
        {
            _context.BatchLocations.Add(batchLocation);
            await _context.SaveChangesAsync();
        }
    }
}
