using Library.Business.Models;
using Library.DataAccess.Enums;

namespace Library.Business.Strategies;

/// <summary>
/// Chiến lược tính phí phạt cho Sách Nói / Thiết bị AudioBook (Chuẩn OCP).
/// </summary>
public class AudioBookFeeStrategy : ILateFeeStrategy
{
    public string StrategyName => "Chính sách Sách Nói & Thiết Bị AudioBook";
    public int Priority => 60;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.Book.Type == BookType.Audio;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var rules = new List<string>();
        decimal dailyRate = 4000m; // 4.000 VNĐ / ngày trễ
        decimal baseFee = context.DaysLate * dailyRate;
        rules.Add($"Phí sách nói cơ bản: {context.DaysLate} ngày x {dailyRate:N0} VND = {baseFee:N0} VND");

        decimal surcharge = 0m;
        if (context.DaysLate > 10)
        {
            surcharge = 15000m;
            rules.Add($"Phụ phí khấu hao thiết bị/bản quyền audio trễ hạn trên 10 ngày: +{surcharge:N0} VND");
        }

        // Giới hạn tối đa 120% giá trị thiết bị/sách
        decimal maxCap = context.Book.BasePrice * 1.2m;
        decimal finalFee = Math.Min(baseFee + surcharge, maxCap);
        rules.Add($"Tổng phí cuối cùng: {finalFee:N0} VND (Giới hạn trần tối đa 120% giá trị gốc: {maxCap:N0} VND)");

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
