using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Common
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;

        public int Page { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize
                ? MaxPageSize
                : value;
        }

    }
}
