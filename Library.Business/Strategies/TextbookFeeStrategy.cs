using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class TextbookFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Giáo Trình (Độc giả thông thường)";
    public int Priority => 40;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Textbook;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();
        decimal dailyRate = 4000m;
        decimal baseFee = context.DaysLate * dailyRate;
        rules.Add($"Phí giáo trình: {context.DaysLate} ngày x {dailyRate:N0} VND = {baseFee:N0} VND");

        decimal finalFee = Math.Min(baseFee, context.Book.BasePrice);
        rules.Add($"Tổng phí: {finalFee:N0} VND (Giới hạn tối đa bằng giá bìa: {context.Book.BasePrice:N0} VND)");

        return new FeeCalculationResult
        {
            StrategyName = StrategyName,
            BaseFee = baseFee,
            DiscountAmount = 0m,
            FinalFee = finalFee,
            AppliedRules = rules
        };
    }
}
