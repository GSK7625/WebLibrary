using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class VIPMemberFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Hội Viên VIP";
    public int Priority => 95; // Ưu tiên rất cao

    public bool CanApply(FeeCalculationContext context)
    {
        return context.MemberType == MemberType.VIP;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();

        // Đặc quyền VIP 1: Ân hạn 3 ngày đầu tiên không tính phí
        int effectiveDaysLate = Math.Max(0, context.DaysLate - 3);
        rules.Add($"Đặc quyền VIP: Ân hạn 3 ngày đầu tiên miễn phí (Số ngày tính phí: {effectiveDaysLate}/{context.DaysLate} ngày)");

        decimal dailyRate = 3000m;
        decimal rawBaseFee = effectiveDaysLate * dailyRate;

        // Đặc quyền VIP 2: Giảm thêm 30% trên tổng phí
        decimal discountAmount = rawBaseFee * 0.3m;
        decimal finalFee = rawBaseFee - discountAmount;
        rules.Add($"Đặc quyền VIP: Giảm 30% tổng phí ({discountAmount:N0} VND)");
        rules.Add($"Tổng phí sau ưu đãi VIP: {finalFee:N0} VND");

        return new FeeCalculationResult
        {
            StrategyName = StrategyName,
            BaseFee = rawBaseFee,
            DiscountAmount = discountAmount,
            FinalFee = finalFee,
            AppliedRules = rules
        };
    }
}
