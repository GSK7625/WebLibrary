using Library.Business.Interfaces;
using System.Net.Http;

namespace Library.DataAccess.Persistence;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly LibraryDbContext _context;
    private readonly IHttpClientFactory? _httpClientFactory;

    public DatabaseInitializer(
        LibraryDbContext context,
        IHttpClientFactory? httpClientFactory = null)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InitializeAsync(bool forceRecreate = false)
    {
        var httpClient = _httpClientFactory?.CreateClient();
        await DbInitializer.SeedDataAsync(_context, httpClient, forceRecreate);
    }
}
