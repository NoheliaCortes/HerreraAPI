using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Line
{
    public int Id { get; set; }

    public string LineName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<LinePresentation> LinePresentations { get; set; } = new List<LinePresentation>();
}
