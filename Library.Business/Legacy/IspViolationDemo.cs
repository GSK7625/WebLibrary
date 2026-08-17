using Library.Business.Entities;

namespace Library.Business.Legacy;

/// <summary>
/// VÍ DỤ VI PHẠM NGUYÊN LÝ I (INTERFACE SEGREGATION PRINCIPLE - ISP)
///
/// Định nghĩa ISP: "Client không nên bị ép buộc phải phụ thuộc vào các interface/phương thức mà nó không sử dụng."
///
/// VẤN ĐỀ (FAT / GOD INTERFACE):
/// Interface `IFatLibraryOperations` gom tất cả mọi nghiệp vụ của toàn hệ thống thư viện vào 1 nơi:
/// - Nghiệp vụ tra cứu cho bạn đọc
/// - Nghiệp vụ mượn trả của quầy thủ thư
/// - Nghiệp vụ nhập kho, xóa sách, kiểm kê của quản lý
/// - Nghiệp vụ in mã vạch, phục chế gáy sách của phòng kỹ thuật
///
/// HẬU QUẢ:
/// Một Kiosk tra cứu cho khách vãng lai (`BadGuestKioskClient`) chỉ muốn tìm kiếm sách
/// nhưng buộc phải implement toàn bộ 8 phương thức thừa thãi và ném `NotImplementedException`.
/// Code trở nên rác, dễ lỗi khi gọi nhầm và vi phạm nghiêm trọng tính đóng gói!
/// </summary>
public interface IFatLibraryOperations
{
    // 1. Dành cho độc giả tra cứu
    IEnumerable<Book> SearchBooks(string keyword);
    Book? GetBookDetails(int bookId);

    // 2. Dành cho mượn trả
    void BorrowBook(int bookId, string borrowerName);
    void ReturnBook(int bookId);

    // 3. Dành cho Quản trị viên thủ kho
    void AddNewBookToInventory(string title, string author, decimal price);
    void UpdateBookPrice(int bookId, decimal newPrice);
    void DeleteBookFromSystem(int bookId);
    int AuditTotalInventory();

    // 4. Dành cho Kỹ thuật & In ấn
    string PrintBarcodeSticker(int bookId);
    void RestoreBookBinding(int bookId);
}

/// <summary>
/// Client Kiosk đặt tại sảnh thư viện: Chỉ phục vụ tra cứu nhanh.
/// VI PHẠM ISP: Bị ép phải implement các phương thức quản trị & kỹ thuật mà Kiosk KHÔNG BAO GIỜ dùng!
/// </summary>
public class BadGuestKioskClient : IFatLibraryOperations
{
    private readonly List<Book> _dummyBooks = new()
    {
        new Book { Id = 1, Title = "Clean Architecture", Author = "Robert C. Martin", BasePrice = 300000m },
        new Book { Id = 2, Title = "Design Patterns in C#", Author = "Gang of Four", BasePrice = 250000m }
    };

    // 1. Chỉ thực sự dùng 2 hàm này:
    public IEnumerable<Book> SearchBooks(string keyword)
    {
        return _dummyBooks.FindAll(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public Book? GetBookDetails(int bookId)
    {
        return _dummyBooks.Find(b => b.Id == bookId);
    }

    // 2. Các hàm mượn trả: Kiosk công cộng không có quyền -> Bị ép ném NotImplementedException
    public void BorrowBook(int bookId, string borrowerName)
    {
        throw new NotImplementedException("[ISP VIOLATION] Kiosk sảnh công cộng không có chức năng mượn sách. Vui lòng liên hệ quầy!");
    }

    public void ReturnBook(int bookId)
    {
        throw new NotImplementedException("[ISP VIOLATION] Kiosk tra cứu không có khay nhận trả sách!");
    }

    // 3. Các hàm quản trị: Bị ép implement vô nghĩa
    public void AddNewBookToInventory(string title, string author, decimal price)
    {
        throw new NotImplementedException("[ISP VIOLATION] Độc giả công cộng không thể tự ý thêm sách vào kho!");
    }

    public void UpdateBookPrice(int bookId, decimal newPrice)
    {
        throw new NotImplementedException("[ISP VIOLATION] Độc giả không được phép sửa giá sách!");
    }

    public void DeleteBookFromSystem(int bookId)
    {
        throw new NotImplementedException("[ISP VIOLATION] Độc giả không được phép xóa sách khỏi hệ thống!");
    }

    public int AuditTotalInventory()
    {
        throw new NotImplementedException("[ISP VIOLATION] Kiosk không có quyền kiểm kê tài sản thư viện!");
    }

    // 4. Kỹ thuật
    public string PrintBarcodeSticker(int bookId)
    {
        throw new NotImplementedException("[ISP VIOLATION] Kiosk không tích hợp máy in mã vạch công nghiệp!");
    }

    public void RestoreBookBinding(int bookId)
    {
        throw new NotImplementedException("[ISP VIOLATION] Kiosk không có cánh tay robot phục chế bìa sách!");
    }
}
