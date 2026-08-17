using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class RareBookFeeStrategy : ILateFeeStrategy
{
    public int Priority => 60;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Rare;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(RareBookFeeStrategy)
        };

        if (context.DaysLate <= 0)
        {
            result.AppliedRules.Add("Trả đúng hạn: Phí = 0 VNĐ.");
            return result;
        }

        // Tính phạt lũy tiến cho sách hiếm:
        // 7 ngày đầu: 20,000 VNĐ / ngày
        // Từ ngày thứ 8: 40,000 VNĐ / ngày (Hệ số nhân tăng dần)
        decimal fee = 0m;
        int normalDays = Math.Min(context.DaysLate, 7);
        int penaltyDays = Math.Max(0, context.DaysLate - 7);

        fee += normalDays * 20000m;
        result.AppliedRules.Add($"Phạt 7 ngày đầu ({normalDays} ngày x 20,000 VNĐ) = {normalDays * 20000m:N0} VNĐ.");

        if (penaltyDays > 0)
        {
            decimal penaltyAmount = penaltyDays * 40000m * 1.5m; // Phạt nặng lũy tiến 1.5x
            fee += penaltyAmount;
            result.AppliedRules.Add($"Phạt lũy tiến từ ngày thứ 8 trở đi ({penaltyDays} ngày x 40,000 VNĐ x 1.5x) = {penaltyAmount:N0} VNĐ.");
        }

        if (context.DaysLate > 14)
        {
            decimal restorationFee = 50000m;
            fee += restorationFee;
            result.AppliedRules.Add($"Trễ quá 14 ngày đối với Sách Hiếm: Thu thêm phí bảo dưỡng phục chế thư tịch ({restorationFee:N0} VNĐ).");
        }

        result.BaseFee = fee;

        // Giới hạn trần tối đa không được vượt quá 100% Giá trị gốc của sách (BasePrice)
        decimal maxCap = context.Book.BasePrice > 0 ? context.Book.BasePrice : 500000m;
        if (fee > maxCap)
        {
            result.AppliedRules.Add($"Áp dụng trần khống chế tối đa (không quá 100% giá trị gốc của sách = {maxCap:N0} VNĐ): Giảm từ {fee:N0} VNĐ xuống {maxCap:N0} VNĐ.");
            result.DiscountAmount = fee - maxCap;
            result.FinalFee = maxCap;
        }
        else
        {
            result.FinalFee = fee;
        }

        return result;
    }
}
