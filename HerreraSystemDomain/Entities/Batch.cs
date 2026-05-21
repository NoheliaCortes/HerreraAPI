using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Batch
{
    public int Id { get; set; }

    public int RestockId { get; set; }

    public int ProductId { get; set; }

    public int BatchStatusId { get; set; }

    public int InitialQuantity { get; set; }

    public decimal UnitProductionCost { get; set; }

    public DateOnly ExpirationDate { get; set; }

    public string? BatchCode { get; set; }

    public virtual ICollection<BatchLocation> BatchLocations { get; set; } = new List<BatchLocation>();

    public virtual BatchStatus BatchStatus { get; set; } = null!;

    public virtual ICollection<MovementDetail> MovementDetails { get; set; } = new List<MovementDetail>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

    public virtual Restock Restock { get; set; } = null!;

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
}
