using Library.DataAccess.Enums;

namespace Library.Business.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public BookType Type { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsBorrowed { get; set; }
}

public class FeePreviewDto
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookType { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public MemberType MemberType { get; set; }
    public int DaysLate { get; set; }
    public decimal BaseFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalFee { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public List<string> AppliedRules { get; set; } = new();
    public string Method { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
