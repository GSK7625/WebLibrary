using Library.Business.Legacy;
using Library.Business.Services;
using Library.Business.Services.Interfaces;
using Library.Business.Strategies;
using Library.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Tự động đăng ký các service của DataAccess
        services.AddDataAccessServices(configuration);

        // Register OCP Strategies via Dependency Injection
        services.AddScoped<ILateFeeStrategy, RegularBookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, RareBookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, TextbookFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, MagazineFeeStrategy>();
        services.AddScoped<ILateFeeStrategy, ForeignBookFeeStrategy>();

        // Register Legacy Calculator (phục vụ đối chiếu demo OCP & SRP)
        services.AddSingleton<LegacyFeeCalculator>();
        services.AddScoped<BadBorrowManager>();

        // Register Application Services với Interfaces tương ứng
        services.AddScoped<ILateFeeApplicationService, LateFeeApplicationService>();
        services.AddScoped<IBookApplicationService, BookApplicationService>();
        services.AddScoped<IBorrowApplicationService, BorrowApplicationService>();

        return services;
    }
}
