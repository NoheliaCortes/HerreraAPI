using HerreraSystem.Application.DTOs.FlavorDtos;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class FlavorRepository : IFlavorRepository   
    {
        private readonly HerreraSystemContext _context;

        public FlavorRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<List<FlavorDto>> GetAllAsync()
        {
            return await _context.Flavors
                .Select(f => new FlavorDto
                {
                    Id = f.Id,
                    FlavorName = f.FlavorName,
                    IsActive = f.IsActive,
                    ImageUrl = f.ImageUrl,
                    FlavorColor = f.FlavorColor
                }).ToListAsync();
        }

        public async Task<FlavorDto?> GetByIdAsync(int id)
        {
            var flavor = await _context.Flavors.FindAsync(id);
            if (flavor is null) return null;

            return new FlavorDto
            {
                Id = flavor.Id,
                FlavorName = flavor.FlavorName,
                IsActive = flavor.IsActive,
                ImageUrl = flavor.ImageUrl,
                FlavorColor = flavor.FlavorColor
            };
        }

        public async Task<FlavorDto> CreateAsync(CreateFlavorDto dto)
        {
            var flavor = new Flavor
            {
                FlavorName = dto.FlavorName,
                ImageUrl = dto.ImageUrl,
                FlavorColor = dto.FlavorColor,
                IsActive = true
            };

            _context.Flavors.Add(flavor);
            await _context.SaveChangesAsync();

            return new FlavorDto
            {
                Id = flavor.Id,
                FlavorName = flavor.FlavorName,
                IsActive = flavor.IsActive,
                ImageUrl = flavor.ImageUrl,
                FlavorColor = flavor.FlavorColor
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateFlavorDto dto)
        {
            var flavor = await _context.Flavors.FindAsync(id);
            if (flavor is null) return false;

            flavor.FlavorName = dto.FlavorName;
            flavor.IsActive = dto.IsActive;
            flavor.ImageUrl = dto.ImageUrl;
            flavor.FlavorColor = dto.FlavorColor;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var flavor = await _context.Flavors.FindAsync(id);
            if (flavor is null) return false;

            _context.Flavors.Remove(flavor);
            await _context.SaveChangesAsync();
            return true;
        }



    }
}
