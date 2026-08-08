using Library.Domain.Enums;

namespace Library.Business.Strategies;

public class MagazineFeeStrategy : ILateFeeStrategy
{
    public BookType SupportedType => BookType.Magazine;

    public decimal CalculateFee(int daysLate)
    {
        if (daysLate <= 0) return 0;
        return daysLate * 1000m;
    }
}

