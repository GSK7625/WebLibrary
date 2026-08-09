using Library.Business.Interfaces;
using Library.Business.Strategies;
using Library.Business.Enums;

namespace Library.Business.Services;

public class LateFeeApplicationService : ILateFeeApplicationService
{
    private readonly IEnumerable<ILateFeeStrategy> _strategies;

    public LateFeeApplicationService(IEnumerable<ILateFeeStrategy> strategies)
    {
        _strategies = strategies;
    }

    public decimal CalculateFee(BookType type, int daysLate)
    {
        if (daysLate <= 0) return 0;

        var strategy = _strategies.FirstOrDefault(s => s.SupportedType == type);
        if (strategy == null)
            throw new InvalidOperationException($"Chua ho tro loai sach: {type}");

        return strategy.CalculateFee(daysLate);
    }
}
