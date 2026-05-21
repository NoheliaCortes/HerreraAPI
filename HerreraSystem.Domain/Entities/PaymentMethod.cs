using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class PaymentMethod
{
    public int Id { get; set; }

    public string PaymentMethodName { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
