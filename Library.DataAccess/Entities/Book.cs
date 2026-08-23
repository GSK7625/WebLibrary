using Library.DataAccess.Enums;

namespace Library.DataAccess.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public BookType Type { get; set; }
    public decimal BasePrice { get; set; } = 100000m;
    public bool IsBorrowed { get; set; }
}
