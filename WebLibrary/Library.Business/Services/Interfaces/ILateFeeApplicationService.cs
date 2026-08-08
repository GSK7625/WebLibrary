using Library.Domain.Enums;

namespace Library.Business.Services.Interfaces;

public interface ILateFeeApplicationService
{
    decimal CalculateFee(BookType type, int daysLate);
}
