using Library.Business.Entities;
using Library.Business.Enums;
using Library.Business.Interfaces;
using Library.Business.Models;
using Library.Business.Strategies;

namespace Library.Business.Services;

/// <summary>
/// Application Service điều phối tính phí trả trễ tuân thủ OCP.
/// 
/// ĐIỂM SÁNG NGUYÊN LÝ OCP:
/// - Class này KHÔNG BAO GIỜ bị sửa đổi (Closed for modification) khi thêm chính sách tính phí mới.
/// - Nó chỉ phụ thuộc vào abstraction `ILateFeeStrategy`.
/// - Khi cần mở rộng chính sách tính phí (Open for extension): Tạo thêm class Strategy mới và đăng ký DI.
/// </summary>
public class LateFeeApplicationService : ILateFeeApplicationService
{
    private readonly IEnumerable<ILateFeeStrategy> _strategies;

    public LateFeeApplicationService(IEnumerable<ILateFeeStrategy> strategies)
    {
        // Sắp xếp các Strategy theo thứ tự ưu tiên (Priority) giảm dần khi khởi tạo
        _strategies = strategies.OrderByDescending(s => s.Priority);
    }

    public FeeCalculationResult CalculateFee(FeeCalculationContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.DaysLate <= 0)
        {
            return new FeeCalculationResult
            {
                StrategyName = "None",
                BaseFee = 0,
                DiscountAmount = 0,
                FinalFee = 0,
                AppliedRules = new List<string> { "Trả đúng hạn: Khai báo Phí = 0 VNĐ." }
            };
        }

        // Tìm Strategy phù hợp nhất đầu tiên theo Priority
        var strategy = _strategies.FirstOrDefault(s => s.CanApply(context));
        if (strategy == null)
        {
            throw new InvalidOperationException($"Không tìm thấy Strategy phù hợp cho ngữ cảnh: BookType={context.Book?.Type}, MemberType={context.MemberType}");
        }

        return strategy.CalculateFee(context);
    }

    public FeeCalculationResult CalculateFee(Book book, int daysLate, MemberType memberType = MemberType.Standard)
    {
        var context = new FeeCalculationContext
        {
            Book = book,
            DaysLate = daysLate,
            MemberType = memberType
        };

        return CalculateFee(context);
    }
}
