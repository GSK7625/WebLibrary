namespace Library.Business.Dip;

/// <summary>
/// THIẾT KẾ CHUẨN NGUYÊN LÝ D (DEPENDENCY INVERSION PRINCIPLE - DIP)
///
/// GIẢI PHÁP TÁI CẤU TRÚC:
/// 1. Định nghĩa Abstraction cấp cao: `INotificationSender` và `IAuditLogger`.
/// 2. Module nghiệp vụ cấp cao (`BorrowNotificationApplicationService`) CHỈ phụ thuộc vào các Abstraction này
///    thông qua Constructor Injection (Inversion of Control).
/// 3. Các chi tiết hạ tầng cấp thấp (`EmailNotificationSender`, `SmsNotificationSender`, `ZaloNotificationSender`,
///    `DatabaseAuditLogger`) hiện thực (implement) Abstraction.
///
/// LỢI ÍCH ĐẠT ĐƯỢC:
/// - Nghiệp vụ độc lập 100% với hạ tầng.
/// - Có thể thêm kênh gửi tin mới (Telegram, Web Push, Zalo) mà không phải sửa 1 dòng code nghiệp vụ nào.
/// - Viết Unit Test dễ dàng bằng Mock/InMemory Sender mà không cần môi trường mạng thật.
/// </summary>

// --- 1. ABSTRACTIONS (GIAO DIỆN CẤP CAO) ---
public interface INotificationSender
{
    string ChannelName { get; }
    Task<NotificationDeliveryResult> SendAsync(string recipient, string subject, string message);
}

public interface IAuditLogger
{
    Task LogAsync(string category, string action, string details);
    IReadOnlyList<string> GetRecentLogs();
}

public class NotificationDeliveryResult
{
    public bool IsSuccess { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string DeliveryDetails { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.Now;
}

// --- 2. CONCRETE LOW-LEVEL ADAPTERS (CÁC HIỆN THỰC CẤP THẤP) ---
public class EmailNotificationSender : INotificationSender
{
    public string ChannelName => "Email Channel (SMTP/SendGrid)";

    public Task<NotificationDeliveryResult> SendAsync(string recipient, string subject, string message)
    {
        // Giả lập gửi email qua hạ tầng
        return Task.FromResult(new NotificationDeliveryResult
        {
            IsSuccess = true,
            Channel = ChannelName,
            Recipient = recipient,
            DeliveryDetails = $"[DIP Email Adapter] Gửi thư điện tử tới '{recipient}' - Chủ đề: [{subject}] - Nội dung: {message}"
        });
    }
}

public class SmsNotificationSender : INotificationSender
{
    public string ChannelName => "SMS Brandname Channel";

    public Task<NotificationDeliveryResult> SendAsync(string recipient, string subject, string message)
    {
        return Task.FromResult(new NotificationDeliveryResult
        {
            IsSuccess = true,
            Channel = ChannelName,
            Recipient = recipient,
            DeliveryDetails = $"[DIP SMS Adapter] Bắn SMS OTP/Brandname tới '{recipient}' - Nội dung: {message}"
        });
    }
}

public class ZaloNotificationSender : INotificationSender
{
    public string ChannelName => "Zalo Official Account (ZNS)";

    public Task<NotificationDeliveryResult> SendAsync(string recipient, string subject, string message)
    {
        return Task.FromResult(new NotificationDeliveryResult
        {
            IsSuccess = true,
            Channel = ChannelName,
            Recipient = recipient,
            DeliveryDetails = $"[DIP Zalo Adapter] Đã gửi thông báo ZNS mẫu tới người dùng Zalo '{recipient}'"
        });
    }
}

public class InMemoryAuditLogger : IAuditLogger
{
    private readonly List<string> _logs = new();

    public Task LogAsync(string category, string action, string details)
    {
        _logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{category.ToUpper()}] [{action}]: {details}");
        return Task.CompletedTask;
    }

    public IReadOnlyList<string> GetRecentLogs() => _logs.AsReadOnly();
}

// --- 3. HIGH-LEVEL BUSINESS APPLICATION SERVICE ---
public interface IBorrowNotificationService
{
    Task<BorrowNotificationResponse> SendOverdueNotificationAsync(
        string borrowerName,
        string contactInfo,
        string bookTitle,
        int daysLate);
}

public class BorrowNotificationResponse
{
    public string BorrowerName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public int DaysLate { get; set; }
    public List<NotificationDeliveryResult> DeliveryResults { get; set; } = new();
    public List<string> AuditLogs { get; set; } = new();
    public string ArchitectureExplanation { get; set; } = string.Empty;
}

/// <summary>
/// Lớp nghiệp vụ điều phối thông báo chuẩn DIP.
/// Hoàn toàn KHÔNG biết chi tiết Email/SMS/Zalo được gửi như thế nào!
/// </summary>
public class BorrowNotificationApplicationService : IBorrowNotificationService
{
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly IAuditLogger _auditLogger;

    public BorrowNotificationApplicationService(
        IEnumerable<INotificationSender> notificationSenders,
        IAuditLogger auditLogger)
    {
        _notificationSenders = notificationSenders;
        _auditLogger = auditLogger;
    }

    public async Task<BorrowNotificationResponse> SendOverdueNotificationAsync(
        string borrowerName,
        string contactInfo,
        string bookTitle,
        int daysLate)
    {
        var response = new BorrowNotificationResponse
        {
            BorrowerName = borrowerName,
            BookTitle = bookTitle,
            DaysLate = daysLate,
            ArchitectureExplanation = "DIP Compliant: Lớp nghiệp vụ cấp cao chỉ phụ thuộc vào Interface `INotificationSender` và `IAuditLogger`. Các kênh thông báo được inject tự động qua DI Container."
        };

        string subject = "THƯ VIỆN: THÔNG BÁO SÁCH MƯỢN ĐÃ QUÁ HẠN";
        string message = $"Kính gửi {borrowerName}, cuốn sách '{bookTitle}' bạn mượn đã quá hạn {daysLate} ngày. Vui lòng mang sách đến trả để tránh phát sinh thêm phí phạt.";

        foreach (var sender in _notificationSenders)
        {
            var deliveryResult = await sender.SendAsync(contactInfo, subject, message);
            response.DeliveryResults.Add(deliveryResult);

            await _auditLogger.LogAsync("Notification", "SendOverdueAlert", $"Đã điều phối qua kênh {sender.ChannelName} cho {borrowerName}");
        }

        response.AuditLogs = _auditLogger.GetRecentLogs().ToList();
        return response;
    }
}
