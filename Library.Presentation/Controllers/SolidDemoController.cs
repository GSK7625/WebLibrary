using Library.Business.Dip;
using Library.Business.Isp;
using Library.Business.Legacy;
using Library.Business.Lsp;
using Microsoft.AspNetCore.Mvc;

namespace Library.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolidDemoController : ControllerBase
{
    private readonly LspViolationProcessor _lspViolationProcessor;
    private readonly LspCleanBorrowProcessor _lspCleanProcessor;
    private readonly BadGuestKioskClient _badGuestKiosk;
    private readonly IBookSearchService _cleanSearchService;
    private readonly CleanSelfCheckoutStation _selfCheckoutStation;
    private readonly CleanLibrarianInventoryService _librarianInventoryService;
    private readonly BadBorrowNotificationManager _badDipNotifier;
    private readonly IBorrowNotificationService _cleanDipNotifier;

    public SolidDemoController(
        LspViolationProcessor lspViolationProcessor,
        LspCleanBorrowProcessor lspCleanProcessor,
        BadGuestKioskClient badGuestKiosk,
        IBookSearchService cleanSearchService,
        CleanSelfCheckoutStation selfCheckoutStation,
        CleanLibrarianInventoryService librarianInventoryService,
        BadBorrowNotificationManager badDipNotifier,
        IBorrowNotificationService cleanDipNotifier)
    {
        _lspViolationProcessor = lspViolationProcessor;
        _lspCleanProcessor = lspCleanProcessor;
        _badGuestKiosk = badGuestKiosk;
        _cleanSearchService = cleanSearchService;
        _selfCheckoutStation = selfCheckoutStation;
        _librarianInventoryService = librarianInventoryService;
        _badDipNotifier = badDipNotifier;
        _cleanDipNotifier = cleanDipNotifier;
    }

    // ==========================================
    // 1. L - LISKOV SUBSTITUTION PRINCIPLE (LSP)
    // ==========================================

    /// <summary>
    /// [LSP VIOLATION DEMO] Minh chứng lỗi khi lớp con (Sách đọc tại chỗ) phá vỡ hợp đồng của lớp cha (Tài nguyên mượn)
    /// </summary>
    [HttpGet("lsp/violation-demo")]
    public IActionResult DemoLspViolation()
    {
        var items = new List<BadLibraryResource>
        {
            new BadPhysicalBook { Id = 1, Title = "Giáo trình C# Nâng cao", DailyRentalPrice = 3000m },
            new BadReferenceOnlyBook { Id = 2, Title = "Đại Việt Sử Ký Toàn Thư (Bản Gốc 1697)", DailyRentalPrice = 0m },
            new BadPhysicalBook { Id = 3, Title = "Clean Code", DailyRentalPrice = 4000m }
        };

        var (successMsgs, errorMsg) = _lspViolationProcessor.ProcessBatchBorrow(items, "Nguyễn Văn A", 5);

        return Ok(new
        {
            Principle = "L - Liskov Substitution Principle (LSP)",
            Status = "VIOLATION (Vi phạm)",
            Explanation = "Lớp `BadReferenceOnlyBook` kế thừa `BadLibraryResource` nhưng bên trong `Borrow()` lại ném ngoại lệ InvalidOperationException. Khi duyệt danh sách đa hình, hệ thống bị sập!",
            ProcessedBeforeCrash = successMsgs,
            CrashError = errorMsg
        });
    }

    /// <summary>
    /// [LSP CLEAN DEMO] Thiết kế chuẩn: Lớp con thay thế hoàn hảo cho lớp cha/interface
    /// </summary>
    [HttpGet("lsp/clean-demo")]
    public IActionResult DemoLspClean()
    {
        // 1. Danh sách mượn đa hình an toàn 100%
        var borrowableItems = new List<IBorrowableResource>
        {
            new PhysicalBorrowableBook { Id = 1, Title = "Clean Architecture", DailyRentalRate = 3000m, Isbn = "978-0134494166" },
            new AudioBookPlayerDevice { Id = 2, Title = "Máy đọc sách Kindle + Audio Sapiens", DailyRentalRate = 6000m, DeviceSerialNumber = "KDL-9821-VN" }
        };

        var (borrowResults, summary) = _lspCleanProcessor.ProcessBatchBorrow(borrowableItems, "Trần Thị B", 7);

        // 2. Tài liệu chỉ đọc tại chỗ được tách biệt hợp đồng, xử lý an toàn
        var rareManuscript = new SpecialArchiveManuscript
        {
            Id = 99,
            Title = "Bản thảo Chiếu Dời Đô Cổ",
            LocationDescription = "Khu bảo mật A"
        };
        string inLibraryResult = rareManuscript.ReserveReadingDesk("Trần Thị B", 3);

        return Ok(new
        {
            Principle = "L - Liskov Substitution Principle (LSP)",
            Status = "CLEAN & COMPLIANT (Chuẩn thiết kế)",
            Explanation = "Mọi đối tượng kế thừa `IBorrowableResource` đều thay thế cho nhau hoàn hảo. Tài liệu đọc tại chỗ tách sang `IInLibraryConsultableResource`, không bao giờ bị ép mượn sai.",
            BatchBorrowSummary = summary,
            BorrowTransactions = borrowResults,
            InLibraryConsultation = inLibraryResult
        });
    }

