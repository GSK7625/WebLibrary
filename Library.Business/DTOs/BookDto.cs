using Library.Business.Enums;

namespace Library.Business.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public BookType Type { get; set; }
    public string TypeName => Type.ToString();
    public bool IsBorrowed { get; set; }
}

