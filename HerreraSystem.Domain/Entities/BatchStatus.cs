using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class BatchStatus
{
    public int Id { get; set; }

    public string BatchStatusName { get; set; } = null!;

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
}
