using Library.Business.Models;

namespace Library.Business.Strategies;

/// <summary>
/// Strategy Pattern Nâng Cao (Tuân thủ Open-Closed Principle):
/// 1. Priority: Thứ tự ưu tiên kiểm tra chiến lược (Số càng lớn, ưu tiên càng cao).
/// 2. CanApply: Điều kiện kích hoạt động (Dynamic Predicate) dựa trên Context đa tiêu chí.
/// 3. CalculateFee: Tính toán và trả về kết quả kèm danh sách giải trình chi tiết.
/// </summary>
public interface ILateFeeStrategy
{
    int Priority { get; }
    bool CanApply(FeeCalculationContext context);
    FeeCalculationResult CalculateFee(FeeCalculationContext context);
}
