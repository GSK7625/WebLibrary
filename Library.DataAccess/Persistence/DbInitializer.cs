using System.Text.Json;
using Library.Business.Entities;
using Library.Business.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.DataAccess.Persistence;

public static class DbInitializer
{
    public static async Task SeedDataAsync(LibraryDbContext context, HttpClient? httpClient = null, bool forceRecreate = false)
    {
        await context.Database.EnsureCreatedAsync();

        if (forceRecreate)
        {
            context.BorrowRecords.RemoveRange(context.BorrowRecords);
            context.Books.RemoveRange(context.Books);
            await context.SaveChangesAsync();
            Console.WriteLine("[DbInitializer] Đã xóa toàn bộ dữ liệu cũ thành công!");
        }
        else if (await context.Books.AnyAsync())
        {
            return;
        }

        var realBooks = new List<Book>();

        if (httpClient != null)
        {
            try
            {
                realBooks = await FetchBooksFromOpenLibraryApiAsync(httpClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer] OpenLibrary API: {ex.Message}");
            }
        }

        if (realBooks.Count == 0)
        {
            // Seed dữ liệu mẫu đa dạng nếu không fetch được từ API ngoài
            realBooks = GetFallbackSampleBooks();
        }

        await context.Books.AddRangeAsync(realBooks);
        await context.SaveChangesAsync();

        var savedBooks = await context.Books.ToListAsync();
        if (savedBooks.Count >= 5)
        {
            var now = DateTime.Now;
            var borrowRecords = new List<BorrowRecord>
            {
                new BorrowRecord
                {
                    BookId = savedBooks[0].Id,
                    BorrowerName = "Lê Văn Hùng (Sinh viên)",
                    BorrowDate = now.AddDays(-15),
                    DueDate = now.AddDays(-5),
                    ReturnedDate = null, // Quá hạn 10 ngày
                    LateFee = null
                },
                new BorrowRecord
                {
                    BookId = savedBooks[1].Id,
                    BorrowerName = "Phạm Thị Minh (Độc giả VIP)",
                    BorrowDate = now.AddDays(-20),
                    DueDate = now.AddDays(-10),
                    ReturnedDate = now.AddDays(-8),
                    LateFee = 14000m
                },
                new BorrowRecord
                {
                    BookId = savedBooks[2].Id,
                    BorrowerName = "Nguyễn Hoàng Nam (Cán bộ Staff)",
                    BorrowDate = now.AddDays(-7),
                    DueDate = now.AddDays(7),
                    ReturnedDate = null,
                    LateFee = 0m
                },
                new BorrowRecord
                {
                    BookId = savedBooks[3].Id,
                    BorrowerName = "Trần Bảo Ngọc (Thường)",
                    BorrowDate = now.AddDays(-30),
                    DueDate = now.AddDays(-16),
                    ReturnedDate = null, // Quá hạn 16 ngày (Sách hiếm / Ngoại văn)
                    LateFee = null
                }
            };

            savedBooks[0].IsBorrowed = true;
            savedBooks[2].IsBorrowed = true;
            savedBooks[3].IsBorrowed = true;

            await context.BorrowRecords.AddRangeAsync(borrowRecords);
            await context.SaveChangesAsync();
        }

        Console.WriteLine($"[DbInitializer] Đã nạp thành công {realBooks.Count} cuốn sách vào Database!");
    }

