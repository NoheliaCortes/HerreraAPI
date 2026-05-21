using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class PriceType
{
    public int Id { get; set; }

    public string PriceName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
}
