using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class StaffExemptionFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Miễn phí Cán bộ / Giảng viên";
    public int Priority => 100; // Ưu tiên cao nhất

    public bool CanApply(FeeCalculationContext context)
    {
        return context.MemberType == MemberType.Staff;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>
        {
            "Độc giả là Cán bộ / Nhân viên thư viện: Miễn 100% toàn bộ phí phạt trễ hạn."
        };

        return new FeeCalculationResult
        {
            StrategyName = StrategyName,
            BaseFee = context.DaysLate * 3000m,
            DiscountAmount = context.DaysLate * 3000m,
            FinalFee = 0m,
            AppliedRules = rules
        };
    }
}
