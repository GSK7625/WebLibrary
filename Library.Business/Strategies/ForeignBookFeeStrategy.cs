using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class ForeignBookFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Sách Ngoại Văn (Foreign Book)";
    public int Priority => 80;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Foreign;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();
        decimal dailyRate = 7000m;
        decimal baseFee = context.DaysLate * dailyRate;
        rules.Add($"Phí cơ bản: {context.DaysLate} ngày x {dailyRate:N0} VND/ngày = {baseFee:N0} VND");

        decimal surcharge = 0m;
        if (context.DaysLate > 14)
        {
            surcharge = 25000m;
            rules.Add($"Phụ phí ngoại văn quá 14 ngày (bảo quản đặc thù): +{surcharge:N0} VND");
        }

        decimal finalFee = Math.Min(baseFee + surcharge, context.Book.BasePrice * 1.5m);
        rules.Add($"Tổng phí cuối cùng: {finalFee:N0} VND (Giới hạn tối đa 150% giá bìa: {context.Book.BasePrice * 1.5m:N0} VND)");

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
