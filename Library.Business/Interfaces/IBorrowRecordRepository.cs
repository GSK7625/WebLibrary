using Library.Business.Entities;

namespace Library.Business.Interfaces;

public interface IBorrowRecordRepository
{
    Task<IEnumerable<BorrowRecord>> GetAllAsync();
    Task<BorrowRecord?> GetByIdAsync(int id);
    Task<BorrowRecord?> GetActiveBorrowRecordByBookIdAsync(int bookId);
    Task AddAsync(BorrowRecord record);
    Task UpdateAsync(BorrowRecord record);
}
