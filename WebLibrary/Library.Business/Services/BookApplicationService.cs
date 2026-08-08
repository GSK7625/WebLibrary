using Library.Business.DTOs;
using Library.Business.Legacy;
using Library.Business.Services.Interfaces;
using Library.DataAccess.Interfaces;
using Library.Domain.Entities;

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
        return books.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            Type = b.Type,
            IsBorrowed = b.IsBorrowed
        });
    }

    public async Task<FeePreviewDto> PreviewFeeAsync(int bookId, int daysLate)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sách có Id = {bookId}");
        }

        var fee = _lateFeeService.CalculateFee(book.Type, daysLate);

        return new FeePreviewDto
        {
            BookId = book.Id,
            BookTitle = book.Title,
            BookType = book.Type.ToString(),
            DaysLate = daysLate,
            Fee = fee,
            Method = "Strategy Pattern + DI (Tuân thủ OCP)",
            Note = "Dễ dàng thêm loại sách mới mà KHÔNG SỬA code tính phí hiện tại!"
        };
    }

    public async Task<FeePreviewDto> PreviewLegacyFeeAsync(int bookId, int daysLate)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sách có Id = {bookId}");
        }

        var fee = _legacyFeeCalculator.CalculateLateFee(book, daysLate);

        return new FeePreviewDto
        {
            BookId = book.Id,
            BookTitle = book.Title,
            BookType = book.Type.ToString(),
            DaysLate = daysLate,
            Fee = fee,
            Method = "Switch-Case Legacy (Vi phạm OCP)",
            Note = "Muốn thêm loại sách mới buộc phải SỬA CODE switch-case trong LegacyFeeCalculator!"
        };
    }
}
