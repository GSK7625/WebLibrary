using Library.Business.Models;
using Library.Business.Services;
using Library.Business.Strategies;
using Library.DataAccess.Entities;
using Library.DataAccess.Enums;
using Xunit;

namespace Library.Tests;

public class OcpStrategyTests
{
    private readonly LateFeeApplicationService _ocpFeeService;

    public OcpStrategyTests()
    {
        var strategies = new List<ILateFeeStrategy>
        {
            new StaffExemptionFeeStrategy(),
            new VIPMemberFeeStrategy(),
            new StudentTextbookFeeStrategy(),
            new RareBookFeeStrategy(),
            new ForeignBookFeeStrategy(),
            new AudioBookFeeStrategy(),
            new MagazineFeeStrategy(),
            new TextbookFeeStrategy(),
            new RegularBookFeeStrategy()
        };

        _ocpFeeService = new LateFeeApplicationService(strategies);
    }

    [Fact]
    public void OnTimeReturn_ShouldHaveZeroFee()
    {
        var book = new Book { Title = "Sách thường", Type = BookType.Regular, BasePrice = 100000m };
        var result = _ocpFeeService.CalculateFee(book, daysLate: 0, memberType: MemberType.Standard);

        Assert.Equal(0, result.FinalFee);
        Assert.Equal(0, result.BaseFee);
        Assert.Equal("None", result.StrategyName);
        Assert.Contains(result.AppliedRules, r => r.Contains("Trả đúng hạn"));
    }

    [Fact]
    public void StaffMember_ShouldBeExemptFromAllLateFees_RegardlessOfBookType()
    {
        var book = new Book { Title = "Sách cổ quý hiếm", Type = BookType.Rare, BasePrice = 800000m };
        var result = _ocpFeeService.CalculateFee(book, daysLate: 15, memberType: MemberType.Staff);

        Assert.Equal(0m, result.FinalFee);
        Assert.Equal("Chính sách Miễn phí Cán bộ / Giảng viên", result.StrategyName);
        Assert.Contains(result.AppliedRules, r => r.Contains("Cán bộ / Nhân viên"));
    }

    [Fact]
    public void VIPMember_ShouldGet3DaysGracePeriodAnd30PercentDiscount()
    {
        var book = new Book { Title = "Sách thường", Type = BookType.Regular, BasePrice = 100000m };
        // 5 ngày trễ -> ân hạn 3 ngày -> tính 2 ngày x 3,000 = 6,000 VNĐ -> giảm 30% (1,800) = 4,200 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 5, memberType: MemberType.VIP);

        Assert.Equal(6000m, result.BaseFee);
        Assert.Equal(1800m, result.DiscountAmount);
        Assert.Equal(4200m, result.FinalFee);
        Assert.Equal("Chính sách Hội Viên VIP", result.StrategyName);
    }

    [Fact]
    public void Student_Textbook_ShouldBeFreeIfWithin3Days()
    {
        var book = new Book { Title = "Giáo trình C#", Type = BookType.Textbook, BasePrice = 150000m };
        // 2 ngày trễ -> SV mượn giáo trình <= 3 ngày được miễn phí
        var result = _ocpFeeService.CalculateFee(book, daysLate: 2, memberType: MemberType.Student);

        Assert.Equal(3000m, result.BaseFee);
        Assert.Equal(3000m, result.DiscountAmount);
        Assert.Equal(0m, result.FinalFee);
        Assert.Equal("Chính sách Hỗ trợ Sinh viên Mượn Giáo Trình", result.StrategyName);
    }

    [Fact]
    public void Student_Textbook_ShouldChargeDiscountedRate_WhenMoreThan3Days()
    {
        var book = new Book { Title = "Giáo trình Giải thuật", Type = BookType.Textbook, BasePrice = 120000m };
        // 10 ngày trễ: 10 x 1500 = 15,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 10, memberType: MemberType.Student);

        Assert.Equal(15000m, result.BaseFee);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(15000m, result.FinalFee);
        Assert.Equal("Chính sách Hỗ trợ Sinh viên Mượn Giáo Trình", result.StrategyName);
    }

