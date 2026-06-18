using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shop.Domain.Entities;
using Shop.Infrastructure.Identity;

namespace Shop.Infrastructure.Data;

public static class DbSeeder
{
    private const string InitialMigrationId = "20260607111303_InitialCreate";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // serviceProvider is already a scoped provider created in Program.cs
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await ApplyMigrationsAsync(context);

        // Seed roles
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole("Customer"));

        // Seed admin user
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@technoshop.local",
                FullName = "مدیر سیستم",
                PhoneNumber = "09000000000",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await context.Categories.AnyAsync())
            return;

        // Seed categories
        var mobile = new Category { Name = "گوشی موبایل", Slug = "mobile", DisplayOrder = 1 };
        var tablet = new Category { Name = "تبلت", Slug = "tablet", DisplayOrder = 2 };
        var laptop = new Category { Name = "لپ تاپ", Slug = "laptop", DisplayOrder = 3 };
        var digital = new Category { Name = "محصولات دیجیتال", Slug = "digital", DisplayOrder = 4 };

        context.Categories.AddRange(mobile, tablet, laptop, digital);
        await context.SaveChangesAsync();

        var samsung = new Category { Name = "سامسونگ", Slug = "samsung", ParentCategoryId = mobile.Id, DisplayOrder = 1 };
        var apple = new Category { Name = "اپل", Slug = "apple", ParentCategoryId = mobile.Id, DisplayOrder = 2 };
        context.Categories.AddRange(samsung, apple);
        await context.SaveChangesAsync();

        // Seed products
        var products = new List<Product>
        {
            new()
            {
                Name = "ساعت هوشمند سری 7 اپل",
                Slug = "apple-watch-series-7",
                ShortDescription = "Apple Watch Series 7",
                Description = "<p>ساعت هوشمند اپل با قابلیت‌های پیشرفته سلامت و ورزش</p>",
                Price = 12_000_000,
                DiscountPrice = 10_500_000,
                MainImageUrl = "/images/products/mo.png",
                Brand = "Apple",
                CategoryId = apple.Id,
                IsFeatured = true,
                IsActive = true,
                DigitalFileName = "sample-product.zip",
                DigitalFileOriginalName = "apple-watch-guide.zip"
            },
            new()
            {
                Name = "گوشی موبایل اپل iPhone 12 Pro Max",
                Slug = "iphone-12-pro-max",
                ShortDescription = "iPhone 12 Pro Max",
                Description = "<p>گوشی موبایل اپل مدل iPhone 12 Pro Max با صفحه Super Retina XDR OLED</p>",
                Price = 45_000_000,
                DiscountPrice = 42_000_000,
                MainImageUrl = "/images/singleproduct/apple-16.jfif",
                Brand = "Apple",
                CategoryId = apple.Id,
                IsFeatured = true,
                IsActive = true,
                DigitalFileName = "sample-product.zip",
                DigitalFileOriginalName = "iphone-manual.zip"
            },
            new()
            {
                Name = "گوشی سامسونگ Galaxy A54",
                Slug = "samsung-galaxy-a54",
                ShortDescription = "Samsung Galaxy A54",
                Description = "<p>گوشی میان‌رده سامسونگ با دوربین عالی</p>",
                Price = 18_000_000,
                MainImageUrl = "/images/singleproduct/a54.jfif",
                Brand = "Samsung",
                CategoryId = samsung.Id,
                IsFeatured = true,
                IsSpecialOffer = true,
                IsActive = true,
                DigitalFileName = "sample-product.zip",
                DigitalFileOriginalName = "galaxy-a54-guide.zip"
            },
            new()
            {
                Name = "قالب وردپرس فروشگاهی",
                Slug = "wordpress-shop-theme",
                ShortDescription = "قالب حرفه‌ای وردپرس",
                Description = "<p>قالب وردپرس آماده برای راه‌اندازی فروشگاه آنلاین</p>",
                Price = 2_500_000,
                DiscountPrice = 1_900_000,
                MainImageUrl = "/images/products/mo.png",
                Brand = "TechnoShop",
                CategoryId = digital.Id,
                IsFeatured = true,
                IsActive = true,
                DigitalFileName = "sample-product.zip",
                DigitalFileOriginalName = "shop-theme.zip"
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Seed product images for phone products
        foreach (var product in products.Where(p => p.Slug.Contains("iphone") || p.Slug.Contains("galaxy")))
        {
            context.ProductImages.AddRange(
                new ProductImage { ProductId = product.Id, ImageUrl = "/images/singleproduct/a54.jfif", DisplayOrder = 1 },
                new ProductImage { ProductId = product.Id, ImageUrl = "/images/singleproduct/apple-16-blue.jfif", DisplayOrder = 2 },
                new ProductImage { ProductId = product.Id, ImageUrl = "/images/singleproduct/apple-16.jfif", DisplayOrder = 3 },
                new ProductImage { ProductId = product.Id, ImageUrl = "/images/singleproduct/apple-pro.jfif", DisplayOrder = 4 }
            );
        }

        // Seed articles
        context.Articles.Add(new Article
        {
            Title = "راهنمای خرید گوشی موبایل",
            Slug = "mobile-buying-guide",
            Summary = "نکات مهم برای انتخاب گوشی مناسب",
            Content = "<p>در این مقاله نکات مهم خرید گوشی موبایل را بررسی می‌کنیم.</p><p>هنگام خرید گوشی به پردازنده، رم، دوربین و باتری توجه کنید.</p>",
            ImageUrl = "/images/articles/article-1.jpg",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // Create sample digital product file
        var sampleZip = Path.Combine("App_Data", "DigitalProducts", "sample-product.zip");
        if (!File.Exists(sampleZip))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(sampleZip)!);
            // Write a minimal valid zip (empty zip = PK\x05\x06 + 18 zeros)
            var emptyZip = new byte[] {
                0x50, 0x4B, 0x05, 0x06,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00
            };
            await File.WriteAllBytesAsync(sampleZip, emptyZip);
        }
    }

    private static async Task ApplyMigrationsAsync(ApplicationDbContext context)
    {
        var pending = await context.Database.GetPendingMigrationsAsync();
        if (!pending.Any())
            return;

        try
        {
            await context.Database.MigrateAsync();
        }
        catch (SqlException ex) when (ex.Number == 2714)
        {
            await BaselineExistingSchemaAsync(context);
        }
    }

    private static async Task BaselineExistingSchemaAsync(ApplicationDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync($@"
            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
            CREATE TABLE [__EFMigrationsHistory] (
                [MigrationId] nvarchar(150) NOT NULL,
                [ProductVersion] nvarchar(32) NOT NULL,
                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
            );
            IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'{InitialMigrationId}')
            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'{InitialMigrationId}', N'8.0.27');
        ");
    }
}
