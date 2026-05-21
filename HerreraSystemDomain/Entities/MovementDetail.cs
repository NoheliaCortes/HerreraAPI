using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class MovementDetail
{
    public int Id { get; set; }

    public int MovementId { get; set; }

    public int BatchId { get; set; }

    public int? SourceLocationId { get; set; }

    public int? DestinationLocationId { get; set; }

    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal UnitCost { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Batch Batch { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Location? DestinationLocation { get; set; }

    public virtual InventoryMovement Movement { get; set; } = null!;

    public virtual Location? SourceLocation { get; set; }
}
