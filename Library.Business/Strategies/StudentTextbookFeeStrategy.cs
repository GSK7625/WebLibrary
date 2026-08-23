using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class StudentTextbookFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Hỗ trợ Sinh viên Mượn Giáo Trình";
    public int Priority => 90;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.MemberType == MemberType.Student && context.Book.Type == BookType.Textbook;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();
        decimal dailyRate = 1500m; // Giá ưu đãi 50% so với 3000 thông thường
        decimal baseFee = context.DaysLate * dailyRate;
        rules.Add($"Phí ưu đãi SV mượn giáo trình: {context.DaysLate} ngày x {dailyRate:N0} VND = {baseFee:N0} VND");

        decimal discount = 0m;
        if (context.DaysLate <= 3)
        {
            discount = baseFee;
            rules.Add("Chính sách hỗ trợ học tập: Miễn phí hoàn toàn nếu chỉ trễ dưới 3 ngày.");
        }

        decimal finalFee = baseFee - discount;
        return new FeeCalculationResult
        {
            StrategyName = StrategyName,
            BaseFee = baseFee,
            DiscountAmount = discount,
            FinalFee = finalFee,
            AppliedRules = rules
        };
    }
}
