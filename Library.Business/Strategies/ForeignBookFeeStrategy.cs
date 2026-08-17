using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class ForeignBookFeeStrategy : ILateFeeStrategy
{
    public int Priority => 50;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Foreign;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(ForeignBookFeeStrategy)
        };

        if (context.DaysLate <= 0)
        {
            result.AppliedRules.Add("Trả đúng hạn: Phí = 0 VNĐ.");
            return result;
        }

        decimal baseRate = 12000m;
        decimal baseFee = context.DaysLate * baseRate;
        result.BaseFee = baseFee;
        result.AppliedRules.Add($"Sách Ngoại văn: {context.DaysLate} ngày x {baseRate:N0} VNĐ/ngày = {baseFee:N0} VNĐ.");

        // Phụ phí quy đổi ngoại tệ và nhập khẩu (10%)
        decimal importSurcharge = baseFee * 0.10m;
        result.AppliedRules.Add($"Phụ phí lưu kho & bảo quản sách nhập khẩu (10%): +{importSurcharge:N0} VNĐ.");

        result.FinalFee = baseFee + importSurcharge;
        return result;
    }
}
