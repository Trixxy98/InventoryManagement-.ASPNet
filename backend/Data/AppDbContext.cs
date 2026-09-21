using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) :
base(options){

}

public DbSet<Category> Categories => Set<Category>();
public DbSet<Product> Products => Set<Product>();
public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
public DbSet<User> Users => Set<User>();

protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Category>(entity => 
    {
        entity.HasIndex(x => x.Name).IsUnique();
        entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(255);
    });

        modelBuilder.Entity<Product>(entity => 
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Price).HasPrecision(18,2);
            entity.HasQueryFilter(x => !x.IsDeleted);

            entity.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(40).IsRequired();
        });

    modelBuilder.Entity<StockTransaction>(entity => 
    {
        entity.Property(x => x.Note).HasMaxLength(255);

        entity.HasOne(x => x.Product)
            .WithMany(p => p.StockTransactions)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    var created = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc);

    modelBuilder.Entity<Category>().HasData(
        new Category {Id = 1, Name = "Electronics", Description = 
"Gadgets and accessories", CreatedAt = created},
        new Category {Id = 2, Name = "Stationery", Description = "Paper and pens", CreatedAt = created},
        new Category { Id = 3, Name = "Grocery", Description = "Daily essentials", CreatedAt = created },
        new Category {Id = 4, Name = "Clothing", Description = "Tops", CreatedAt = created}
    );

        modelBuilder.Entity<Product>().HasData(
            new Product {Id = 1, Name = "USB Cable", Sku = "USB-001", CategoryId = 1, Price = 9.90m, Quantity = 45, MinimumStock = 10, CreatedAt = created},
            new Product {Id = 2, Name = "Wireless Mouse", Sku = "MOU-002", CategoryId = 1, Price = 29.00m, Quantity = 8, MinimumStock = 10, CreatedAt = created},
            new Product {Id = 3, Name = "A4 Paper Ream", Sku = "PPR-003", CategoryId = 2, Price = 12.50m, Quantity = 0, MinimumStock = 5, CreatedAt = created},
            new Product {Id = 4, Name = "Blue Pen", Sku = "PEN-004", CategoryId = 2, Price = 1.20m, Quantity = 200, MinimumStock = 50, CreatedAt = created},
            new Product {Id = 5, Name = "Instant Noodles", Sku = "NOO-005", CategoryId = 3, Price = 2.50m, Quantity = 15, MinimumStock = 20, CreatedAt = created},
            new Product {Id = 6, Name = "Bottled Water", Sku = "WTR-006", CategoryId = 3, Price = 1.00m, Quantity = 3, MinimumStock = 12, CreatedAt = created},
            new Product {Id = 7, Name = "Black Polo Shirt", Sku = "POL-007", CategoryId = 4, Price = 1.00m, Quantity = 3, MinimumStock = 12, CreatedAt = created}
        );
}
}