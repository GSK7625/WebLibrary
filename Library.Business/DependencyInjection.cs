using Library.Business.Interfaces;
using Library.Business.Legacy;
using Library.Business.Services;
using Library.Business.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Library.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register OCP Strategies via Dependency Injection
        services.AddScoped<ILateFeeStrategy, RegularBookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, RareBookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, TextbookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, MagazineFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, ForeignBookFeeStrategy>();

        // Register Legacy Calculator (phuc vu doi chieu demo OCP & SRP)
        services.AddSingleton<LegacyFeeCalculator>();
        services.AddScoped<BadBorrowManager>();

        // Register Application Services voi Interfaces tuong ung
        services.AddScoped<ILateFeeApplicationService, LateFeeApplicationService>();
        services.AddScoped<IBookApplicationService, BookApplicationService>();
        services.AddScoped<IBorrowApplicationService, BorrowApplicationService>();

        return services;
    }

    public static async Task<IServiceProvider> InitializeDatabaseAsync(this IServiceProvider services, bool forceRecreate = false)
    {
        using (var scope = services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            try
            {
                var dbInitializer = sp.GetRequiredService<IDatabaseInitializer>();
                await dbInitializer.InitializeAsync(forceRecreate);
            }
            catch (Exception ex)
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("DatabaseInitializer");
                logger?.LogError(ex, "Loi xay ra khi khoi tao du lieu Database!");
            }
        }
        return services;
    }
}
