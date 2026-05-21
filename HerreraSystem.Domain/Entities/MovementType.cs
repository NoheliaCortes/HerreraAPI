using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class MovementType
{
    public int Id { get; set; }

    public string MovementTypeName { get; set; } = null!;

    public short Sign { get; set; }

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
