using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Municipality
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public string MunicipalityName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual Department Department { get; set; } = null!;
}
