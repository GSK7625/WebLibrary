using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Business.Legacy;

public class LegacyFeeCalculator
{
    public decimal CalculateLateFee(Book book, int daysLate)
    {
        if (daysLate <= 0) return 0;

        switch (book.Type)
        {
            case BookType.Regular: return daysLate * 2000m;
            case BookType.Rare: return daysLate * 10000m;
            case BookType.Textbook: return daysLate * 3000m;
            case BookType.Magazine: return daysLate * 1000m;
            default:
                throw new NotSupportedException($"Lo?i sách '{book.Type}' chua du?c h? tr? trong switch-case (Vi ph?m OCP!)");
        }
    }
}

