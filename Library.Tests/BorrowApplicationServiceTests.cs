using Library.Business.DTOs;
using Library.Business.Interfaces;
using Library.Business.Models;
using Library.Business.Services;
using Library.DataAccess.Entities;
using Library.DataAccess.Enums;
using Library.DataAccess.Interfaces;
using Xunit;

namespace Library.Tests;

public class BorrowApplicationServiceTests
{
    [Fact]
    public async Task BorrowBookAsync_ShouldUpdateBookState_AndAddRecord()
    {
        // ARRANGE
        var bookRepo = new FakeBookRepository();
        var borrowRepo = new FakeBorrowRecordRepository();
        var lateFeeService = new FakeLateFeeService();

        var book = new Book
        {
            Id = 1,
            Title = "Design Patterns",
            Type = BookType.Regular,
            BasePrice = 100000m,
            IsBorrowed = false
        };
        await bookRepo.AddAsync(book);

        var service = new BorrowApplicationService(borrowRepo, bookRepo, lateFeeService);

        var request = new BorrowRequestDto
        {
            BookId = 1,
            BorrowerName = "Đỗ Trung Kiên",
            BorrowDays = 14
        };

        // ACT
        var result = await service.BorrowBookAsync(request);

        // ASSERT
        Assert.NotNull(result);
        Assert.True(book.IsBorrowed);
        Assert.True(bookRepo.WasUpdateCalled, "Phải gọi tường minh _bookRepository.UpdateAsync(book)!");
        Assert.Single(borrowRepo.Records);
        Assert.Equal("Đỗ Trung Kiên", result.BorrowerName);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenBookAlreadyBorrowed_ShouldThrowInvalidOperationException()
    {
        // ARRANGE
        var bookRepo = new FakeBookRepository();
        var borrowRepo = new FakeBorrowRecordRepository();
        var lateFeeService = new FakeLateFeeService();

        var book = new Book
        {
            Id = 1,
            Title = "Clean Code",
            IsBorrowed = true // Đang được mượn
        };
        await bookRepo.AddAsync(book);

        var service = new BorrowApplicationService(borrowRepo, bookRepo, lateFeeService);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.BorrowBookAsync(new BorrowRequestDto
        {
            BookId = 1,
            BorrowerName = "Nguyễn Văn B",
            BorrowDays = 7
        }));
    }

    [Fact]
    public async Task ReturnBookAsync_ShouldUpdateBookStateToAvailable_AndSetLateFee()
    {
        // ARRANGE
        var bookRepo = new FakeBookRepository();
        var borrowRepo = new FakeBorrowRecordRepository();
        var lateFeeService = new FakeLateFeeService();

        var book = new Book
        {
            Id = 2,
            Title = "Refactoring",
            Type = BookType.Regular,
            IsBorrowed = true
        };
        await bookRepo.AddAsync(book);

        var record = new BorrowRecord
        {
            Id = 100,
            BookId = 2,
            BorrowerName = "Trần Thị C",
            BorrowDate = DateTime.Now.AddDays(-20),
            DueDate = DateTime.Now.AddDays(-6),
            ReturnedDate = null
        };
        await borrowRepo.AddAsync(record);

        var service = new BorrowApplicationService(borrowRepo, bookRepo, lateFeeService);

        // ACT
        var response = await service.ReturnBookByBorrowRecordIdAsync(100);

        // ASSERT
        Assert.False(book.IsBorrowed);
        Assert.True(bookRepo.WasUpdateCalled, "Phải gọi tường minh _bookRepository.UpdateAsync(book) khi trả sách!");
        Assert.NotNull(record.ReturnedDate);
        Assert.NotNull(record.LateFee);
        Assert.True(response.DaysLate > 0);
    }

    // --- Fake Repositories & Services để Unit Test nhanh và độc lập ---
    private class FakeBookRepository : IBookRepository
    {
        public readonly Dictionary<int, Book> Books = new();
        public bool WasUpdateCalled { get; private set; }

        public Task<IEnumerable<Book>> GetAllAsync() => Task.FromResult<IEnumerable<Book>>(Books.Values);
        public Task<Book?> GetByIdAsync(int id) => Task.FromResult(Books.GetValueOrDefault(id));
        public Task AddAsync(Book book) { Books[book.Id] = book; return Task.CompletedTask; }
        public Task UpdateAsync(Book book) { Books[book.Id] = book; WasUpdateCalled = true; return Task.CompletedTask; }
    }

    private class FakeBorrowRecordRepository : IBorrowRecordRepository
    {
        public readonly List<BorrowRecord> Records = new();

        public Task<IEnumerable<BorrowRecord>> GetAllAsync() => Task.FromResult<IEnumerable<BorrowRecord>>(Records);
        public Task<BorrowRecord?> GetByIdAsync(int id) => Task.FromResult(Records.FirstOrDefault(r => r.Id == id));
        public Task<BorrowRecord?> GetActiveBorrowRecordByBookIdAsync(int bookId) =>
            Task.FromResult(Records.FirstOrDefault(r => r.BookId == bookId && r.ReturnedDate == null));
        public Task AddAsync(BorrowRecord record) { Records.Add(record); return Task.CompletedTask; }
        public Task UpdateAsync(BorrowRecord record) => Task.CompletedTask;
    }

    private class FakeLateFeeService : ILateFeeApplicationService
    {
        public FeeCalculationResult CalculateFee(FeeCalculationContext context) => new()
        {
            StrategyName = "FakeStrategy",
            BaseFee = 10000m,
            DiscountAmount = 0m,
            FinalFee = 10000m,
            AppliedRules = new List<string> { "Rule test" }
        };

        public FeeCalculationResult CalculateFee(Book book, int daysLate, MemberType memberType = MemberType.Standard) =>
            CalculateFee(new FeeCalculationContext { Book = book, DaysLate = daysLate, MemberType = memberType });
    }
}
