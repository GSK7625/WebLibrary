using Library.Domain.Enums;
namespace Library.Domain.Entities;
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public BookType Type { get; set; }
    public bool IsBorrowed { get; set; }
}

