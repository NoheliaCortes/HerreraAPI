using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class OrderStatus
{
    public int Id { get; set; }

    public string OrderStatusName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
