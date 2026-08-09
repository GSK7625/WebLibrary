using Library.Business.DTOs;

namespace Library.Business.Interfaces;

public interface IBorrowApplicationService
{
    Task<IEnumerable<BorrowRecordDto>> GetAllBorrowRecordsAsync();
    Task<BorrowRecordDto> BorrowBookAsync(BorrowRequestDto request);
    Task<ReturnBookResponseDto> ReturnBookByBorrowRecordIdAsync(int borrowRecordId, DateTime? actualReturnedDate = null);
}
