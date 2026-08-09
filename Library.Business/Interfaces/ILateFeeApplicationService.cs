using Library.Business.Enums;

namespace Library.Business.Interfaces;

public interface ILateFeeApplicationService
{
    decimal CalculateFee(BookType type, int daysLate);
}