    private static List<Book> GetFallbackSampleBooks()
    {
        return new List<Book>
        {
            new Book { Title = "Giáo Trình C# .NET Enterprise Core", Author = "Microsoft Press", ISBN = "978-0134685991", Type = BookType.Textbook, BasePrice = 150000m },
            new Book { Title = "Đại Việt Sử Ký Toàn Thư (Bản Cổ Khắc Gỗ 1697)", Author = "Ngô Sĩ Liên", ISBN = "978-6045501234", Type = BookType.Rare, BasePrice = 500000m },
            new Book { Title = "Clean Architecture: A Craftsman's Guide", Author = "Robert C. Martin", ISBN = "978-0134494166", Type = BookType.Foreign, BasePrice = 320000m },
            new Book { Title = "Tạp Chí Khoa Học Thư Viện & CNTT Số 45", Author = "Hội Thư Viện VN", ISBN = "978-6047719999", Type = BookType.Magazine, BasePrice = 50000m },
            new Book { Title = "Audiobook: Lược Sử Loài Người Sapiens", Author = "Yuval Noah Harari", ISBN = "978-6045618888", Type = BookType.Audio, BasePrice = 180000m },
            new Book { Title = "Lập Trình Hướng Đối Tượng & SOLID Principles", Author = "Nguyễn Văn A", ISBN = "978-6045509999", Type = BookType.Regular, BasePrice = 100000m }
        };
    }

    private static async Task<List<Book>> FetchBooksFromOpenLibraryApiAsync(HttpClient httpClient)
    {
        var books = new List<Book>();
        var searchQueries = new[]
        {
            ("csharp programming", BookType.Textbook),
            ("clean code", BookType.Foreign),
            ("design patterns", BookType.Foreign),
            ("computer science", BookType.Textbook)
        };

        var seenIsbns = new HashSet<string>();

        foreach (var (query, defaultType) in searchQueries)
        {
            try
            {
                var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(query)}&limit=5";
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) continue;

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                if (!doc.RootElement.TryGetProperty("docs", out var docs)) continue;

                foreach (var docItem in docs.EnumerateArray())
                {
                    var title = docItem.TryGetProperty("title", out var t) ? t.GetString() : null;
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var authorsStr = "Nhái tác giả";
                    if (docItem.TryGetProperty("author_name", out var authorsArr) && authorsArr.ValueKind == JsonValueKind.Array)
                    {
                        var authors = authorsArr.EnumerateArray().Select(a => a.GetString()).Where(a => !string.IsNullOrEmpty(a));
                        authorsStr = string.Join(", ", authors);
                    }

                    string isbn = string.Empty;
                    if (docItem.TryGetProperty("isbn", out var isbnArr) && isbnArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var isbnElem in isbnArr.EnumerateArray())
                        {
                            var val = isbnElem.GetString();
                            if (!string.IsNullOrEmpty(val) && (val.Length == 13 || val.Length == 10))
                            {
                                isbn = val;
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(isbn))
                    {
                        isbn = $"978-{Math.Abs(title.GetHashCode() % 900000000 + 100000000)}";
                    }

                    if (seenIsbns.Contains(isbn)) continue;
                    seenIsbns.Add(isbn);

                    var bookType = defaultType;
                    var titleLower = title.ToLower();
                    if (titleLower.Contains("journal") || titleLower.Contains("magazine"))
                    {
                        bookType = BookType.Magazine;
                    }
                    else if (titleLower.Contains("edition") || titleLower.Contains("handbook") || titleLower.Contains("guide"))
                    {
                        bookType = BookType.Textbook;
                    }

                    decimal basePrice = bookType switch
                    {
                        BookType.Rare => 500000m,
                        BookType.Foreign => 280000m,
                        BookType.Textbook => 150000m,
                        BookType.Magazine => 50000m,
                        _ => 100000m
                    };

                    books.Add(new Book
                    {
                        Title = title.Length > 150 ? title.Substring(0, 147) + "..." : title,
                        Author = authorsStr.Length > 100 ? authorsStr.Substring(0, 97) + "..." : authorsStr,
                        ISBN = isbn,
                        Type = bookType,
                        BasePrice = basePrice,
                        IsBorrowed = false
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer] Lỗi khi gửi OpenLibrary API ({query}): {ex.Message}");
            }
        }

        if (books.Count == 0)
        {
            books = GetFallbackSampleBooks();
        }

        return books;
    }
}
