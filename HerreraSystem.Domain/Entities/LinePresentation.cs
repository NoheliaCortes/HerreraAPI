using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class LinePresentation
{
    public int Id { get; set; }

    public int LineId { get; set; }

    public int PresentationId { get; set; }

    public virtual Line Line { get; set; } = null!;

    public virtual Presentation Presentation { get; set; } = null!;

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
