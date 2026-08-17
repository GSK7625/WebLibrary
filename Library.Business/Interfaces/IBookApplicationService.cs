using Library.Business.DTOs;
using Library.Business.Enums;

namespace Library.Business.Interfaces;

public interface IBookApplicationService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<FeePreviewDto> PreviewFeeAsync(int bookId, int daysLate, MemberType memberType = MemberType.Standard);
    Task<FeePreviewDto> PreviewLegacyFeeAsync(int bookId, int daysLate, MemberType memberType = MemberType.Standard);
}
