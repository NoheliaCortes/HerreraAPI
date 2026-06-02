using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Application.Common
{
    public static class PaginationExtensions
    {
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
            this IQueryable<T> query,
            PaginationParams paginationParams)
        {
            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResponse<T>
            {
                Data = data,
                CurrentPage = paginationParams.Page,
                PageSize = paginationParams.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)paginationParams.PageSize)
            };
        }

    }
}
