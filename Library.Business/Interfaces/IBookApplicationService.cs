using Library.Business.DTOs;
using Library.DataAccess.Enums;

namespace Library.Business.Interfaces;

public interface IBookApplicationService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<FeePreviewDto> PreviewFeeAsync(int bookId, int daysLate, MemberType memberType = MemberType.Standard);
    Task<FeePreviewDto> PreviewLegacyFeeAsync(int bookId, int daysLate, MemberType memberType = MemberType.Standard);
}
