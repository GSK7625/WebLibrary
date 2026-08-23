using Library.DataAccess.Entities;
using Library.DataAccess.Enums;

namespace Library.Business.Models;

/// <summary>
/// Ngữ cảnh chứa toàn bộ dữ liệu cần thiết để tính phí phạt trả trễ.
/// Giúp OCP nâng cao: Khi thêm thông tin mới (VD: IsHoliday, MemberType...),
/// ta chỉ cần mở rộng Context mà KHÔNG phải đổi tham số của interface ILateFeeStrategy.
/// </summary>
public class FeeCalculationContext
{
    public Book Book { get; set; } = default!;
    public MemberType MemberType { get; set; } = MemberType.Standard;
    public int DaysLate { get; set; }
    public DateTime DueDate { get; set; } = DateTime.Now.AddDays(-7);
    public DateTime ReturnedDate { get; set; } = DateTime.Now;
    public bool IsDamaged { get; set; } = false;
}
