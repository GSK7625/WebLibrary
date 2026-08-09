using Library.Business.Interfaces;
using Library.DataAccess.Persistence;
using Library.DataAccess.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=library.db";
        services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBorrowRecordRepository, BorrowRecordRepository>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        return services;
    }
}
