using System.Text.Json;
using Library.Business.Entities;
using Library.Business.Enums;
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
            Console.WriteLine("[DbInitializer] Da xoa toan bo du lieu cu thanh cong!");
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

        // Seed lich su muon tra thuc te voi cac ma sach that vua nap
        var savedBooks = await context.Books.ToListAsync();
        if (savedBooks.Count >= 5)
        {
            var now = DateTime.Now;
            var borrowRecords = new List<BorrowRecord>
            {
                new BorrowRecord
                {
                    BookId = savedBooks[0].Id, // Sach 1
                    BorrowerName = "Le Van Hung",
                    BorrowDate = now.AddDays(-15),
                    DueDate = now.AddDays(-5),
                    ReturnedDate = null, // Qua han 5 ngay
                    LateFee = null
                },
                new BorrowRecord
                {
                    BookId = savedBooks[1].Id, // Sach 2 (Foreign / Rare)
                    BorrowerName = "Pham Thi Minh",
                    BorrowDate = now.AddDays(-20),
                    DueDate = now.AddDays(-10),
                    ReturnedDate = now.AddDays(-8),
                    LateFee = 10000m // Da tra tre 2 ngay
                },
                new BorrowRecord
                {
                    BookId = savedBooks[2].Id, // Sach 3 (Textbook)
                    BorrowerName = "Nguyen Hoang Nam",
                    BorrowDate = now.AddDays(-7),
                    DueDate = now.AddDays(7),
                    ReturnedDate = null, // Con han
                    LateFee = 0m
                },
                new BorrowRecord
                {
                    BookId = savedBooks[3].Id, // Sach 4
                    BorrowerName = "Tran Bao Ngoc",
                    BorrowDate = now.AddDays(-30),
                    DueDate = now.AddDays(-16),
                    ReturnedDate = null, // Qua han 16 ngay
                    LateFee = null
                }
            };

            // Cap nhat dong bo trang thai IsBorrowed = true cho cac sach dang duoc muon (chua tra)
            savedBooks[0].IsBorrowed = true;
            savedBooks[2].IsBorrowed = true;
            savedBooks[3].IsBorrowed = true;

            await context.BorrowRecords.AddRangeAsync(borrowRecords);
            await context.SaveChangesAsync();
        }

        Console.WriteLine($"[DbInitializer] Da nap thanh cong {realBooks.Count} cuon sach THAT vao Database!");
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

                    var authorsStr = "Nhieu tac gia";
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
                    if (titleLower.Contains("journal") || titleLower.Contains("magazine") || titleLower.Contains("tap chi"))
                    {
                        bookType = BookType.Magazine;
                    }
                    else if (titleLower.Contains("edition") || titleLower.Contains("handbook") || titleLower.Contains("guide") || titleLower.Contains("giao trinh") || titleLower.Contains("programming"))
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
                Console.WriteLine($"[DbInitializer] Loi khi gui OpenLibrary API ({query}): {ex.Message}");
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
