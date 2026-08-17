using Library.Business.Entities;
using Library.Business.Enums;
using Library.Business.Models;

namespace Library.Business.Interfaces;

public interface ILateFeeApplicationService
{
    FeeCalculationResult CalculateFee(FeeCalculationContext context);
    FeeCalculationResult CalculateFee(Book book, int daysLate, MemberType memberType = MemberType.Standard);
}
