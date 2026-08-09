using Library.Business.Enums;

namespace Library.Business.Strategies;

public class ForeignBookFeeStrategy : ILateFeeStrategy
{
    public BookType SupportedType => BookType.Foreign;

    public decimal CalculateFee(int daysLate)
    {
        if (daysLate <= 0) return 0;
        return daysLate * 15000m;
    }
}

