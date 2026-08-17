namespace Library.Business.Legacy;

/// <summary>
/// VÍ DỤ VI PHẠM NGUYÊN LÝ L (LISKOV SUBSTITUTION PRINCIPLE - LSP)
///
/// Định nghĩa LSP: "Các đối tượng của lớp con phải có thể thay thế cho các đối tượng của lớp cha
/// mà KHÔNG làm thay đổi tính đúng đắn của chương trình."
///
/// VẤN ĐỀ TRONG VÍ DỤ NÀY:
/// Lớp cha `BadLibraryResource` định nghĩa hợp đồng chung là mọi tài nguyên đều có thể mượn (`Borrow`).
/// Tuy nhiên, lớp con `BadReferenceOnlyBook` (Sách cổ quý hiếm, Từ điển chỉ đọc tại chỗ) lại KẾ THỪA
/// từ `BadLibraryResource` nhưng bên trong phương thức `Borrow()` lại NÉM NGOẠI LỆ `InvalidOperationException`.
///
/// HẬU QUẢ:
/// Khi một hàm cấp cao xử lý danh sách tài nguyên đa hình `IEnumerable<BadLibraryResource>`,
/// nó kỳ vọng mọi phần tử đều mượn được. Khi duyệt trúng `BadReferenceOnlyBook`, cả hệ thống SẬP (Crash)
/// vì lớp con KHÔNG THỂ THAY THẾ CHO LỚP CHA!
/// </summary>
public abstract class BadLibraryResource
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal DailyRentalPrice { get; set; }

    /// <summary>
    /// Phương thức ở lớp cha cam kết cho phép mượn tài nguyên
    /// </summary>
    public virtual string Borrow(string borrowerName, int days)
    {
        return $"[LSP Base] Độc giả '{borrowerName}' đã mượn tài nguyên '{Title}' trong {days} ngày.";
    }

    public virtual decimal CalculateRentalFee(int days)
    {
        return DailyRentalPrice * days;
    }
}

/// <summary>
/// Sách giáo trình/Sách thường mượn được bình thường -> Tuân thủ hành vi lớp cha
/// </summary>
public class BadPhysicalBook : BadLibraryResource
{
    public string Isbn { get; set; } = string.Empty;
}

/// <summary>
/// Sách quý hiếm / Bản thảo cổ / Từ điển bách khoa: CHỈ ĐƯỢC ĐỌC TẠI THƯ VIỆN.
/// VI PHẠM LSP: Kế thừa lớp cha nhưng phá vỡ hợp đồng của lớp cha bằng cách ném Exception!
/// </summary>
public class BadReferenceOnlyBook : BadLibraryResource
{
    public string ArchiveRoomLocation { get; set; } = "Phòng Lưu trữ Đặc biệt - Tầng 3";

    public override string Borrow(string borrowerName, int days)
    {
        // VI PHẠM LSP: Ném ngoại lệ khi được gọi mượn, làm vỡ kỳ vọng của Client sử dụng BadLibraryResource
        throw new InvalidOperationException(
            $"[LSP VIOLATION] Tài liệu tham khảo '{Title}' là hiện vật quý tại '{ArchiveRoomLocation}', CHỈ ĐỌC TẠI CHỖ, TUYỆT ĐỐI KHÔNG CHO MƯỢN VỀ NHÀ!");
    }

    public override decimal CalculateRentalFee(int days)
    {
        throw new NotSupportedException("[LSP VIOLATION] Tài liệu không cho mượn nên không có phí thuê ngày!");
    }
}

/// <summary>
/// Dịch vụ xử lý mượn đa hình theo danh sách tài nguyên (Client code).
/// Minh chứng sự sụp đổ khi một class con vi phạm LSP.
/// </summary>
public class LspViolationProcessor
{
    public (List<string> SuccessMessages, string? ErrorMessage) ProcessBatchBorrow(
        IEnumerable<BadLibraryResource> resources,
        string borrowerName,
        int borrowDays)
    {
        var messages = new List<string>();

        try
        {
            foreach (var item in resources)
            {
                // Client tin tưởng vào hợp đồng của BadLibraryResource
                // Nhưng nếu item là BadReferenceOnlyBook -> Ném Exception -> Đứt gãy toàn bộ chu trình xử lý!
                var msg = item.Borrow(borrowerName, borrowDays);
                var fee = item.CalculateRentalFee(borrowDays);
                messages.Add($"{msg} (Dự kiến phí: {fee:N0} VNĐ)");
            }

            return (messages, null);
        }
        catch (Exception ex)
        {
            return (messages, $"[SẬP HỆ THỐNG DO VI PHẠM LSP]: {ex.Message}");
        }
    }
}
