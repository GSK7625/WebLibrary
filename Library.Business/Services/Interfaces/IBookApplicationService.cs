using Library.Business.DTOs;

namespace Library.Business.Services.Interfaces;

public interface IBookApplicationService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<FeePreviewDto> PreviewFeeAsync(int bookId, int daysLate);
    Task<FeePreviewDto> PreviewLegacyFeeAsync(int bookId, int daysLate);
}
