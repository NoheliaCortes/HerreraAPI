using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class BatchLocation
{
    public int Id { get; set; }

    public int BatchId { get; set; }

    public int LocationId { get; set; }

    public int CurrentStock { get; set; }

    public virtual Batch Batch { get; set; } = null!;

    public virtual Location Location { get; set; } = null!;
}
