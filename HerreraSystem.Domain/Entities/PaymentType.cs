using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class PaymentType
{
    public int Id { get; set; }

    public string PaymentTypeName { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
