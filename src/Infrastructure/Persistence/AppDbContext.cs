
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<OrderRow> Orders => Set<OrderRow>();

  public DbSet<OrderLineRow> OrderLines => Set<OrderLineRow>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<OrderRow>(order =>
    {
      order.ToTable("Orders");

      order.HasKey(o => o.Id);

      order.Property(o => o.Id)
              .IsRequired()
              .HasMaxLength(64);

      order.Property(o => o.CustomerId)
              .IsRequired()
              .HasMaxLength(50);

      order.Property(o => o.CreatedAt)
              .IsRequired();

      order.Property(o => o.Status)
              .HasConversion<string>()
              .IsRequired()
              .HasMaxLength(20);

      order.HasIndex(o => o.CustomerId);

      order.HasMany(o => o.Lines)
              .WithOne(l => l.Order)
              .HasForeignKey(l => l.OrderId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<OrderLineRow>(line =>
    {
      line.ToTable("OrderLines");

      line.HasKey(l => l.Id);

      line.Property(l => l.Id)
              .ValueGeneratedOnAdd();

      line.Property(l => l.OrderId)
              .IsRequired()
              .HasMaxLength(64);

      line.Property(l => l.ProductId)
              .IsRequired()
              .HasMaxLength(50);

      line.Property(l => l.Name)
              .IsRequired()
              .HasMaxLength(200);

      line.Property(l => l.Price)
              .IsRequired();

      line.Property(l => l.Quantity)
              .IsRequired();

      line.ToTable(t =>
          {
          t.HasCheckConstraint(
                  "CK_OrderLines_Price",
                  "\"Price\" >= 0");

          t.HasCheckConstraint(
                  "CK_OrderLines_Quantity",
                  "\"Quantity\" > 0");
        });
    });
  }
}
