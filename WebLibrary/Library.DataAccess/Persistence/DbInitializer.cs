using System.Text.Json;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.DataAccess.Persistence;

public static class DbInitializer
{
    public static async Task SeedDataAsync(LibraryDbContext context, HttpClient? httpClient = null, bool forceRecreate = false)
    {
        // Ensure database exists without deleting file on startup to prevent file locks
        await context.Database.EnsureCreatedAsync();

        if (forceRecreate)
        {
            // Clear existing records cleanly without dropping SQLite DB file
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

        await context.Books.AddRangeAsync(realBooks);
        await context.SaveChangesAsync();

        // Seed lịch sử mượn trả thực tế với các mã sách thật vừa nạp
        var savedBooks = await context.Books.ToListAsync();
        if (savedBooks.Count >= 5)
        {
            var now = DateTime.Now;
            var borrowRecords = new List<BorrowRecord>
            {
                new BorrowRecord
                {
                    BookId = savedBooks[0].Id, // Sách 1
                    BorrowerName = "Lê Văn Hùng",
                    BorrowDate = now.AddDays(-15),
                    DueDate = now.AddDays(-5),
                    ReturnedDate = null, // Quá hạn 5 ngày
                    LateFee = null
                },
                new BorrowRecord
                {
                    BookId = savedBooks[1].Id, // Sách 2 (Foreign / Rare)
                    BorrowerName = "Phạm Thị Minh",
                    BorrowDate = now.AddDays(-20),
                    DueDate = now.AddDays(-10),
                    ReturnedDate = now.AddDays(-8),
                    LateFee = 10000m // Đã trả trễ 2 ngày
                },
                new BorrowRecord
                {
                    BookId = savedBooks[2].Id, // Sách 3 (Textbook)
                    BorrowerName = "Nguyễn Hoàng Nam",
                    BorrowDate = now.AddDays(-7),
                    DueDate = now.AddDays(7),
                    ReturnedDate = null, // Còn hạn
                    LateFee = 0m
                },
                new BorrowRecord
                {
                    BookId = savedBooks[3].Id, // Sách 4
                    BorrowerName = "Trần Bảo Ngọc",
                    BorrowDate = now.AddDays(-30),
                    DueDate = now.AddDays(-16),
                    ReturnedDate = null, // Quá hạn 16 ngày
                    LateFee = null
                }
            };

            // Cập nhật đồng bộ trạng thái IsBorrowed = true cho các sách đang được mượn (chưa trả)
            savedBooks[0].IsBorrowed = true;
            savedBooks[2].IsBorrowed = true;
            savedBooks[3].IsBorrowed = true;

            await context.BorrowRecords.AddRangeAsync(borrowRecords);
            await context.SaveChangesAsync();
        }

        Console.WriteLine($"[DbInitializer] Đã nạp thành công {realBooks.Count} cuốn sách THẬT vào Database!");
    }

    private static async Task<List<Book>> FetchBooksFromOpenLibraryApiAsync(HttpClient httpClient)
    {
        var books = new List<Book>();
        var searchQueries = new[]
        {
            ("csharp programming", BookType.Textbook),
            ("clean code", BookType.Foreign),
            ("design patterns", BookType.Foreign),
            ("computer science", BookType.Textbook),
            ("software engineering", BookType.Textbook)
        };

        var seenIsbns = new HashSet<string>();

        foreach (var (query, defaultType) in searchQueries)
        {
            try
            {
                var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(query)}&limit=10";
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) continue;

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                if (!doc.RootElement.TryGetProperty("docs", out var docs)) continue;

                foreach (var docItem in docs.EnumerateArray())
                {
                    var title = docItem.TryGetProperty("title", out var t) ? t.GetString() : null;
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var authorsStr = "Nhiều tác giả";
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
                    if (titleLower.Contains("journal") || titleLower.Contains("magazine") || titleLower.Contains("tạp chí"))
                    {
                        bookType = BookType.Magazine;
                    }
                    else if (titleLower.Contains("edition") || titleLower.Contains("handbook") || titleLower.Contains("guide") || titleLower.Contains("giáo trình") || titleLower.Contains("programming"))
                    {
                        bookType = BookType.Textbook;
                    }
                    else if (!IsVietnameseText(title) && defaultType == BookType.Regular)
                    {
                        bookType = BookType.Foreign;
                    }

                    books.Add(new Book
                    {
                        Title = title.Length > 150 ? title.Substring(0, 147) + "..." : title,
                        Author = authorsStr.Length > 100 ? authorsStr.Substring(0, 97) + "..." : authorsStr,
                        ISBN = isbn,
                        Type = bookType,
                        IsBorrowed = false
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer] Lỗi khi gửi OpenLibrary API ({query}): {ex.Message}");
            }
        }

        return books;
    }

    private static bool IsVietnameseText(string input)
    {
        string vietnameseChars = "àáảãạâầấẩẫậăằắẳẵặèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ";
        return input.ToLower().Any(c => vietnameseChars.Contains(c));
    }
}
