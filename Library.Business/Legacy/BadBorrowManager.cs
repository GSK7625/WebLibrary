using Library.DataAccess.Entities;
using Library.DataAccess.Enums;
using Library.DataAccess.Interfaces;

namespace Library.Business.Legacy;

/// <summary>
/// CLASS VI PHAM SRP (Single Responsibility Principle) - DANH CHO DEMO
/// Class nay om dom toi 5 trach nhiem khac nhau trong cung 1 noi:
/// 1. Validation quy tac muon/tra
/// 2. Thao tac va cap nhat du lieu (Persistence)
/// 3. Tinh toan tien phat (Business Logic - Om luan switch-case)
/// 4. Dinh dang van ban hoa don/thong bao (Formatting)
/// 5. Ghi log he thong (Logging)
/// -> Co 5 LY DO DE THAY DOI class nay (Vi pham nghiem trong SRP)!
/// </summary>
public class BadBorrowManager
{
    private readonly IBorrowRecordRepository _borrowRepository;
    private readonly IBookRepository _bookRepository;

    public BadBorrowManager(
        IBorrowRecordRepository borrowRepository,
        IBookRepository bookRepository)
    {
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
    }

    public async Task<object> ReturnBookBadSRPAsync(int borrowRecordId, DateTime? actualReturnedDate = null)
    {
        // TRACH NHIEM 1: Validation du lieu & quy tac muon tra
        var record = await _borrowRepository.GetByIdAsync(borrowRecordId);
        if (record is null)
        {
            throw new KeyNotFoundException($"[SRP Violation] Khong tim thay phieu muon Id = {borrowRecordId}");
        }

        if (record.ReturnedDate.HasValue)
        {
            throw new InvalidOperationException($"[SRP Violation] Phieu muon Id = {borrowRecordId} da tra truoc do vao ngay {record.ReturnedDate.Value:dd/MM/yyyy HH:mm}.");
        }

        var returnedDate = actualReturnedDate ?? DateTime.Now;
        if (returnedDate < record.BorrowDate)
        {
            throw new ArgumentException("[SRP Violation] Ngay tra sach khong duoc nho hon ngay muon sach.");
        }

        var book = await _bookRepository.GetByIdAsync(record.BookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"[SRP Violation] Khong tim thay sach lien ket Id = {record.BookId}");
        }

        // TRACH NHIEM 2: Business Logic - Tinh so ngay tre & Switch-case tinh tien phat
        int daysLate = 0;
        if (returnedDate > record.DueDate)
        {
            daysLate = (int)Math.Ceiling((returnedDate - record.DueDate).TotalDays);
        }

        decimal lateFee = 0;
        if (daysLate > 0)
        {
            // Tu tinh tien phat bang switch-case (om luan trach nhiem tinh toan cua Strategy Pattern)
            switch (book.Type)
            {
                case BookType.Regular: lateFee = daysLate * 2000m; break;
                case BookType.Rare: lateFee = daysLate * 10000m; break;
                case BookType.Textbook: lateFee = daysLate * 3000m; break;
                case BookType.Magazine: lateFee = daysLate * 1000m; break;
                case BookType.Foreign: lateFee = daysLate * 15000m; break;
                default: lateFee = daysLate * 2000m; break;
            }
        }

        // TRACH NHIEM 3: Persistence - Truc tiep cap nhat trang thai du lieu
        record.ReturnedDate = returnedDate;
        record.LateFee = lateFee;
        book.IsBorrowed = false;

        await _borrowRepository.UpdateAsync(record);

        // TRACH NHIEM 4: Formatting - Tu dinh dang chuoi van ban hoa don xac nhan tra sach
        string receiptText = $"=== HOA DON XAC NHAN TRA SACH (VI PHAM SRP) ===\n" +
                             $"Phieu muon: #{record.Id}\n" +
                             $"Nguoi muon: {record.BorrowerName}\n" +
                             $"Sach: {book.Title} ({book.Type})\n" +
                             $"Ngay muon: {record.BorrowDate:dd/MM/yyyy HH:mm}\n" +
                             $"Han tra: {record.DueDate:dd/MM/yyyy HH:mm}\n" +
                             $"Ngay tra thuc te: {returnedDate:dd/MM/yyyy HH:mm}\n" +
                             $"So ngay tre: {daysLate} ngay\n" +
                             $"Phi phat tra han: {lateFee:N0} VND\n" +
                             $"===============================================";

        // TRACH NHIEM 5: Logging / Notification - Tu ghi log he thong
        Console.WriteLine($"[LOG BAD SRP] Xu ly tra sach phieu #{record.Id} luc {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine(receiptText);

        return new
        {
            BorrowRecordId = record.Id,
            BookTitle = book.Title,
            BorrowerName = record.BorrowerName,
            DaysLate = daysLate,
            LateFee = lateFee,
            ReceiptText = receiptText,
            Method = "BadBorrowManager (VI PHAM SRP)",
            SrpViolationReason = "1 Class duy nhat om 5 trach nhiem: Validation + Persistence + Switch-case Calculation + Invoice Formatting + Console Logging"
        };
    }
}
