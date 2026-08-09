using Library.Business.Entities;
using Library.Business.Enums;

namespace Library.Business.Legacy;

public class LegacyFeeCalculator
{
    public decimal CalculateLateFee(Book book, int daysLate)
    {
        if (daysLate <= 0) return 0;

        return book.Type switch
        {
            BookType.Regular => daysLate * 5000m,
            BookType.Rare => daysLate * 20000m,
            BookType.Textbook => daysLate * 3000m,
            BookType.Magazine => daysLate * 2000m,
            BookType.Foreign => daysLate * 10000m,
            _ => daysLate * 5000m
        };
    }
}
