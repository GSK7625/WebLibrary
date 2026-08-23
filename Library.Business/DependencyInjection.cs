using Library.Business.Interfaces;
using Library.Business.Legacy;
using Library.Business.Services;
using Library.Business.Strategies;
using Library.DataAccess;
using Library.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Library.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký toàn bộ dịch vụ của tầng Data Access bên dưới (DAL)
        services.AddDataAccessServices(configuration);

        // Register Advanced OCP Strategies via Dependency Injection
        services.AddScoped<ILateFeeStrategy, StaffExemptionFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, VIPMemberFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, StudentTextbookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, RareBookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, ForeignBookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, MagazineFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, TextbookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, RegularBookFeeStrategy>();

        // Register Legacy Demos (phục vụ đối chiếu demo SOLID)
        services.AddSingleton<LegacyFeeCalculator>();
        services.AddScoped<BadBorrowManager>();
        services.AddScoped<LspViolationProcessor>();
        services.AddScoped<BadGuestKioskClient>();
        services.AddScoped<BadBorrowNotificationManager>();

        // Register Application Services với Interfaces tương ứng (SRP & OCP)
        services.AddScoped<ILateFeeApplicationService, LateFeeApplicationService>();
        services.AddScoped<IBookApplicationService, BookApplicationService>();
        services.AddScoped<IBorrowApplicationService, BorrowApplicationService>();

        // Register LSP Services
        services.AddScoped<Library.Business.Lsp.LspCleanBorrowProcessor>();

        // Register ISP Services
        services.AddScoped<Library.Business.Isp.IBookSearchService, Library.Business.Isp.CleanGuestKioskService>();
        services.AddScoped<Library.Business.Isp.CleanSelfCheckoutStation>();
        services.AddScoped<Library.Business.Isp.CleanLibrarianInventoryService>();

        // Register DIP Services (Abstractions & Concrete Implementations)
        services.AddSingleton<Library.Business.Dip.IAuditLogger, Library.Business.Dip.InMemoryAuditLogger>();
        services.AddScoped<Library.Business.Dip.INotificationSender, Library.Business.Dip.EmailNotificationSender>();
        services.AddScoped<Library.Business.Dip.INotificationSender, Library.Business.Dip.SmsNotificationSender>();
        services.AddScoped<Library.Business.Dip.INotificationSender, Library.Business.Dip.ZaloNotificationSender>();
        services.AddScoped<Library.Business.Dip.IBorrowNotificationService, Library.Business.Dip.BorrowNotificationApplicationService>();

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
                logger?.LogError(ex, "Lỗi xảy ra khi khởi tạo dữ liệu Database!");
            }
        }
        return services;
    }
}
