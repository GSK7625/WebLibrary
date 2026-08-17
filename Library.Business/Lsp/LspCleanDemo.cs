namespace Library.Business.Lsp;

/// <summary>
/// THIẾT KẾ CHUẨN NGUYÊN LÝ L (LISKOV SUBSTITUTION PRINCIPLE - LSP)
///
/// GIẢI PHÁP TÁI CẤU TRÚC:
/// 1. Tách biệt rõ ràng thuộc tính nền tảng (BaseLibraryResource) không ép buộc hành vi mượn.
/// 2. Sử dụng Interface/Hợp đồng rõ ràng: `IBorrowableResource` cho tài nguyên mang về nhà được,
///    và `IInLibraryConsultableResource` cho tài nguyên chỉ đọc/tra cứu tại chỗ.
/// 3. Mọi class con hiện thực `IBorrowableResource` (`PhysicalBorrowableBook`, `AudioBookPlayerDevice`, `EBookLicense`)
///    đều tuân thủ 100% hợp đồng mượn mà KHÔNG BAO GIỜ ném ngoại lệ bất thường.
/// 4. Trình biên dịch C# sẽ ngăn chặn ngay từ lúc compile nếu ai đó cố gắng truyền một tài liệu chỉ đọc
///    vào luồng mượn sách mang về -> Đảm bảo tính đúng đắn tuyệt đối của hệ thống đa hình!
/// </summary>

public abstract class BaseLibraryResource
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string LocationDescription { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// Hợp đồng dành cho các tài nguyên CÓ THỂ MƯỢN MANG VỀ.
/// Mọi class con hiện thực interface này cam kết thực hiện đúng hợp đồng (LSP Compliant).
/// </summary>
public interface IBorrowableResource
{
    int Id { get; }
    string Title { get; }
    decimal DailyRentalRate { get; }
    int MaxBorrowDays { get; }

    BorrowTransactionResult Borrow(string borrowerName, int requestedDays);
    decimal CalculateRentalFee(int days);
}

/// <summary>
/// Hợp đồng dành cho các tài liệu QUÝ HIẾM / TRA CỨU CHỈ ĐỌC TẠI CHỖ.
/// </summary>
public interface IInLibraryConsultableResource
{
    int Id { get; }
    string Title { get; }
    string ReadingRoom { get; }
    string ReserveReadingDesk(string readerName, int hours);
}

public class BorrowTransactionResult
{
    public bool IsSuccess { get; set; }
    public string ResourceTitle { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public int ApprovedDays { get; set; }
    public decimal EstimatedFee { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Sách in thông thường mượn về nhà
/// </summary>
public class PhysicalBorrowableBook : BaseLibraryResource, IBorrowableResource
{
    public string Isbn { get; set; } = string.Empty;
    public decimal DailyRentalRate { get; set; } = 3000m;
    public int MaxBorrowDays => 30;

    public BorrowTransactionResult Borrow(string borrowerName, int requestedDays)
    {
        int actualDays = Math.Min(requestedDays, MaxBorrowDays);
        decimal fee = CalculateRentalFee(actualDays);
        IsAvailable = false;

        return new BorrowTransactionResult
        {
            IsSuccess = true,
            ResourceTitle = Title,
            BorrowerName = borrowerName,
            ApprovedDays = actualDays,
            EstimatedFee = fee,
            Message = $"[LSP Chuẩn] Sách in '{Title}' (ISBN: {Isbn}) đã được mượn thành công cho '{borrowerName}' trong {actualDays} ngày."
        };
    }

    public decimal CalculateRentalFee(int days)
    {
        return DailyRentalRate * days;
    }
}

/// <summary>
/// Thiết bị nghe AudioBook mượn về nhà (Class con khác loại nhưng THAY THẾ HOÀN HẢO theo LSP)
/// </summary>
public class AudioBookPlayerDevice : BaseLibraryResource, IBorrowableResource
{
    public string DeviceSerialNumber { get; set; } = string.Empty;
    public decimal DailyRentalRate { get; set; } = 5000m;
    public int MaxBorrowDays => 14;

    public BorrowTransactionResult Borrow(string borrowerName, int requestedDays)
    {
        int actualDays = Math.Min(requestedDays, MaxBorrowDays);
        decimal fee = CalculateRentalFee(actualDays);
        IsAvailable = false;

        return new BorrowTransactionResult
        {
            IsSuccess = true,
            ResourceTitle = Title,
            BorrowerName = borrowerName,
            ApprovedDays = actualDays,
            EstimatedFee = fee,
            Message = $"[LSP Chuẩn] Thiết bị AudioBook '{Title}' (S/N: {DeviceSerialNumber}) đã giao cho '{borrowerName}' trong {actualDays} ngày (kèm tai nghe & củ sạc)."
        };
    }

    public decimal CalculateRentalFee(int days)
    {
        // Có thể có bảo hiểm thiết bị
        return (DailyRentalRate * days) + 2000m;
    }
}

/// <summary>
/// Bản thảo cổ quý hiếm chỉ đọc tại chỗ: KHÔNG implement IBorrowableResource!
/// Không thể bị vô tình truyền vào hàm mượn sách mang về -> Bảo vệ tính đúng đắn LSP.
/// </summary>
public class SpecialArchiveManuscript : BaseLibraryResource, IInLibraryConsultableResource
{
    public string ReadingRoom => "Phòng Giám sát Bảo vật Cổ - Tầng 4";
    public string SecurityCode { get; set; } = "SEC-ARCHIVE-99";

    public string ReserveReadingDesk(string readerName, int hours)
    {
        return $"[Đọc Tại Chỗ] Đã xếp chỗ đọc tại bàn chuyên dụng cho độc giả '{readerName}' để tra cứu hiện vật cổ '{Title}' ({SecurityCode}) trong {hours} giờ dưới sự giám sát của thủ thư.";
    }
}

/// <summary>
/// Dịch vụ xử lý mượn đa hình chuẩn LSP.
/// Bất kỳ đối tượng nào hiện thực `IBorrowableResource` đều có thể thay thế cho nhau một cách hoàn hảo!
/// </summary>
public class LspCleanBorrowProcessor
{
    public (List<BorrowTransactionResult> Results, string Summary) ProcessBatchBorrow(
        IEnumerable<IBorrowableResource> borrowableItems,
        string borrowerName,
        int requestedDays)
    {
        var results = new List<BorrowTransactionResult>();

        foreach (var item in borrowableItems)
        {
            // Thay thế đa hình hoàn toàn an toàn - Không bao giờ văng Exception bất ngờ!
            var result = item.Borrow(borrowerName, requestedDays);
            results.Add(result);
        }

        string summary = $"[LSP Thành Công] Đã xử lý mượn an toàn {results.Count} tài nguyên đa hình cho độc giả '{borrowerName}'.";
        return (results, summary);
    }
}
