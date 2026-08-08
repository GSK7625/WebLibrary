using Library.Domain.Entities;

namespace Library.DataAccess.Interfaces;

public interface IBorrowRecordRepository
{
    Task<IEnumerable<BorrowRecord>> GetAllAsync();
    Task<BorrowRecord?> GetByIdAsync(int id);
    Task<BorrowRecord?> GetActiveBorrowRecordByBookIdAsync(int bookId);
    Task AddAsync(BorrowRecord record);
    Task UpdateAsync(BorrowRecord record);
}
