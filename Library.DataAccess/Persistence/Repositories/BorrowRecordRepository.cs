using Library.DataAccess.Entities;
using Library.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.DataAccess.Persistence.Repositories;

public class BorrowRecordRepository : IBorrowRecordRepository
{
    private readonly LibraryDbContext _context;

    public BorrowRecordRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BorrowRecord>> GetAllAsync()
    {
        return await _context.BorrowRecords
            .Include(r => r.Book)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<BorrowRecord?> GetByIdAsync(int id)
    {
        return await _context.BorrowRecords
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<BorrowRecord?> GetActiveBorrowRecordByBookIdAsync(int bookId)
    {
        return await _context.BorrowRecords
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.ReturnedDate == null);
    }

    public async Task AddAsync(BorrowRecord record)
    {
        await _context.BorrowRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(BorrowRecord record)
    {
        _context.BorrowRecords.Update(record);
        await _context.SaveChangesAsync();
    }
}
