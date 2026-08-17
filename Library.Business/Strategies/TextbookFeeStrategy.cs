using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class TextbookFeeStrategy : ILateFeeStrategy
{
    public int Priority => 30;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Textbook;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(TextbookFeeStrategy)
        };

        if (context.DaysLate <= 0)
        {
            result.AppliedRules.Add("Trả đúng hạn: Phí = 0 VNĐ.");
            return result;
        }

        decimal dailyRate = 3000m;
        decimal total = context.DaysLate * dailyRate;
        result.BaseFee = total;
        result.FinalFee = total;
        result.AppliedRules.Add($"Giáo trình học tập (Độc giả thông thường): {context.DaysLate} ngày x {dailyRate:N0} VNĐ/ngày = {total:N0} VNĐ.");

        return result;
    }
}
