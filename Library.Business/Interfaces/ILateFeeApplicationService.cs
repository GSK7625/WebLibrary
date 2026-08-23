using Library.DataAccess.Entities;
using Library.DataAccess.Enums;
using Library.Business.Models;

namespace Library.Business.Interfaces;

public interface ILateFeeApplicationService
{
    FeeCalculationResult CalculateFee(Book book, int daysLate, MemberType memberType = MemberType.Standard);
}
