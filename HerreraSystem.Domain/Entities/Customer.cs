using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Customer
{
    public int Id { get; set; }

    public int MunicipalityId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? PointOfSale { get; set; }

    public string? Posaddress { get; set; }

    public bool? IsActive { get; set; }

    public virtual Municipality Municipality { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
