using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int QuantityRequested { get; set; }

    public int ProductPriceId { get; set; }

    public int? BatchId { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductPrice ProductPrice { get; set; } = null!;
}
