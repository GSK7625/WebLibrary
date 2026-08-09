using System.ComponentModel.DataAnnotations;

namespace Library.Business.DTOs;

public class BorrowRequestDto
{
    [Required(ErrorMessage = "Id cua sach khong duoc de trong")]
    [Range(1, int.MaxValue, ErrorMessage = "Id cua sach phai lon hon 0")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "Ten nguoi muon khong duoc de trong")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ten nguoi muon phai tu 2 den 100 ky tu")]
    public string BorrowerName { get; set; } = string.Empty;

    [Range(1, 365, ErrorMessage = "So ngay muon phai nam trong khoang tu 1 den 365 ngay")]
    public int BorrowDays { get; set; } = 7;
}

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

public class ReturnBookRequestDto
{
    public DateTime? ReturnedDate { get; set; }
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
    public decimal LateFee { get; set; }
    public string FeeCalculationMethod { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class FeePreviewDto
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookType { get; set; } = string.Empty;
    public int DaysLate { get; set; }
    public decimal Fee { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
