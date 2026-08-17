using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class VIPMemberFeeStrategy : ILateFeeStrategy
{
    public int Priority => 80;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.MemberType == MemberType.VIP;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(VIPMemberFeeStrategy)
        };

        if (context.DaysLate <= 0)
        {
            result.AppliedRules.Add("Trả đúng hạn: Phí = 0 VNĐ.");
            return result;
        }

        // Chính sách VIP: Ân hạn 3 ngày đầu
        int graceDays = 3;
        int chargeableDays = Math.Max(0, context.DaysLate - graceDays);

        result.AppliedRules.Add($"Độc giả VIP được ân hạn {graceDays} ngày đầu tiên không tính phạt.");

        if (chargeableDays == 0)
        {
            result.AppliedRules.Add("Số ngày trễ nằm trong khoảng ân hạn VIP: Miễn phí.");
            result.FinalFee = 0;
            return result;
        }

        decimal dailyRate = 10000m;
        decimal baseFee = chargeableDays * dailyRate;
        result.BaseFee = baseFee;
        result.AppliedRules.Add($"Tính phí cho {chargeableDays} ngày vượt hạn x {dailyRate:N0} VNĐ/ngày = {baseFee:N0} VNĐ.");

        // Giảm giá 30% tổng phí cho VIP
        decimal discount = baseFee * 0.30m;
        result.DiscountAmount = discount;
        result.FinalFee = baseFee - discount;
        result.AppliedRules.Add($"Ưu đãi VIP: Giảm 30% tổng phí (-{discount:N0} VNĐ).");

        return result;
    }
}
