using Library.Business.DTOs;
using Library.Business.Services.Interfaces;
using Library.DataAccess.Interfaces;
using Library.Domain.Entities;

namespace Library.Business.Services;

public class BorrowApplicationService : IBorrowApplicationService
{
    private readonly IBorrowRecordRepository _borrowRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILateFeeApplicationService _lateFeeService;

    public BorrowApplicationService(
        IBorrowRecordRepository borrowRepository,
        IBookRepository bookRepository,
        ILateFeeApplicationService lateFeeService)
    {
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
        _lateFeeService = lateFeeService;
    }

    public async Task<IEnumerable<BorrowRecordDto>> GetAllBorrowRecordsAsync()
    {
        var records = await _borrowRepository.GetAllAsync();
        return records.Select(r => new BorrowRecordDto
        {
            Id = r.Id,
            BookId = r.BookId,
            BookTitle = r.Book?.Title ?? "N/A",
            BorrowerName = r.BorrowerName,
            BorrowDate = r.BorrowDate,
            DueDate = r.DueDate,
            ReturnedDate = r.ReturnedDate,
            LateFee = r.LateFee
        });
    }

    public async Task<BorrowRecordDto> BorrowBookAsync(BorrowRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.BorrowerName))
        {
            throw new ArgumentException("Tên người mượn không được để trống.");
        }

        if (request.BorrowDays <= 0)
        {
            throw new ArgumentException("Số ngày mượn phải lớn hơn 0.");
        }

        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sách có Id = {request.BookId}");
        }

        if (book.IsBorrowed)
        {
            throw new InvalidOperationException($"Sách '{book.Title}' đang được mượn, không thể mượn tiếp.");
        }

        var borrowDate = DateTime.Now;
        var dueDate = borrowDate.AddDays(request.BorrowDays);

        var record = new BorrowRecord
        {
            BookId = book.Id,
            BorrowerName = request.BorrowerName.Trim(),
            BorrowDate = borrowDate,
            DueDate = dueDate
        };

        book.IsBorrowed = true;

        await _borrowRepository.AddAsync(record);

        return new BorrowRecordDto
        {
            Id = record.Id,
            BookId = record.BookId,
            BookTitle = book.Title,
            BorrowerName = record.BorrowerName,
            BorrowDate = record.BorrowDate,
            DueDate = record.DueDate,
            ReturnedDate = record.ReturnedDate,
            LateFee = record.LateFee
        };
    }

    public async Task<ReturnBookResponseDto> ReturnBookByBorrowRecordIdAsync(int borrowRecordId, DateTime? actualReturnedDate = null)
    {
        var record = await _borrowRepository.GetByIdAsync(borrowRecordId);
        if (record is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy phiếu mượn có Id = {borrowRecordId}");
        }

        if (record.ReturnedDate.HasValue)
        {
            throw new InvalidOperationException($"Phiếu mượn Id = {borrowRecordId} đã được trả trước đó vào ngày {record.ReturnedDate.Value:dd/MM/yyyy HH:mm}.");
        }

        var returnedDate = actualReturnedDate ?? DateTime.Now;
        if (returnedDate < record.BorrowDate)
        {
            throw new ArgumentException("Ngày trả sách không được nhỏ hơn ngày mượn sách.");
        }

        var book = await _bookRepository.GetByIdAsync(record.BookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sách liên kết với phiếu mượn (BookId = {record.BookId})");
        }

        // Tính số ngày trễ
        int daysLate = 0;
        if (returnedDate > record.DueDate)
        {
            daysLate = (int)Math.Ceiling((returnedDate - record.DueDate).TotalDays);
        }

        // Tính phí bằng LateFeeApplicationService (Strategy Pattern OCP)
        decimal lateFee = _lateFeeService.CalculateFee(book.Type, daysLate);

        record.ReturnedDate = returnedDate;
        record.LateFee = lateFee;
        book.IsBorrowed = false;

        await _borrowRepository.UpdateAsync(record);

        return new ReturnBookResponseDto
        {
            BorrowRecordId = record.Id,
            BookTitle = book.Title,
            BorrowerName = record.BorrowerName,
            BorrowDate = record.BorrowDate,
            DueDate = record.DueDate,
            ReturnedDate = returnedDate,
            DaysLate = daysLate,
            LateFee = lateFee,
            FeeCalculationMethod = "Strategy Pattern (OCP)",
            Message = daysLate > 0
                ? $"Trả sách trễ {daysLate} ngày. Phí trả hạn là {lateFee:N0} VNĐ ({book.Type})"
                : "Trả sách đúng hạn. Không phát sinh phí trả hạn."
        };
    }
}
