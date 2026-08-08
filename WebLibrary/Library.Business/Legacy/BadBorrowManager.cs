using Library.DataAccess.Interfaces;
using Library.Domain.Enums;

namespace Library.Business.Legacy;

/// <summary>
/// CLASS VI PH?M SRP (Single Responsibility Principle) - D�NH CHO TV3 DEMO
/// Class n�y �m d?m t?i 5 tr�ch nhi?m kh�c nhau trong c�ng 1 noi:
/// 1. Validation quy t?c mu?n/tr?
/// 2. Thao t�c v� c?p nh?t d? li?u (Persistence)
/// 3. T�nh to�n ti?n ph?t (Business Logic - �m lu�n switch-case)
/// 4. �?nh d?ng van b?n h�a don/th�ng b�o (Formatting)
/// 5. Ghi log h? th?ng (Logging)
/// ? C� 5 L� DO �? THAY �?I class n�y (Vi ph?m nghi�m tr?ng SRP)!
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
        // ? TR�CH NHI?M 1: Validation d? li?u & quy t?c mu?n tr?
        var record = await _borrowRepository.GetByIdAsync(borrowRecordId);
        if (record is null)
        {
            throw new KeyNotFoundException($"[SRP Violation] Kh�ng t�m th?y phi?u mu?n Id = {borrowRecordId}");
        }

        if (record.ReturnedDate.HasValue)
        {
            throw new InvalidOperationException($"[SRP Violation] Phi?u mu?n Id = {borrowRecordId} d� tr? tru?c d� v�o ng�y {record.ReturnedDate.Value:dd/MM/yyyy HH:mm}.");
        }

        var returnedDate = actualReturnedDate ?? DateTime.Now;
        if (returnedDate < record.BorrowDate)
        {
            throw new ArgumentException("[SRP Violation] Ng�y tr? s�ch kh�ng du?c nh? hon ng�y mu?n s�ch.");
        }

        var book = await _bookRepository.GetByIdAsync(record.BookId);
        if (book is null)
        {
            throw new KeyNotFoundException($"[SRP Violation] Kh�ng t�m th?y s�ch li�n k?t Id = {record.BookId}");
        }

        // ? TR�CH NHI?M 2: Business Logic - T�nh s? ng�y tr? & Switch-case t�nh ti?n ph?t
        int daysLate = 0;
        if (returnedDate > record.DueDate)
        {
            daysLate = (int)Math.Ceiling((returnedDate - record.DueDate).TotalDays);
        }

        decimal lateFee = 0;
        if (daysLate > 0)
        {
            // T? t�nh ti?n ph?t b?ng switch-case (�m lu�n tr�ch nhi?m t�nh to�n c?a Strategy Pattern)
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

        // ? TR�CH NHI?M 3: Persistence - Tr?c ti?p c?p nh?t tr?ng th�i d? li?u
        record.ReturnedDate = returnedDate;
        record.LateFee = lateFee;
        book.IsBorrowed = false;

        await _borrowRepository.UpdateAsync(record);

        // ? TR�CH NHI?M 4: Formatting - T? d?nh d?ng chu?i van b?n h�a don x�c nh?n tr? s�ch
        string receiptText = $"=== H�A �ON X�C NH?N TR? S�CH (VI PH?M SRP) ===\n" +
                             $"Phi?u mu?n: #{record.Id}\n" +
                             $"Ngu?i mu?n: {record.BorrowerName}\n" +
                             $"S�ch: {book.Title} ({book.Type})\n" +
                             $"Ng�y mu?n: {record.BorrowDate:dd/MM/yyyy HH:mm}\n" +
                             $"H?n tr?: {record.DueDate:dd/MM/yyyy HH:mm}\n" +
                             $"Ng�y tr? th?c t?: {returnedDate:dd/MM/yyyy HH:mm}\n" +
                             $"S? ng�y tr?: {daysLate} ng�y\n" +
                             $"Ph� ph?t tr? h?n: {lateFee:N0} VN�\n" +
                             $"===============================================";

        // ? TR�CH NHI?M 5: Logging / Notification - T? ghi log h? th?ng
        Console.WriteLine($"[LOG BAD SRP] X? l� tr? s�ch phi?u #{record.Id} l�c {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine(receiptText);

        return new
        {
            BorrowRecordId = record.Id,
            BookTitle = book.Title,
            BorrowerName = record.BorrowerName,
            DaysLate = daysLate,
            LateFee = lateFee,
            ReceiptText = receiptText,
            Method = "BadBorrowManager (VI PH?M SRP)",
            SrpViolationReason = "1 Class duy nh?t �m 5 tr�ch nhi?m: Validation + Persistence + Switch-case Calculation + Invoice Formatting + Console Logging"
        };
    }
}

