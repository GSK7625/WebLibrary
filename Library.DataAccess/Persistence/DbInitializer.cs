using Library.DataAccess.Entities;
using Library.DataAccess.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.DataAccess.Persistence;

public static class DbInitializer
{
    public static async Task SeedDataAsync(LibraryDbContext context, bool forceRecreate = false)
    {
        await context.Database.EnsureCreatedAsync();

        if (forceRecreate)
        {
            context.BorrowRecords.RemoveRange(context.BorrowRecords);
            context.Books.RemoveRange(context.Books);
            await context.SaveChangesAsync();
        }
        else if (await context.Books.AnyAsync())
        {
            return;
        }

        var sampleBooks = new List<Book>
        {
            new() { Title = "Giáo Trình C# .NET Enterprise Core", Author = "Microsoft Press", ISBN = "978-0134685991", Type = BookType.Textbook, BasePrice = 150000m, IsBorrowed = true },
            new() { Title = "Đại Việt Sử Ký Toàn Thư (Bản Cổ Khắc Gỗ 1697)", Author = "Ngô Sĩ Liên", ISBN = "978-6045501234", Type = BookType.Rare, BasePrice = 500000m, IsBorrowed = false },
            new() { Title = "Clean Architecture: A Craftsman's Guide", Author = "Robert C. Martin", ISBN = "978-0134494166", Type = BookType.Foreign, BasePrice = 320000m, IsBorrowed = true },
            new() { Title = "Tạp Chí Khoa Học Thư Viện & CNTT Số 45", Author = "Hội Thư Viện VN", ISBN = "978-6047719999", Type = BookType.Magazine, BasePrice = 50000m, IsBorrowed = true },
            new() { Title = "Audiobook: Lược Sử Loài Người Sapiens", Author = "Yuval Noah Harari", ISBN = "978-6045618888", Type = BookType.Audio, BasePrice = 180000m, IsBorrowed = false },
            new() { Title = "Lập Trình Hướng Đối Tượng & SOLID Principles", Author = "Nguyễn Văn A", ISBN = "978-6045509999", Type = BookType.Regular, BasePrice = 100000m, IsBorrowed = false },
            new() { Title = "Design Patterns: Elements of Reusable Object-Oriented Software", Author = "Erich Gamma et al.", ISBN = "978-0201633610", Type = BookType.Foreign, BasePrice = 350000m, IsBorrowed = false },
            new() { Title = "Cấu Trúc Dữ Liệu và Giải Thuật", Author = "Đỗ Xuân Lôi", ISBN = "978-6040102030", Type = BookType.Textbook, BasePrice = 120000m, IsBorrowed = false },
            new() { Title = "Bản Thảo Truyện Kiều Bản Cổ Độc Bản", Author = "Nguyễn Du", ISBN = "978-6045509876", Type = BookType.Rare, BasePrice = 800000m, IsBorrowed = false },
            new() { Title = "Tạp Chí Tin Học & Điều Khiển Học", Author = "Viện Hàn Lâm KH&CN", ISBN = "978-6047718888", Type = BookType.Magazine, BasePrice = 450000m, IsBorrowed = false }
        };

        await context.Books.AddRangeAsync(sampleBooks);
        await context.SaveChangesAsync();

        var savedBooks = await context.Books.ToListAsync();
        var now = DateTime.Now;

        var borrowRecords = new List<BorrowRecord>
        {
            new()
            {
                BookId = savedBooks[0].Id,
                BorrowerName = "Lê Văn Hùng (Sinh viên)",
                BorrowDate = now.AddDays(-15),
                DueDate = now.AddDays(-5),
                ReturnedDate = null, // Quá hạn 5 ngày
                LateFee = null
            },
            new()
            {
                BookId = savedBooks[2].Id,
                BorrowerName = "Phạm Thị Minh (Độc giả VIP)",
                BorrowDate = now.AddDays(-20),
                DueDate = now.AddDays(-10),
                ReturnedDate = now.AddDays(-8),
                LateFee = 14000m
            },
            new()
            {
                BookId = savedBooks[3].Id,
                BorrowerName = "Trần Bảo Ngọc (Thường)",
                BorrowDate = now.AddDays(-30),
                DueDate = now.AddDays(-16),
                ReturnedDate = null, // Quá hạn 16 ngày
                LateFee = null
            }
        };

        await context.BorrowRecords.AddRangeAsync(borrowRecords);
        await context.SaveChangesAsync();
    }
}
