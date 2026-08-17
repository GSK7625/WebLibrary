using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Strategies;

public class StudentTextbookFeeStrategy : ILateFeeStrategy
{
    public int Priority => 70;

    public bool CanApply(FeeCalculationContext context)
    {
        return context.MemberType == MemberType.Student && context.Book.Type == BookType.Textbook;
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = nameof(StudentTextbookFeeStrategy)
        };

        if (context.DaysLate <= 0)
        {
            result.AppliedRules.Add("Trả đúng hạn: Phí = 0 VNĐ.");
            return result;
        }

        decimal dailyRate = 4000m;
        decimal baseFee = context.DaysLate * dailyRate;
        result.BaseFee = baseFee;
        result.AppliedRules.Add($"Độc giả Sinh viên mượn Giáo trình: {context.DaysLate} ngày x {dailyRate:N0} VNĐ/ngày = {baseFee:N0} VNĐ.");

        // Giảm 50% cho Sinh viên mượn Giáo trình học tập
        decimal discount = baseFee * 0.50m;
        result.DiscountAmount = discount;
        decimal feeAfterDiscount = baseFee - discount;
        result.AppliedRules.Add($"Ưu đãi Sinh viên học tập: Giảm 50% phí phạt (-{discount:N0} VNĐ).");

        // Khống chế trần phí tối đa 50,000 VNĐ hỗ trợ sinh viên
        decimal maxCap = 50000m;
        if (feeAfterDiscount > maxCap)
        {
            result.AppliedRules.Add($"Áp dụng chính sách khống chế trần phí tối đa cho Sinh viên: Giảm từ {feeAfterDiscount:N0} VNĐ xuống {maxCap:N0} VNĐ.");
            result.FinalFee = maxCap;
        }
        else
        {
            result.FinalFee = feeAfterDiscount;
        }

        return result;
    }
}
