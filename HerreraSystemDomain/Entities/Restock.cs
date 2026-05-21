using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Restock
{
    public int Id { get; set; }

    public DateTime? RestockDate { get; set; }

    public int CreatedBy { get; set; }

    public string RestockCode { get; set; } = null!;

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual User CreatedByNavigation { get; set; } = null!;
}
