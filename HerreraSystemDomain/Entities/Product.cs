using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Product
{
    public int Id { get; set; }

    public int LinePresentationId { get; set; }

    public int FlavorId { get; set; }

    public string ProductName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ImageUrl { get; set; }

    public int MinimumStock { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Flavor Flavor { get; set; } = null!;

    public virtual LinePresentation LinePresentation { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
}
