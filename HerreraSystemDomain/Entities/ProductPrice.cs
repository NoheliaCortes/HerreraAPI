using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class ProductPrice
{
    public int Id { get; set; }

    public int PriceTypeId { get; set; }

    public int? LinePresentationId { get; set; }

    public int? ProductId { get; set; }

    public decimal Price { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool? IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual LinePresentation? LinePresentation { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual PriceType PriceType { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
