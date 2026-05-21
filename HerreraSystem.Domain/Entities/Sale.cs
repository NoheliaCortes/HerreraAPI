using System;
using System.Collections.Generic;

namespace HerreraSystem.Domain.Entities;

public partial class Sale
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime? SaleDate { get; set; }

    public decimal TotalSale { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public decimal? PendingBalance { get; set; }

    public int CreatedBy { get; set; }

    public int PaymentTypeId { get; set; }

    public int SaleTypeId { get; set; }

    public string SaleCode { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual Order? Order { get; set; }

    public virtual PaymentType PaymentType { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    public virtual SaleType SaleType { get; set; } = null!;
}
