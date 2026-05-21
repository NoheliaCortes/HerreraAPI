using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Presentation
{
    public int Id { get; set; }

    public string PresentationName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<LinePresentation> LinePresentations { get; set; } = new List<LinePresentation>();
}
