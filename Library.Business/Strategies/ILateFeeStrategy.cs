using Library.Business.Enums;

namespace Library.Business.Strategies;

public interface ILateFeeStrategy
{
    BookType SupportedType { get; }
    decimal CalculateFee(int daysLate);
}

