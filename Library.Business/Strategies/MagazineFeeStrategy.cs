using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class MagazineFeeStrategy : ILateFeeStrategy
{
    public int Priority => 40;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Magazine;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(MagazineFeeStrategy)
        };

        if (context.DaysLate <= 0)
        {
            result.AppliedRules.Add("Trả đúng hạn: Phí = 0 VNĐ.");
            return result;
        }

        decimal dailyRate = 2000m;
        decimal total = context.DaysLate * dailyRate;
        result.BaseFee = total;
        result.FinalFee = total;
        result.AppliedRules.Add($"Tạp chí định kỳ: {context.DaysLate} ngày x {dailyRate:N0} VNĐ/ngày = {total:N0} VNĐ.");

        return result;
    }
}
