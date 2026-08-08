using Library.Business.DTOs;

namespace Library.Business.Services.Interfaces;

public interface IBorrowApplicationService
{
    Task<IEnumerable<BorrowRecordDto>> GetAllBorrowRecordsAsync();
    Task<BorrowRecordDto> BorrowBookAsync(BorrowRequestDto request);
    Task<ReturnBookResponseDto> ReturnBookByBorrowRecordIdAsync(int borrowRecordId, DateTime? actualReturnedDate = null);
}
