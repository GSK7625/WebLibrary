using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class RegularBookFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Sách Thường (Mặc định)";
    public int Priority => 10; // Thấp nhất, áp dụng nếu không có chiến lược nào khác

    public bool CanApply(FeeCalculationContext context)
    {
        return true; // Phù hợp cho mọi loại sách
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();
        decimal dailyRate = 3000m;
        decimal baseFee = context.DaysLate * dailyRate;
        rules.Add($"Phí cơ bản: {context.DaysLate} ngày x {dailyRate:N0} VND = {baseFee:N0} VND");

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
