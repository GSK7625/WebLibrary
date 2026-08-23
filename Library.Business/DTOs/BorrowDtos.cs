using System.ComponentModel.DataAnnotations;
using Library.DataAccess.Enums;

namespace Library.Business.DTOs;

public class BorrowRecordDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public decimal? LateFee { get; set; }
}

public class BorrowRequestDto
{
    [Required(ErrorMessage = "Vui lòng nhập BookId")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên người mượn")]
    public string BorrowerName { get; set; } = string.Empty;

    [Range(1, 90, ErrorMessage = "Số ngày mượn phải từ 1 đến 90 ngày")]
    public int BorrowDays { get; set; } = 14;
}

public class ReturnBookRequestDto
{
    public DateTime? ReturnedDate { get; set; }
    public MemberType MemberType { get; set; } = MemberType.Standard;
}

public class ReturnBookResponseDto
{
    public int BorrowRecordId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime ReturnedDate { get; set; }
    public int DaysLate { get; set; }
    public decimal BaseFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LateFee { get; set; }
    public string FeeCalculationMethod { get; set; } = string.Empty;
    public List<string> AppliedRules { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
