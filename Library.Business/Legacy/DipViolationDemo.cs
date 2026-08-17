namespace Library.Business.Legacy;

/// <summary>
/// VÍ DỤ VI PHẠM NGUYÊN LÝ D (DEPENDENCY INVERSION PRINCIPLE - DIP)
///
/// Định nghĩa DIP:
/// 1. Các module cấp cao không nên phụ thuộc vào các module cấp thấp. Cả hai nên phụ thuộc vào abstraction (interface/abstract class).
/// 2. Abstraction không nên phụ thuộc vào chi tiết. Chi tiết phải phụ thuộc vào abstraction.
///
/// VẤN ĐỀ TRONG VÍ DỤ NÀY:
/// Lớp nghiệp vụ cấp cao `BadBorrowNotificationManager` trực tiếp khởi tạo (bằng từ khóa `new`)
/// các class hạ tầng cấp thấp: `HardcodedSmsGateway`, `HardcodedSmtpMailer`, `HardcodedFileLogger`.
///
/// HẬU QUẢ VI PHẠM DIP:
/// 1. THẮT CHẶT PHỤ THUỘC (Tight Coupling): Nghiệp vụ mượn sách bị "bắt cóc" bởi phần cứng và cổng mạng SMS/SMTP cụ thể.
/// 2. KHÔNG THỂ VIẾT UNIT TEST: Không thể test logic gửi thông báo mà không kết nối cổng mạng thật hoặc làm phát sinh chi phí gửi SMS thật.
/// 3. KHÓ THAY ĐỔI: Nếu thư viện muốn chuyển từ SMS sang Zalo ZNS hoặc Telegram, hay đổi nhà cung cấp Mail,
///    ta BUỘC PHẢI SỬA CODE trong class nghiệp vụ cấp cao `BadBorrowNotificationManager`.
/// </summary>

// --- CÁC MODULE HẠ TẦNG CẤP THẤP (LOW-LEVEL DETAILS) ---
public class HardcodedSmsGateway
{
    private readonly string _apiKey;
    public HardcodedSmsGateway(string apiKey) => _apiKey = apiKey;

    public string SendDirectSms(string phoneNumber, string message)
    {
        return $"[SMS GATEWAY (HARDCODED) Key={_apiKey}] Đã bắn SMS tới số '{phoneNumber}': {message}";
    }
}

public class HardcodedSmtpMailer
{
    private readonly string _smtpHost;
    private readonly int _port;
    public HardcodedSmtpMailer(string host, int port) { _smtpHost = host; _port = port; }

    public string SendSmtpMail(string email, string subject, string body)
    {
        return $"[SMTP MAILER (HARDCODED) Server={_smtpHost}:{_port}] Đã gửi email tới '{email}' - Tiêu đề: {subject}";
    }
}

public class HardcodedFileLogger
{
    private readonly string _filePath;
    public HardcodedFileLogger(string path) => _filePath = path;

    public string WriteLog(string message)
    {
        return $"[FILE LOG (HARDCODED) Path={_filePath}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
    }
}

// --- MODULE NGHIỆP VỤ CẤP CAO (HIGH-LEVEL MODULE) ---
public class BadBorrowNotificationManager
{
    // VI PHẠM DIP: Module cấp cao tự ý 'new' chi tiết cấp thấp thay vì nhận Abstraction qua Constructor Injection
    private readonly HardcodedSmsGateway _smsGateway = new("VIETTEL_SMS_SECRET_KEY_12345");
    private readonly HardcodedSmtpMailer _smtpMailer = new("smtp.internal-library.vn", 587);
    private readonly HardcodedFileLogger _fileLogger = new(@"C:\LibraryLogs\notification_audit.log");

    public (List<string> Logs, string Summary) SendOverdueAlertBadDIP(string borrowerName, string phone, string email, string bookTitle, int daysLate)
    {
        var logs = new List<string>();

        string alertMessage = $"Cảnh báo: Sách '{bookTitle}' của bạn đã quá hạn {daysLate} ngày. Vui lòng hoàn trả sớm!";

        // 1. Phụ thuộc chặt vào SMS cứng
        var smsResult = _smsGateway.SendDirectSms(phone, alertMessage);
        logs.Add(smsResult);

        // 2. Phụ thuộc chặt vào SMTP cứng
        var emailResult = _smtpMailer.SendSmtpMail(email, "THÔNG BÁO QUÁ HẠN MƯỢN SÁCH", alertMessage);
        logs.Add(emailResult);

        // 3. Phụ thuộc chặt vào File Logger cứng
        var logResult = _fileLogger.WriteLog($"Đã gửi thông báo quá hạn cho độc giả '{borrowerName}'");
        logs.Add(logResult);

        string summary = $"[Bad DIP] Hoàn tất gửi cảnh báo trễ hạn nhưng toàn bộ logic bị gắn cứng (hardcoded) vào hạ tầng SMS/SMTP/File!";
        return (logs, summary);
    }
}
