using System;
using System.Collections.Generic;
using HerreraSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Data;

public partial class HerreraSystemContext : DbContext
{
    public HerreraSystemContext()
    {
    }

    public HerreraSystemContext(DbContextOptions<HerreraSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<BatchLocation> BatchLocations { get; set; }

    public virtual DbSet<BatchStatus> BatchStatuses { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Flavor> Flavors { get; set; }

    public virtual DbSet<InventoryMovement> InventoryMovements { get; set; }

    public virtual DbSet<Line> Lines { get; set; }

    public virtual DbSet<LinePresentation> LinePresentations { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<MovementDetail> MovementDetails { get; set; }

    public virtual DbSet<MovementType> MovementTypes { get; set; }

    public virtual DbSet<Municipality> Municipalities { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<Presentation> Presentations { get; set; }

    public virtual DbSet<PriceType> PriceTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductPrice> ProductPrices { get; set; }

    public virtual DbSet<Restock> Restocks { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleDetail> SaleDetails { get; set; }

    public virtual DbSet<SaleType> SaleTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MARK42;Database=HerreraSystem;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Batches__3214EC07F0175079");

            entity.HasIndex(e => e.BatchCode, "UQ__Batches__B22ADA8E7340E5C3").IsUnique();

            entity.Property(e => e.BatchCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitProductionCost).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.BatchStatus).WithMany(p => p.Batches)
                .HasForeignKey(d => d.BatchStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Batch_Status");

            entity.HasOne(d => d.Product).WithMany(p => p.Batches)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Batch_Product");

            entity.HasOne(d => d.Restock).WithMany(p => p.Batches)
                .HasForeignKey(d => d.RestockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Batch_Restock");
        });

        modelBuilder.Entity<BatchLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BatchLoc__3214EC072F9B81E0");

            entity.HasIndex(e => new { e.BatchId, e.LocationId }, "UQ_BatchLocation").IsUnique();

            entity.HasOne(d => d.Batch).WithMany(p => p.BatchLocations)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BatchLocation_Batch");

            entity.HasOne(d => d.Location).WithMany(p => p.BatchLocations)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BatchLocation_Location");
        });

        modelBuilder.Entity<BatchStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BatchSta__3214EC0755474FB9");

            entity.Property(e => e.BatchStatusName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07061A3DB2");

            entity.Property(e => e.FirstName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PointOfSale)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Posaddress)
                .IsUnicode(false)
                .HasColumnName("POSAddress");

            entity.HasOne(d => d.Municipality).WithMany(p => p.Customers)
                .HasForeignKey(d => d.MunicipalityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Customer_Municipality");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC0742C9F50F");

            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Flavor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Flavors__3214EC072A0E89A0");

            entity.Property(e => e.FlavorColor)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.FlavorName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2048)
                .IsUnicode(false)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC07F67FBA38");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MovementDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvMov_User_CreatedBy");

            entity.HasOne(d => d.MovementType).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.MovementTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvMov_MovementType");

            entity.HasOne(d => d.Order).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_InvMov_Order");