    // ==========================================
    // 2. I - INTERFACE SEGREGATION PRINCIPLE (ISP)
    // ==========================================

    /// <summary>
    /// [ISP VIOLATION DEMO] Minh chứng lỗi khi Client (Kiosk công cộng) bị ép implement Fat Interface
    /// </summary>
    [HttpGet("isp/violation-demo")]
    public IActionResult DemoIspViolation()
    {
        var searchResults = _badGuestKiosk.SearchBooks("Clean");

        string? unsupportedException = null;
        try
        {
            // Kiosk công cộng bị gọi nhầm hàm quản trị hoặc mượn trả do Interface quá béo
            _badGuestKiosk.DeleteBookFromSystem(1);
        }
        catch (NotImplementedException ex)
        {
            unsupportedException = ex.Message;
        }

        return Ok(new
        {
            Principle = "I - Interface Segregation Principle (ISP)",
            Status = "VIOLATION (Vi phạm)",
            Explanation = "Interface `IFatLibraryOperations` quá cồng kềnh. `BadGuestKioskClient` chỉ cần tìm kiếm nhưng buộc phải implement 8 hàm thừa và ném `NotImplementedException` khi bị gọi!",
            WorkingSearchFeature = searchResults,
            UnexpectedFailure = unsupportedException
        });
    }

    /// <summary>
    /// [ISP CLEAN DEMO] Phân tách thành các Role-based Interfaces chuyên biệt
    /// </summary>
    [HttpGet("isp/clean-demo")]
    public IActionResult DemoIspClean()
    {
        // 1. Kiosk tra cứu chỉ dùng IBookSearchService
        var searchBooks = _cleanSearchService.SearchBooks("Design");

        // 2. Kiosk tự mượn trả dùng kết hợp IBookSearchService & IBookBorrowingService
        var borrowMsg = _selfCheckoutStation.BorrowBook(2, "Nguyễn Hoàng Nam");

        // 3. Quản trị viên dùng IBookInventoryService & IBarcodePrintingService
        var newBook = _librarianInventoryService.AddNewBook("Domain-Driven Design", "Eric Evans", 420000m);
        var barcode = _librarianInventoryService.PrintBarcode(newBook.Id, "TECH");

        return Ok(new
        {
            Principle = "I - Interface Segregation Principle (ISP)",
            Status = "CLEAN & COMPLIANT (Chuẩn thiết kế)",
            Explanation = "Chia nhỏ thành `IBookSearchService`, `IBookBorrowingService`, `IBookInventoryService`, `IBarcodePrintingService`. Mỗi client chỉ phụ thuộc đúng phần mình cần.",
            GuestKioskSearchResult = searchBooks,
            SelfCheckoutAction = borrowMsg,
            LibrarianAdminAction = new
            {
                CreatedBook = newBook,
                PrintedBarcode = barcode,
                TotalInventory = _librarianInventoryService.AuditInventoryCount()
            }
        });
    }

    // ==========================================
    // 3. D - DEPENDENCY INVERSION PRINCIPLE (DIP)
    // ==========================================

    /// <summary>
    /// [DIP VIOLATION DEMO] Module cấp cao tự ý 'new' trực tiếp các class hạ tầng cấp thấp
    /// </summary>
    [HttpGet("dip/violation-demo")]
    public IActionResult DemoDipViolation()
    {
        var (logs, summary) = _badDipNotifier.SendOverdueAlertBadDIP(
            borrowerName: "Lê Văn C",
            phone: "0912345678",
            email: "levanc@example.com",
            bookTitle: "Tạp Chí Khoa Học",
            daysLate: 12);

        return Ok(new
        {
            Principle = "D - Dependency Inversion Principle (DIP)",
            Status = "VIOLATION (Vi phạm)",
            Explanation = "`BadBorrowNotificationManager` tự ý 'new' các class hạ tầng `HardcodedSmsGateway`, `HardcodedSmtpMailer`, `HardcodedFileLogger`. Không thể mock khi test và bị khóa chặt vào nhà cung cấp!",
            ExecutionLogs = logs,
            Summary = summary
        });
    }

    /// <summary>
    /// [DIP CLEAN DEMO] Module cấp cao điều phối thông báo qua Abstractions (INotificationSender & IAuditLogger)
    /// </summary>
    [HttpPost("dip/clean-notify")]
    public async Task<IActionResult> DemoDipClean(
        [FromQuery] string borrowerName = "Phạm Thị Minh (VIP)",
        [FromQuery] string contactInfo = "minh.pham@company.com / 0987654321",
        [FromQuery] string bookTitle = "Clean Architecture",
        [FromQuery] int daysLate = 4)
    {
        var response = await _cleanDipNotifier.SendOverdueNotificationAsync(
            borrowerName,
            contactInfo,
            bookTitle,
            daysLate);

        return Ok(response);
    }
}
