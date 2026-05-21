using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Payment
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int PaymentMethodId { get; set; }

    public decimal AmountReceived { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? TransactionReference { get; set; }

    public int RegisteredBy { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual User RegisteredByNavigation { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
