using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class SaleType
{
    public int Id { get; set; }

    public string SaleTypeName { get; set; } = null!;

    public string? SaleTypeDescription { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
