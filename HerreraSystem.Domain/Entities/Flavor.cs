using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Flavor
{
    public int Id { get; set; }

    public string FlavorName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? ImageUrl { get; set; }

    public string? FlavorColor { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
