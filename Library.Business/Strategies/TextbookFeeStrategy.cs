using Library.Business.Enums;

namespace Library.Business.Strategies;

public class TextbookFeeStrategy : ILateFeeStrategy
{
    public BookType SupportedType => BookType.Textbook;

    public decimal CalculateFee(int daysLate)
    {
        if (daysLate <= 0) return 0;
        return daysLate * 3000m;
    }
}

