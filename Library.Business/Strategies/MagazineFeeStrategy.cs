using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class MagazineFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Báo & Tạp Chí";
    public int Priority => 50;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Magazine;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();
        decimal dailyRate = 2000m;
        decimal baseFee = context.DaysLate * dailyRate;
        rules.Add($"Phí báo/tạp chí: {context.DaysLate} ngày x {dailyRate:N0} VND = {baseFee:N0} VND");

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
