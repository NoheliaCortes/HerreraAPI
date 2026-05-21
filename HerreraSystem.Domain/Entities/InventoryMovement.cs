using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class InventoryMovement
{
    public int Id { get; set; }

    public int MovementTypeId { get; set; }

    public int? SaleId { get; set; }

    public int? OrderId { get; set; }

    public DateTime? MovementDate { get; set; }

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }

    public bool? IsActive { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<MovementDetail> MovementDetails { get; set; } = new List<MovementDetail>();

    public virtual MovementType MovementType { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual Sale? Sale { get; set; }
}
