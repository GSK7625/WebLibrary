using Library.DataAccess.Interfaces;

namespace Library.DataAccess.Persistence;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly LibraryDbContext _context;

    public DatabaseInitializer(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(bool forceRecreate = false)
    {
        await DbInitializer.SeedDataAsync(_context, forceRecreate);
    }
}
