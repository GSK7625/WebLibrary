using Library.Business.Legacy;
using Library.Business.Lsp;
using Xunit;

namespace Library.Tests;

public class LspTests
{
    [Fact]
    public void LspViolation_WhenDerivedClassThrowsUnexpectedException_ShouldBreakBatchBorrowing()
    {
        // ARRANGE: Danh sách đa hình chứa lớp con vi phạm LSP
        var resources = new List<BadLibraryResource>
        {
            new BadPhysicalBook { Id = 1, Title = "C# 13 In A Nutshell", DailyRentalPrice = 5000m },
            new BadReferenceOnlyBook { Id = 2, Title = "Đại Nam Thực Lục (Bản Gốc 1844)", DailyRentalPrice = 0m },
            new BadPhysicalBook { Id = 3, Title = "Design Patterns", DailyRentalPrice = 4000m }
        };

        var processor = new LspViolationProcessor();

        // ACT: Xử lý mượn đa hình
        var (successMessages, errorMessage) = processor.ProcessBatchBorrow(resources, "Lê Văn A", 3);

        // ASSERT: Chu trình bị đứt gãy giữa chừng do BadReferenceOnlyBook ném InvalidOperationException
        Assert.Single(successMessages); // Chỉ cuốn đầu tiên thành công
        Assert.NotNull(errorMessage);
        Assert.Contains("SẬP HỆ THỐNG DO VI PHẠM LSP", errorMessage);
        Assert.Contains("TUYỆT ĐỐI KHÔNG CHO MƯỢN VỀ NHÀ", errorMessage);
    }

    [Fact]
    public void LspCompliant_WhenDerivedClassesFulfillContract_ShouldSubstituteSeamlessly()
    {
        // ARRANGE: Danh sách các đối tượng khác nhau nhưng đều hiện thực IBorrowableResource
        var borrowableItems = new List<IBorrowableResource>
        {
            new PhysicalBorrowableBook
            {
                Id = 1,
                Title = "Clean Code",
                Isbn = "978-0132350884",
                DailyRentalRate = 3000m
            },
            new AudioBookPlayerDevice
            {
                Id = 2,
                Title = "Thiết Bị Sách Nói Sapiens",
                DeviceSerialNumber = "SN-DEV-9082",
                DailyRentalRate = 5000m
            }
        };

        var cleanProcessor = new LspCleanBorrowProcessor();

        // ACT: Xử lý mượn đa hình
        var (results, summary) = cleanProcessor.ProcessBatchBorrow(borrowableItems, "Nguyễn Thị Mai", 5);

        // ASSERT: Cả 2 đối tượng thay thế cho nhau hoàn hảo, không có lỗi ngoại lệ
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.IsSuccess));
        Assert.Contains("LSP Thành Công", summary);
        Assert.Equal(15000m, results[0].EstimatedFee); // 5 * 3000
        Assert.Equal(27000m, results[1].EstimatedFee); // (5 * 5000) + 2000 phụ phí
    }

    [Fact]
    public void ReferenceResource_SegregatedCorrectly_AllowsDeskReservation()
    {
        var manuscript = new SpecialArchiveManuscript
        {
            Id = 10,
            Title = "Bản đồ Cổ Hoàng Triều",
            LocationDescription = "Kho báu tầng hầm"
        };

        string reservation = manuscript.ReserveReadingDesk("Phan Anh", 2);

        Assert.Contains("Đã xếp chỗ đọc tại bàn chuyên dụng", reservation);
        Assert.Contains("Phan Anh", reservation);
    }
}
