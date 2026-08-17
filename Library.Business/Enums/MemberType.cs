namespace Library.Business.Enums;

/// <summary>
/// Loai hoi vien thu vien
/// </summary>
public enum MemberType
{
    Standard = 1, // Độc giả phổ thông
    Student = 2,  // Sinh viên (được ưu đãi giáo trình)
    VIP = 3,      // Hội viên VIP (ân hạn & giảm phí)
    Staff = 4     // Cán bộ / Nhân viên (miễn phí phạt)
}
