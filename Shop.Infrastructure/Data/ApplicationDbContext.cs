using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities;
using Shop.Infrastructure.Identity;

namespace Shop.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<DownloadToken> DownloadTokens => Set<DownloadToken>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Slug).IsUnique();
            e.HasIndex(p => p.CategoryId);
            e.HasIndex(p => p.IsActive);
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.DiscountPrice).HasPrecision(18, 2);
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderNumber).IsUnique();
            // Keep indexable size for UserId (Identity keys are typically <= 450 chars).
            e.Property(o => o.UserId).HasMaxLength(450);
            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.UserId);
            e.HasIndex(o => o.CreatedAt);
            e.HasIndex(o => o.PaidAt);
            e.HasIndex(o => o.TotalAmount);
            e.Property(o => o.SubTotal).HasPrecision(18, 2);
            e.Property(o => o.DiscountAmount).HasPrecision(18, 2);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
            e.Property(i => i.TotalPrice).HasPrecision(18, 2);
            e.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CartItem>(e =>
        {
            e.HasOne(c => c.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DownloadToken>(e =>
        {
            e.HasIndex(t => t.Token).IsUnique();
            e.HasOne(t => t.OrderItem)
                .WithOne(i => i.DownloadToken)
                .HasForeignKey<DownloadToken>(t => t.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderStatusHistory>(e =>
        {
            e.HasIndex(h => h.OrderId);
            e.Property(h => h.OldStatus).HasConversion<int>();
            e.Property(h => h.NewStatus).HasConversion<int>();
            e.HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Article>(e =>
        {
            e.HasIndex(a => a.Slug).IsUnique();
        });

        builder.Entity<Comment>(e =>
        {
            e.HasIndex(c => c.ProductId);
            e.HasIndex(c => c.Status);
            e.HasIndex(c => c.CreatedAt);
            e.Property(c => c.UserId).HasMaxLength(450);
            e.Property(c => c.AuthorName).HasMaxLength(120);
            e.Property(c => c.AuthorEmail).HasMaxLength(256);
            e.Property(c => c.Content).HasMaxLength(2000);
            e.Property(c => c.AdminReply).HasMaxLength(2000);
            e.Property(c => c.Status).HasConversion<int>();
            e.HasOne(c => c.Product)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
