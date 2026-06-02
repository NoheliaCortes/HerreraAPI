using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.DTOs.BatchDtos
{
    public class BatchLocationFIFODto
    {
        public int BatchLocationId { get; set; }
        public int BatchId { get; set; }
        public int CurrentStock { get; set; }
        public DateTime RestockDate { get; set; }

    }
}
