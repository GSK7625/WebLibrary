using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class StaffExemptionFeeStrategy : ILateFeeStrategy
{
    public int Priority => 100; // Ưu tiên cao nhất

    public bool CanApply(FeeCalculationContext context)
    {
        return context.MemberType == MemberType.Staff;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(StaffExemptionFeeStrategy),
            BaseFee = 0,
            DiscountAmount = 0,
            FinalFee = 0
        };

        result.AppliedRules.Add("Độc giả là Cán bộ / Nhân viên thư viện: Miễn 100% phí phạt trễ.");
        return result;
    }
}
