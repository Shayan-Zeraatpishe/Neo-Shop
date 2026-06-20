using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Interfaces;
using Shop.Application.Services;
using Shop.Infrastructure.Data;
using Shop.Infrastructure.Identity;
using Shop.Infrastructure.Repositories;
using Shop.Infrastructure.Services;
using Shop.Application.DTOs;

namespace Shop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IDownloadTokenRepository, DownloadTokenRepository>();
        services.AddHttpClient();
        services.AddScoped<IPaymentService, ZarinPalPaymentService>();

        var digitalPath = configuration["DigitalProducts:StoragePath"] ?? "App_Data/DigitalProducts";
        services.AddScoped<IDownloadService>(sp => new DownloadService(
            sp.GetRequiredService<IDownloadTokenRepository>(),
            sp.GetRequiredService<IOrderRepository>(),
            sp.GetRequiredService<IProductRepository>(),
            digitalPath));

        services.Configure<OllamaSettings>(
    configuration.GetSection("Ollama"));

        services.AddScoped<ICommentModerationService,
                   CommentModerationService>();

        return services;
    }
}
