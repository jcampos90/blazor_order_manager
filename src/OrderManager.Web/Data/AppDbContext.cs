using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Models;

namespace OrderManager.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<AppOwner> AppOwners => Set<AppOwner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(120);
            e.Property(p => p.Price).HasColumnType("numeric(12,2)");
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(160);
            e.Property(c => c.Phone).HasMaxLength(40);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.Status).HasConversion<int>();
            e.Property(o => o.DeliveryAt).HasColumnType("timestamp without time zone");
            e.Property(o => o.CreatedAt).HasColumnType("timestamp without time zone");
            e.HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId);
            e.Navigation(o => o.Customer).IsRequired();
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.Property(l => l.Quantity).HasColumnType("numeric(12,3)");
            e.Property(l => l.UnitPrice).HasColumnType("numeric(12,2)");
            e.HasOne(l => l.Order).WithMany(o => o.Lines).HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppOwner>(e =>
        {
            e.Property(o => o.Id).ValueGeneratedNever();
            e.Property(o => o.ClerkUserId).IsRequired().HasMaxLength(200);
            e.Property(o => o.CreatedAt).HasColumnType("timestamp without time zone");
            e.ToTable(t => t.HasCheckConstraint("CK_AppOwner_SingleRow", "\"Id\" = 1"));
        });
    }
}
