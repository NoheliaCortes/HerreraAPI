using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Location
{
    public int Id { get; set; }

    public string LocationName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<BatchLocation> BatchLocations { get; set; } = new List<BatchLocation>();

    public virtual ICollection<MovementDetail> MovementDetailDestinationLocations { get; set; } = new List<MovementDetail>();

    public virtual ICollection<MovementDetail> MovementDetailSourceLocations { get; set; } = new List<MovementDetail>();
}
