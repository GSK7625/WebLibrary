using Library.Business.Entities;
using Library.Business.Enums;
using Library.Business.Legacy;
using Library.Business.Services;
using Library.Business.Strategies;
using Xunit;

namespace Library.Tests;

public class OcpStrategyTests
{
    private readonly LateFeeApplicationService _ocpFeeService;
    private readonly LegacyFeeCalculator _legacyCalculator;

    public OcpStrategyTests()
    {
        var strategies = new List<ILateFeeStrategy>
        {
            new StaffExemptionFeeStrategy(),
            new VIPMemberFeeStrategy(),
            new StudentTextbookFeeStrategy(),
            new RareBookFeeStrategy(),
            new ForeignBookFeeStrategy(),
            new MagazineFeeStrategy(),
            new TextbookFeeStrategy(),
            new RegularBookFeeStrategy()
        };

        _ocpFeeService = new LateFeeApplicationService(strategies);
        _legacyCalculator = new LegacyFeeCalculator();
    }

    [Fact]
    public void StaffMember_ShouldBeExemptFromAllLateFees()
    {
        var book = new Book { Title = "Sách hiếm", Type = BookType.Rare, BasePrice = 500000m };
        var result = _ocpFeeService.CalculateFee(book, daysLate: 10, memberType: MemberType.Staff);

        Assert.Equal(0, result.FinalFee);
        Assert.Equal(nameof(StaffExemptionFeeStrategy), result.StrategyName);
        Assert.Contains(result.AppliedRules, r => r.Contains("Cán bộ / Nhân viên"));
    }

    [Fact]
    public void VIPMember_ShouldGet3DaysGracePeriodAnd30PercentDiscount()
    {
        var book = new Book { Title = "Sách thường", Type = BookType.Regular, BasePrice = 100000m };
        // 5 ngày trễ -> ân hạn 3 ngày -> tính 2 ngày x 10,000 = 20,000 VNĐ -> giảm 30% (6,000) = 14,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 5, memberType: MemberType.VIP);

        Assert.Equal(20000m, result.BaseFee);
        Assert.Equal(6000m, result.DiscountAmount);
        Assert.Equal(14000m, result.FinalFee);
        Assert.Equal(nameof(VIPMemberFeeStrategy), result.StrategyName);
    }

    [Fact]
    public void Student_Textbook_ShouldGet50PercentDiscountAndCapAt50k()
    {
        var book = new Book { Title = "Giáo trình C#", Type = BookType.Textbook, BasePrice = 150000m };
        // 40 ngày trễ x 4,000 = 160,000 VNĐ -> giảm 50% = 80,000 VNĐ -> Bị khống chế trần cap 50,000 VNĐ
        var result = _ocpFeeService.CalculateFee(book, daysLate: 40, memberType: MemberType.Student);

        Assert.Equal(160000m, result.BaseFee);
        Assert.Equal(80000m, result.DiscountAmount);
        Assert.Equal(50000m, result.FinalFee);
        Assert.Equal(nameof(StudentTextbookFeeStrategy), result.StrategyName);
    }

    [Fact]
    public void RareBook_TieredPricing_ShouldCapAtBasePrice()
    {
        var book = new Book { Title = "Đại Việt Sử Ký", Type = BookType.Rare, BasePrice = 300000m };
        // 10 ngày trễ: 7 ngày x 20k = 140k. 3 ngày x 40k x 1.5 = 180k. Tổng = 320k > BasePrice 300k -> Cap = 300k
        var result = _ocpFeeService.CalculateFee(book, daysLate: 10, memberType: MemberType.Standard);

        Assert.Equal(300000m, result.FinalFee);
        Assert.Equal(nameof(RareBookFeeStrategy), result.StrategyName);
    }

    [Theory]
    [InlineData(BookType.Regular, 5, MemberType.Standard)]
    [InlineData(BookType.Rare, 10, MemberType.Standard)]
    [InlineData(BookType.Foreign, 4, MemberType.Standard)]
    [InlineData(BookType.Textbook, 10, MemberType.Student)]
    [InlineData(BookType.Magazine, 3, MemberType.VIP)]
    [InlineData(BookType.Regular, 15, MemberType.Staff)]
    public void OcpStrategyResult_ShouldMatchLegacyCalculatorResult(BookType bookType, int daysLate, MemberType memberType)
    {
        var book = new Book { Title = "Test Book", Type = bookType, BasePrice = 300000m };

        var ocpResult = _ocpFeeService.CalculateFee(book, daysLate, memberType);
        var legacyResult = _legacyCalculator.CalculateLateFeeLegacy(book, daysLate, memberType);

        Assert.Equal(legacyResult.FinalFee, ocpResult.FinalFee);
    }
}
