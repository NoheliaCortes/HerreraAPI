using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class InventoryMovementRepository : IInventoryMovementRepository
    {
        private readonly HerreraSystemContext _context;

        public InventoryMovementRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<InventoryMovement> CreateAsync(InventoryMovement movement)
        {
            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
            return movement;
        }
    }
}
