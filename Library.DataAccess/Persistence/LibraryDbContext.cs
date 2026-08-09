using Library.Business.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.DataAccess.Persistence;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
}
