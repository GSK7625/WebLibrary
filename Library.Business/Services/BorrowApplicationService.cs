using Library.Business.DTOs;
using Library.Business.Interfaces;
using Library.Business.Entities;

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
            throw new ArgumentException("Ten nguoi muon khong duoc de trong.");
        }

        if (request.BorrowDays <= 0)
        {
            throw new ArgumentException("So ngay muon phai lon hon 0.");
        }

        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Khong tim thay sach co Id = {request.BookId}");
        }

        if (book.IsBorrowed)
        {
            throw new InvalidOperationException($"Sach '{book.Title}' dang duoc muon, khong the muon tiep.");
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

    public async Task<ReturnBookResponseDto> ReturnBookByBorrowRecordIdAsync(int borrowRecordId, ReturnBookRequestDto? request = null)
    {
        var record = await _borrowRepository.GetByIdAsync(borrowRecordId);
        if (record is null)
        {
            throw new KeyNotFoundException($"Khong tim thay phieu muon co Id = {borrowRecordId}");
        }

        if (record.ReturnedDate.HasValue)
        {
            throw new InvalidOperationException($"Phieu muon Id = {borrowRecordId} da duoc tra truoc do vao ngay {record.ReturnedDate.Value:dd/MM/yyyy HH:mm}.");
        }

        var returnedDate = request?.ReturnedDate ?? DateTime.Now;
        if (returnedDate < record.BorrowDate)
        {
            throw new ArgumentException("Ngay tra sach khong duoc nho hon ngay muon sach.");
        }

        var book = await _bookRepository.GetByIdAsync(record.BookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"Khong tim thay sach lien ket voi phieu muon (BookId = {record.BookId})");
        }

        // Tinh so ngay tre
        int daysLate = 0;
        if (returnedDate > record.DueDate)
        {
            daysLate = (int)Math.Ceiling((returnedDate - record.DueDate).TotalDays);
        }

        var memberType = request?.MemberType ?? Enums.MemberType.Standard;

        // Tinh phi bang LateFeeApplicationService (Strategy Pattern OCP)
        var feeResult = _lateFeeService.CalculateFee(book, daysLate, memberType);

        record.ReturnedDate = returnedDate;
        record.LateFee = feeResult.FinalFee;
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
            BaseFee = feeResult.BaseFee,
            DiscountAmount = feeResult.DiscountAmount,
            LateFee = feeResult.FinalFee,
            FeeCalculationMethod = $"Strategy Pattern - {feeResult.StrategyName} (OCP)",
            AppliedRules = feeResult.AppliedRules,
            Message = daysLate > 0
                ? $"Tra sach tre {daysLate} ngay. Phi tra han la {feeResult.FinalFee:N0} VND ({book.Type})"
                : "Tra sach dung han. Khong phat sinh phi tra han."
        };
    }
}
