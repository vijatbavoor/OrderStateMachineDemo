namespace OrderStateMachineDemo.Persistence;

using Microsoft.EntityFrameworkCore;

public class OrdersDbContext : DbContext
{
    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<StockEntity> Stock { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=orders.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // We control the ID
            entity.Property(e => e.State).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PaymentAttempts).HasDefaultValue(0);
            entity.Property(e => e.RequiredQuantity).HasDefaultValue(5);
        });

        modelBuilder.Entity<StockEntity>(entity =>
        {
            entity.HasKey(e => e.StockId);
            entity.Property(e => e.Quantity).HasDefaultValue(0);
        });
    }
}
