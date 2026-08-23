using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

public class RareBookFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Sách Hiếm / Cổ";
    public int Priority => 70;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Rare;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();
        decimal baseRate = 10000m;
        decimal baseFee = context.DaysLate * baseRate;
        rules.Add($"Phí sách hiếm cơ bản: {context.DaysLate} ngày x {baseRate:N0} VND = {baseFee:N0} VND");

        decimal surcharge = 0m;
        if (context.DaysLate > 7)
        {
            surcharge = 50000m;
            rules.Add($"Phạt trễ quá 7 ngày đối với sách hiếm: +{surcharge:N0} VND");
        }

        decimal finalFee = baseFee + surcharge;
        rules.Add($"Tổng phí: {finalFee:N0} VND (Không giới hạn tối đa do tính chất quý hiếm)");

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
