using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.CustomerDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;    

namespace HerreraSystem.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly HerreraSystemContext _context;

        public CustomerRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<CustomerDto>> GetAllAsync(
            PaginationParams paginationParams)
        {
            var query = _context.Customers
                .AsNoTracking()
                .Include(c => c.Municipality)
                    .ThenInclude(m => m.Department)
                .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    MunicipalityId = c.MunicipalityId,
                    MunicipalityName = c.Municipality.MunicipalityName,
                    DepartmentId = c.Municipality.DepartmentId,
                    DepartmentName = c.Municipality.Department.DepartmentName,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    PointOfSale = c.PointOfSale,
                    Posaddress = c.Posaddress,
                    IsActive = c.IsActive
                });

            return await query.ToPagedResponseAsync(paginationParams);
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Include(c => c.Municipality)
                    .ThenInclude(m => m.Department)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer is null) return null;

            return new CustomerDto
            {
                Id = customer.Id,
                MunicipalityId = customer.MunicipalityId,
                MunicipalityName = customer.Municipality.MunicipalityName,
                DepartmentId = customer.Municipality.DepartmentId,
                DepartmentName = customer.Municipality.Department.DepartmentName,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                PointOfSale = customer.PointOfSale,
                Posaddress = customer.Posaddress,
                IsActive = customer.IsActive
            };
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                MunicipalityId = dto.MunicipalityId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                PointOfSale = dto.PointOfSale,
                Posaddress = dto.Posaddress,
                IsActive = true
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Recargar con relaciones para devolver nombres
            return await _context.Customers
                .AsNoTracking()
                .Include(c => c.Municipality)
                    .ThenInclude(m => m.Department)
                .Where(c => c.Id == customer.Id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    MunicipalityId = c.MunicipalityId,
                    MunicipalityName = c.Municipality.MunicipalityName,
                    DepartmentId = c.Municipality.DepartmentId,
                    DepartmentName = c.Municipality.Department.DepartmentName,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    PointOfSale = c.PointOfSale,
                    Posaddress = c.Posaddress,
                    IsActive = c.IsActive
                })
                .FirstAsync();
        }

        public async Task<bool> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer is null) return false;

            customer.MunicipalityId = dto.MunicipalityId;
            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.IsActive = dto.IsActive;
            customer.Phone = dto.Phone;
            customer.PointOfSale = dto.PointOfSale;
            customer.Posaddress = dto.Posaddress;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer is null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(
            string firstName,
            string lastName,
            int municipalityId,
            int? excludeId = null)
        {
            return await _context.Customers
                .AnyAsync(c =>
                    c.FirstName == firstName &&
                    c.LastName == lastName &&
                    c.MunicipalityId == municipalityId &&
                    (excludeId == null || c.Id != excludeId));
        }

        public async Task<bool> HasOrdersOrSalesAsync(int customerId)
        {
            var hasOrders = await _context.Orders
                .AnyAsync(o => o.CustomerId == customerId);

            if (hasOrders) return true;

            return await _context.Sales
                .AnyAsync(s => s.CustomerId == customerId);
        }
    }
}
