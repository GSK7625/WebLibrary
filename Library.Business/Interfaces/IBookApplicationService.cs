using Library.Business.DTOs;

namespace Library.Business.Interfaces;

public interface IBookApplicationService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<FeePreviewDto> PreviewFeeAsync(int bookId, int daysLate);
    Task<FeePreviewDto> PreviewLegacyFeeAsync(int bookId, int daysLate);
}
