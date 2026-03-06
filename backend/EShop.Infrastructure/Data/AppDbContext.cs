using EShop.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
           entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Elektronika", Description = "Smartfony, laptopy, telewizory i inne urządzenia elektroniczne." },
            new Category { Id = 2, Name = "Moda", Description = "Odzież, obuwie i akcesoria dla kobiet, mężczyzn i dzieci." },
            new Category { Id = 3, Name = "Dom i Ogród", Description = "Meble, dekoracje, narzędzia ogrodowe i inne produkty do domu i ogrodu." },
            new Category { Id = 4, Name = "Sport i Rekreacja", Description = "Sprzęt sportowy, odzież sportowa i akcesoria dla miłośników aktywności fizycznej." },
            new Category { Id = 5, Name = "Zdrowie i Uroda", Description = "Kosmetyki, suplementy diety i produkty do pielęgnacji ciała." }
        );
    }
}