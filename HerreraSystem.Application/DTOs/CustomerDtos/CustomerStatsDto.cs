using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.CustomerDtos
{
    public class CustomerStatsDto
    {
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public int DistinctMunicipalitiesWithCustomers { get; set; }
        public int ActivePointsOfSale { get; set; }

    }
}
