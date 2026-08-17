using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class RegularBookFeeStrategy : ILateFeeStrategy
{
    public int Priority => 1; // Fallback mặc định

    public bool CanApply(FeeCalculationContext context)
    {
        return true; // Áp dụng mặc định cho tất cả các loại sách khác
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(RegularBookFeeStrategy)
        };

        if (context.DaysLate <= 0)
        {
            result.AppliedRules.Add("Trả đúng hạn: Phí = 0 VNĐ.");
            return result;
        }

        decimal dailyRate = 5000m;
        decimal total = context.DaysLate * dailyRate;
        result.BaseFee = total;
        result.FinalFee = total;
        result.AppliedRules.Add($"Sách tiêu chuẩn mặc định: {context.DaysLate} ngày x {dailyRate:N0} VNĐ/ngày = {total:N0} VNĐ.");

        return result;
    }
}
