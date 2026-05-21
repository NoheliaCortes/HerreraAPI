using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class SaleDetail
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public int BatchId { get; set; }

    public int Quantity { get; set; }

    public decimal AppliedPrice { get; set; }

    public decimal? LineSubtotal { get; set; }

    public virtual Batch Batch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
