using System.ComponentModel.DataAnnotations;

namespace Library.Business.DTOs;

public class BorrowRequestDto
{
    [Required(ErrorMessage = "Id c?a sách không du?c d? tr?ng")]
    [Range(1, int.MaxValue, ErrorMessage = "Id c?a sách ph?i l?n hon 0")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "Tên ngu?i mu?n không du?c d? tr?ng")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên ngu?i mu?n ph?i t? 2 d?n 100 ký t?")]
    public string BorrowerName { get; set; } = string.Empty;

    [Range(1, 365, ErrorMessage = "S? ngày mu?n ph?i n?m trong kho?ng t? 1 d?n 365 ngày")]
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

