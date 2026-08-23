using Library.Business.Models;
using Library.DataAccess.Entities;
using Library.DataAccess.Enums;

namespace Library.Business.Legacy;

/// <summary>
/// VÍ DỤ VI PHẠM NGUYÊN LÝ OCP (OPEN-CLOSED PRINCIPLE):
/// Class này cố gắng ôm đồm tất cả logic tính phí phức tạp vào 1 hàm monolithic duy nhất.
///
/// HẬU QUẢ VI PHẠM OCP:
/// 1. Mỗi khi thư viện ra quy tắc mới (VD: Giảm giá Sinh viên, Ưu đãi VIP, Miễn phí Staff, Phạt lũy tiến),
///    lập trình viên BUỘC PHẢI SỬA CODE CŨ (Modify existing code).
/// 2. Hàm trở nên cồng kềnh với hàng chục nhánh `if-else` và `switch-case` lồng nhau.
/// 3. Rủi ro cực lớn làm hỏng các quy tắc tính phí cũ (Regression Bug) khi thêm quy tắc mới.
/// 4. Khó viết Unit Test vì không thể cô lập từng chính sách.
/// </summary>
public class LegacyFeeCalculator
{
    public FeeCalculationResult CalculateLateFeeLegacy(Book book, int daysLate, MemberType memberType)
    {
        var result = new FeeCalculationResult
        {
            StrategyName = "LegacyMonolithicCalculator (Vi pham OCP)"
        };

        if (daysLate <= 0)
        {
            result.AppliedRules.Add("[Legacy] Trả đúng hạn: Phí = 0 VNĐ.");
            result.FinalFee = 0;
            return result;
        }

        // VI PHẠM OCP: Nhánh 1 - Kiểm tra Nhân viên (Staff)
        if (memberType == MemberType.Staff)
        {
            result.AppliedRules.Add("[Legacy Bad Code] Check hardcode Staff: Miễn phí 100%.");
            result.FinalFee = 0;
            return result;
        }

        // VI PHẠM OCP: Nhánh 2 - Kiểm tra Độc giả VIP
        if (memberType == MemberType.VIP)
        {
            int graceDays = 3;
            int chargeableDays = Math.Max(0, daysLate - graceDays);
            result.AppliedRules.Add($"[Legacy Bad Code] Hardcode check VIP: Ân hạn {graceDays} ngày.");

            if (chargeableDays == 0)
            {
                result.FinalFee = 0;
                return result;
            }

            decimal baseFee = chargeableDays * 10000m;
            decimal discount = baseFee * 0.30m;
            result.BaseFee = baseFee;
            result.DiscountAmount = discount;
            result.FinalFee = baseFee - discount;
            result.AppliedRules.Add($"[Legacy Bad Code] VIP {chargeableDays} ngày x 10,000 - 30% giảm giá = {result.FinalFee:N0} VNĐ.");
            return result;
        }

        // VI PHẠM OCP: Nhánh 3 - Kiểm tra Sinh viên mượn Giáo trình
        if (memberType == MemberType.Student && book.Type == BookType.Textbook)
        {
            decimal baseFee = daysLate * 4000m;
            decimal discount = baseFee * 0.50m;
            decimal feeAfterDiscount = baseFee - discount;
            decimal maxCap = 50000m;

            result.BaseFee = baseFee;
            result.DiscountAmount = discount;

            if (feeAfterDiscount > maxCap)
            {
                result.FinalFee = maxCap;
                result.AppliedRules.Add($"[Legacy Bad Code] Hardcode Sinh viên + Giáo trình bị khống chế trần 50,000 VNĐ.");
            }
            else
            {
                result.FinalFee = feeAfterDiscount;
                result.AppliedRules.Add($"[Legacy Bad Code] Hardcode Sinh viên + Giáo trình giảm 50%: {feeAfterDiscount:N0} VNĐ.");
            }
            return result;
        }

        // VI PHẠM OCP: Nhánh 4 - Switch case khổng lồ theo loại sách
        switch (book.Type)
        {
            case BookType.Rare:
                // Sách hiếm lũy tiến
                decimal rareFee = 0m;
                int normalDays = Math.Min(daysLate, 7);
                int penaltyDays = Math.Max(0, daysLate - 7);
                rareFee += normalDays * 20000m;
                if (penaltyDays > 0)
                {
                    rareFee += penaltyDays * 40000m * 1.5m;
                }
                if (daysLate > 14)
                {
                    rareFee += 50000m;
                }

                decimal maxRareCap = book.BasePrice > 0 ? book.BasePrice : 500000m;
                if (rareFee > maxRareCap)
                {
                    rareFee = maxRareCap;
                }
                result.BaseFee = rareFee;
                result.FinalFee = rareFee;
                result.AppliedRules.Add($"[Legacy Bad Code] Switch-case Rare Book logic: {rareFee:N0} VNĐ.");
                break;

            case BookType.Foreign:
                decimal foreignBase = daysLate * 12000m;
                decimal importSurcharge = foreignBase * 0.10m;
                result.BaseFee = foreignBase;
                result.FinalFee = foreignBase + importSurcharge;
                result.AppliedRules.Add($"[Legacy Bad Code] Switch-case Foreign Book logic: {result.FinalFee:N0} VNĐ.");
                break;

            case BookType.Textbook:
                result.FinalFee = daysLate * 3000m;
                result.AppliedRules.Add($"[Legacy Bad Code] Switch-case Textbook logic: {result.FinalFee:N0} VNĐ.");
                break;

            case BookType.Magazine:
                result.FinalFee = daysLate * 2000m;
                result.AppliedRules.Add($"[Legacy Bad Code] Switch-case Magazine logic: {result.FinalFee:N0} VNĐ.");
                break;

            case BookType.Audio:
                result.FinalFee = daysLate * 4000m;
                result.AppliedRules.Add($"[Legacy Bad Code] Switch-case Audio logic: {result.FinalFee:N0} VNĐ.");
                break;

            case BookType.Regular:
            default:
                result.FinalFee = daysLate * 5000m;
                result.AppliedRules.Add($"[Legacy Bad Code] Switch-case Default Regular logic: {result.FinalFee:N0} VNĐ.");
                break;
        }

        return result;
    }
}
