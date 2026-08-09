using Library.Business.DTOs;
using Library.Business.Legacy;
using Library.Business.Interfaces;
using Library.Business.Entities;

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
            throw new KeyNotFoundException($"Khong tim thay sach co Id = {bookId}");
        }

        var fee = _lateFeeService.CalculateFee(book.Type, daysLate);

        return new FeePreviewDto
        {
            BookId = book.Id,
            BookTitle = book.Title,
            BookType = book.Type.ToString(),
            DaysLate = daysLate,
            Fee = fee,
            Method = "Strategy Pattern + DI (Tuan thu OCP)",
            Note = "De dang them loai sach moi ma KHONG SUA code tinh phi hien tai!"
        };
    }

    public async Task<FeePreviewDto> PreviewLegacyFeeAsync(int bookId, int daysLate)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Khong tim thay sach co Id = {bookId}");
        }

        var fee = _legacyFeeCalculator.CalculateLateFee(book, daysLate);

        return new FeePreviewDto
        {
            BookId = book.Id,
            BookTitle = book.Title,
            BookType = book.Type.ToString(),
            DaysLate = daysLate,
            Fee = fee,
            Method = "Switch-Case Legacy (Vi pham OCP)",
            Note = "Muon them loai sach moi buoc phai SUA CODE switch-case trong LegacyFeeCalculator!"
        };
    }
}