            entity.HasOne(d => d.Sale).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("FK_InvMov_Sale");
        });

        modelBuilder.Entity<Line>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lines__3214EC0738136C76");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LineName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LinePresentation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LinePres__3214EC07A37CA463");

            entity.HasIndex(e => new { e.LineId, e.PresentationId }, "UQ_Line_Presentation").IsUnique();

            entity.HasOne(d => d.Line).WithMany(p => p.LinePresentations)
                .HasForeignKey(d => d.LineId)
                .HasConstraintName("FK_Line");

            entity.HasOne(d => d.Presentation).WithMany(p => p.LinePresentations)
                .HasForeignKey(d => d.PresentationId)
                .HasConstraintName("FK_Presentation");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC075DA65C6F");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LocationName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MovementDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Movement__3214EC076F6ED98A");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Batch).WithMany(p => p.MovementDetails)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovDet_Batch");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.MovementDetails)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovDet_User_CreatedBy");

            entity.HasOne(d => d.DestinationLocation).WithMany(p => p.MovementDetailDestinationLocations)
                .HasForeignKey(d => d.DestinationLocationId)
                .HasConstraintName("FK_MovDet_DestLoc");

            entity.HasOne(d => d.Movement).WithMany(p => p.MovementDetails)
                .HasForeignKey(d => d.MovementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovDet_Movement");

            entity.HasOne(d => d.SourceLocation).WithMany(p => p.MovementDetailSourceLocations)
                .HasForeignKey(d => d.SourceLocationId)
                .HasConstraintName("FK_MovDet_SourceLoc");
        });

        modelBuilder.Entity<MovementType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Movement__3214EC077C997F22");

            entity.Property(e => e.MovementTypeName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Municipality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Municipa__3214EC072203A186");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MunicipalityName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Department).WithMany(p => p.Municipalities)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Department_Municipality");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC07D1B14B5E");

            entity.HasIndex(e => e.OrderCode, "UQ_OrderCode").IsUnique();

            entity.Property(e => e.ActualDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.EstimatedDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.OrderCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalOrder)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_User_CreatedBy");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Customer");

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Status");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC0723A315DA");

            entity.HasOne(d => d.Batch).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK_OrdDet_Batch");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdDet_Order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdDet_Product");

            entity.HasOne(d => d.ProductPrice).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductPriceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdDet_Price");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderSta__3214EC073A31751A");

            entity.Property(e => e.OrderStatusName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07A9E7F76F");

            entity.Property(e => e.AmountReceived).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionReference)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Method");

            entity.HasOne(d => d.RegisteredByNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.RegisteredBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_User_RegisteredBy");

            entity.HasOne(d => d.Sale).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Sale");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentM__3214EC07E82D4CDB");

            entity.Property(e => e.PaymentMethodName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentT__3214EC07D013BFE6");

            entity.Property(e => e.PaymentTypeName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Presentation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Presenta__3214EC07994D0105");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PresentationName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PriceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PriceTyp__3214EC074AFD7546");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PriceName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC078717DBEB");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2048)
                .IsUnicode(false)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_CreatedBy");

            entity.HasOne(d => d.Flavor).WithMany(p => p.Products)
                .HasForeignKey(d => d.FlavorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Flavor");

            entity.HasOne(d => d.LinePresentation).WithMany(p => p.Products)
                .HasForeignKey(d => d.LinePresentationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LinePresentation");
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductP__3214EC0795BEEC37");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ValidFrom)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ValidTo).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Price_CreatedBy");

            entity.HasOne(d => d.LinePresentation).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.LinePresentationId)
                .HasConstraintName("FK_LinePresentation_Price");

            entity.HasOne(d => d.PriceType).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.PriceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PriceType");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Product_Price");
        });

        modelBuilder.Entity<Restock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producti__3214EC073165B437");

            entity.HasIndex(e => e.RestockCode, "UQ_RestockCode").IsUnique();

            entity.Property(e => e.RestockCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RestockDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Restocks)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Restock_User_CreatedBy");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC072E1E2D97");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleDescription)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sales__3214EC070AAA74F8");

            entity.HasIndex(e => e.SaleCode, "UQ_SaleCode").IsUnique();

            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PendingBalance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SaleCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalSale).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_User_CreatedBy");

            entity.HasOne(d => d.Customer).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_Customer");

            entity.HasOne(d => d.Order).WithMany(p => p.Sales)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Sale_Order");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.Sales)
                .HasForeignKey(d => d.PaymentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_PaymentType");

            entity.HasOne(d => d.SaleType).WithMany(p => p.Sales)
                .HasForeignKey(d => d.SaleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_SaleType");
        });

        modelBuilder.Entity<SaleDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SaleDeta__3214EC07A5B246F4");

            entity.Property(e => e.AppliedPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LineSubtotal)
                .HasComputedColumnSql("([Quantity]*[AppliedPrice])", false)
                .HasColumnType("decimal(21, 2)");

            entity.HasOne(d => d.Batch).WithMany(p => p.SaleDetails)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleDet_Batch");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleDet_Product");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleDetails)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleDet_Sale");
        });

        modelBuilder.Entity<SaleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SaleType__3214EC0723E150E3");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SaleTypeDescription)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.SaleTypeName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC072725DF1E");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105345433F74E").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F2845638A1D3E6").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ResetToken).HasMaxLength(50);
            entity.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC07346CF698");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_User_Role").IsUnique();

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