    [Fact]
    public void RareBook_ShouldIncludeSurcharge_WhenLateMoreThan7Days()
    {
        var book = new Book { Title = "Đại Việt Sử Ký", Type = BookType.Rare, BasePrice = 500000m };
        // 10 ngày trễ: 10 x 10,000 = 100,000 + 50,000 phụ phí = 150,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 10, memberType: MemberType.Standard);

        Assert.Equal(100000m, result.BaseFee);
        Assert.Equal(150000m, result.FinalFee);
        Assert.Equal("Chính sách Sách Hiếm / Cổ", result.StrategyName);
    }

    [Fact]
    public void ForeignBook_ShouldApplyDailyRateAndCapAt150Percent()
    {
        var book = new Book { Title = "Clean Architecture", Type = BookType.Foreign, BasePrice = 300000m };
        // 5 ngày trễ: 5 x 7,000 = 35,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 5, memberType: MemberType.Standard);

        Assert.Equal(35000m, result.FinalFee);
        Assert.Equal("Chính sách Sách Ngoại Văn (Foreign Book)", result.StrategyName);
    }

    [Fact]
    public void AudioBook_ShouldCalculateCorrectFeeAndSurcharge()
    {
        var book = new Book { Title = "Audiobook Sapiens", Type = BookType.Audio, BasePrice = 180000m };
        // 12 ngày trễ (> 10 ngày): 12 x 4,000 = 48,000 + 15,000 phụ phí = 63,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 12, memberType: MemberType.Standard);

        Assert.Equal(48000m, result.BaseFee);
        Assert.Equal(63000m, result.FinalFee);
        Assert.Equal("Chính sách Sách Nói & Thiết Bị AudioBook", result.StrategyName);
    }

    [Fact]
    public void Magazine_ShouldCapAtBasePrice()
    {
        var book = new Book { Title = "Tạp Chí CNTT", Type = BookType.Magazine, BasePrice = 50000m };
        // 35 ngày trễ: 35 x 2,000 = 70,000 -> Giới hạn trần bằng BasePrice = 50,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 35, memberType: MemberType.Standard);

        Assert.Equal(70000m, result.BaseFee);
        Assert.Equal(50000m, result.FinalFee);
        Assert.Equal("Chính sách Báo & Tạp Chí", result.StrategyName);
    }

    [Fact]
    public void RegularBook_FallbackStrategy_ShouldCapAtBasePrice()
    {
        var book = new Book { Title = "Sách thường A", Type = BookType.Regular, BasePrice = 50000m };
        // 20 ngày trễ: 20 x 3,000 = 60,000 -> Giới hạn trần = 50,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 20, memberType: MemberType.Standard);

        Assert.Equal(60000m, result.BaseFee);
        Assert.Equal(50000m, result.FinalFee);
        Assert.Equal("Chính sách Sách Thường (Mặc định)", result.StrategyName);
    }

    [Fact]
    public void OcpExtensibility_CanAddNewCustomStrategy_WithoutModifyingExistingCode()
    {
        // MINH CHỨNG OCP: Thêm chiến lược khuyến mãi Tết mà KHÔNG sửa 1 dòng code cũ nào trong LateFeeApplicationService
        var holidayStrategy = new MockHolidayPromoStrategy();
        var strategies = new List<ILateFeeStrategy>
        {
            holidayStrategy,
            new RegularBookFeeStrategy()
        };

        var extendedService = new LateFeeApplicationService(strategies);
        var book = new Book { Title = "Sách bất kỳ", Type = BookType.Regular, BasePrice = 100000m };

        var result = extendedService.CalculateFee(book, daysLate: 5, memberType: MemberType.Standard);

        Assert.Equal(999m, result.FinalFee);
        Assert.Equal("Chiến lược Khuyến Mãi Tết (Mở rộng OCP)", result.StrategyName);
    }

    private class MockHolidayPromoStrategy : ILateFeeStrategy
    {
        public int Priority => 999; // Ưu tiên cao nhất
        public bool CanApply(FeeCalculationContext context) => true;
        public FeeCalculationResult CalculateFee(FeeCalculationContext context) => new()
        {
            StrategyName = "Chiến lược Khuyến Mãi Tết (Mở rộng OCP)",
            BaseFee = 999m,
            FinalFee = 999m,
            AppliedRules = new List<string> { "Đồng giá Tết 999 VNĐ" }
        };
    }
}
