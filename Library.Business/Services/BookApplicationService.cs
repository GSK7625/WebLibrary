using Library.Business.DTOs;
using Library.Business.Interfaces;
using Library.Business.Legacy;
using Library.Business.Models;
using Library.DataAccess.Entities;
using Library.DataAccess.Enums;
using Library.DataAccess.Interfaces;

namespace Library.Business.Services;

public class BookApplicationService : IBookApplicationService
{
    private readonly IBookRepository _bookRepository;
    private readonly ILateFeeApplicationService _lateFeeService;
    private readonly LegacyFeeCalculator _legacyFeeCalculator;

    public BookApplicationService(
        IBookRepository bookRepository,
        ILateFeeApplicationService lateFeeService,
        LegacyFeeCalculator legacyFeeCalculator)
    {
        _bookRepository = bookRepository;
        _lateFeeService = lateFeeService;
        _legacyFeeCalculator = legacyFeeCalculator;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(b => MapToDto(b));
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        return book is null ? null : MapToDto(book);
    }

    public async Task<FeePreviewDto> PreviewFeeAsync(int bookId, int daysLate, MemberType memberType = MemberType.Standard)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sách có Id = {bookId}");
        }

        var result = _lateFeeService.CalculateFee(book, daysLate, memberType);

        return new FeePreviewDto
        {
            BookId = book.Id,
            BookTitle = book.Title,
            BookType = book.Type.ToString(),
            BasePrice = book.BasePrice,
            MemberType = memberType,
            DaysLate = daysLate,
            BaseFee = result.BaseFee,
            DiscountAmount = result.DiscountAmount,
            FinalFee = result.FinalFee,
            StrategyName = result.StrategyName,
            AppliedRules = result.AppliedRules,
            Method = "Advanced Strategy Pattern + Dynamic Predicate (Tuân thủ OCP)",
            Note = "Dễ dàng thêm chính sách mới bằng cách tạo class Strategy mới mà KHÔNG CẦN SỬA CODE CŨ!"
        };
    }

    public async Task<FeePreviewDto> PreviewLegacyFeeAsync(int bookId, int daysLate, MemberType memberType = MemberType.Standard)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sách có Id = {bookId}");
        }

        var result = _legacyFeeCalculator.CalculateLateFeeLegacy(book, daysLate, memberType);

        return new FeePreviewDto
        {
            BookId = book.Id,
            BookTitle = book.Title,
            BookType = book.Type.ToString(),
            BasePrice = book.BasePrice,
            MemberType = memberType,
            DaysLate = daysLate,
            BaseFee = result.BaseFee,
            DiscountAmount = result.DiscountAmount,
            FinalFee = result.FinalFee,
            StrategyName = result.StrategyName,
            AppliedRules = result.AppliedRules,
            Method = "Monolithic Switch-Case & If-Else (Vi phạm OCP)",
            Note = "Muốn thêm chính sách mới BẮT BUỘC PHẢI SỬA CODE CŨ trong LegacyFeeCalculator, dễ gây bug!"
        };
    }

    private static BookDto MapToDto(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        ISBN = b.ISBN,
        Type = b.Type,
        BasePrice = b.BasePrice,
        IsBorrowed = b.IsBorrowed
    };
}
