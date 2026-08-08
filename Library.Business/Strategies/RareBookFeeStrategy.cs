using Library.Domain.Enums;
namespace Library.Business.Strategies;

public class RareBookFeeStrategy : ILateFeeStrategy
{
    public BookType SupportedType => BookType.Rare;

    public decimal CalculateFee(int daysLate)
    {
        if (daysLate <= 0) return 0;
        return daysLate * 10000m;
    }
}

