using Library.Business.Enums;
namespace Library.Business.Strategies;

public class RegularBookFeeStrategy : ILateFeeStrategy
{
    public BookType SupportedType => BookType.Regular;
    public decimal CalculateFee(int daysLate) => daysLate > 0 ? daysLate * 2000m : 0;
}

