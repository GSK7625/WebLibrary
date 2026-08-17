using Library.Business.Entities;

namespace Library.Business.Isp;

/// <summary>
/// THIẾT KẾ CHUẨN NGUYÊN LÝ I (INTERFACE SEGREGATION PRINCIPLE - ISP)
///
/// GIẢI PHÁP TÁI CẤU TRÚC:
/// Chia nhỏ "Fat Interface" thành các Interface chuyên biệt theo từng vai trò (Role-based Interfaces):
/// 1. `IBookSearchService`: Chỉ chứa các thao tác tìm kiếm, xem thông tin sách.
/// 2. `IBookBorrowingService`: Chỉ chứa các thao tác mượn/trả sách.
/// 3. `IBookInventoryService`: Chỉ chứa các thao tác quản lý kho, nhập/sửa/xóa sách.
/// 4. `IBarcodePrintingService`: Chỉ chứa thao tác in ấn mã vạch.
///
/// KẾT QUẢ:
/// - Client nào cần gì thì CHỈ phụ thuộc vào Interface đó.
/// - Không một Client nào bị ép buộc phải implement các hàm thừa hoặc ném `NotImplementedException`.
/// - Dễ bảo trì, dễ viết Unit Test và cực kỳ an toàn!
/// </summary>

/// <summary>
/// Interface chuyên biệt: Tra cứu sách
/// </summary>
public interface IBookSearchService
{
    IEnumerable<Book> SearchBooks(string keyword);
    Book? GetBookDetails(int bookId);
}

/// <summary>
/// Interface chuyên biệt: Mượn / Trả sách
/// </summary>
public interface IBookBorrowingService
{
    string BorrowBook(int bookId, string borrowerName);
    string ReturnBook(int bookId, string borrowerName);
}

/// <summary>
/// Interface chuyên biệt: Quản trị kho sách
/// </summary>
public interface IBookInventoryService
{
    Book AddNewBook(string title, string author, decimal price);
    bool UpdatePrice(int bookId, decimal newPrice);
    bool RemoveBook(int bookId);
    int AuditInventoryCount();
}

/// <summary>
/// Interface chuyên biệt: In ấn tem nhãn mã vạch
/// </summary>
public interface IBarcodePrintingService
{
    string PrintBarcode(int bookId, string prefix);
}

/// <summary>
/// Client 1: Kiosk sảnh công cộng
/// CHUẨN ISP: Chỉ phụ thuộc duy nhất vào IBookSearchService!
/// Hoàn toàn KHÔNG bị dính các phương thức quản trị, in ấn hay mượn trả không thuộc thẩm quyền.
/// </summary>
public class CleanGuestKioskService : IBookSearchService
{
    private readonly List<Book> _books = new()
    {
        new Book { Id = 1, Title = "Clean Code: A Handbook of Agile Software Craftsmanship", Author = "Robert C. Martin", BasePrice = 350000m },
        new Book { Id = 2, Title = "Design Patterns: Elements of Reusable Object-Oriented Software", Author = "Erich Gamma et al.", BasePrice = 280000m },
        new Book { Id = 3, Title = "Refactoring: Improving the Design of Existing Code", Author = "Martin Fowler", BasePrice = 310000m }
    };

    public IEnumerable<Book> SearchBooks(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return _books;
        return _books.Where(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                 b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public Book? GetBookDetails(int bookId)
    {
        return _books.FirstOrDefault(b => b.Id == bookId);
    }
}

/// <summary>
/// Client 2: Kiosk tự mượn trả sách tự động (Self-Checkout Station)
/// CHUẨN ISP: Kết hợp 2 interface IBookSearchService và IBookBorrowingService.
/// Vẫn hoàn toàn không dính líu đến IBookInventoryService (Quản trị kho).
/// </summary>
public class CleanSelfCheckoutStation : IBookSearchService, IBookBorrowingService
{
    private readonly CleanGuestKioskService _searchEngine = new();

    public IEnumerable<Book> SearchBooks(string keyword) => _searchEngine.SearchBooks(keyword);
    public Book? GetBookDetails(int bookId) => _searchEngine.GetBookDetails(bookId);

    public string BorrowBook(int bookId, string borrowerName)
    {
        var book = GetBookDetails(bookId);
        if (book is null) return $"[Self-Checkout] Không tìm thấy sách Id = {bookId}";
        book.IsBorrowed = true;
        return $"[Self-Checkout] Độc giả '{borrowerName}' đã tự mượn cuốn '{book.Title}' tại Kiosk tự động.";
    }

    public string ReturnBook(int bookId, string borrowerName)
    {
        var book = GetBookDetails(bookId);
        if (book is null) return $"[Self-Checkout] Không tìm thấy sách Id = {bookId}";
        book.IsBorrowed = false;
        return $"[Self-Checkout] Độc giả '{borrowerName}' đã trả cuốn '{book.Title}' vào hòm nhận sách tự động.";
    }
}

/// <summary>
/// Client 3: Phần mềm Quản trị của Thủ thư (Librarian Management Portal)
/// CHUẨN ISP: Khi cần quản lý kho & in ấn mã vạch, chỉ cần compose các interface tương ứng.
/// </summary>
public class CleanLibrarianInventoryService : IBookInventoryService, IBarcodePrintingService
{
    private readonly List<Book> _inventory = new();
    private int _nextId = 100;

    public Book AddNewBook(string title, string author, decimal price)
    {
        var book = new Book
        {
            Id = ++_nextId,
            Title = title,
            Author = author,
            BasePrice = price,
            IsBorrowed = false
        };
        _inventory.Add(book);
        return book;
    }

    public bool UpdatePrice(int bookId, decimal newPrice)
    {
        var book = _inventory.FirstOrDefault(b => b.Id == bookId);
        if (book is null) return false;
        book.BasePrice = newPrice;
        return true;
    }

    public bool RemoveBook(int bookId)
    {
        var book = _inventory.FirstOrDefault(b => b.Id == bookId);
        if (book is null) return false;
        return _inventory.Remove(book);
    }

    public int AuditInventoryCount() => _inventory.Count;

    public string PrintBarcode(int bookId, string prefix)
    {
        return $"BARCODE-[{prefix.ToUpper()}]-BOOK#{bookId}-{DateTime.UtcNow.Ticks % 1000000}";
    }
}
