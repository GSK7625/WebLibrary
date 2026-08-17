namespace Library.Business.Models;

/// <summary>
/// Kết quả tính phí trả trễ chi tiết, hỗ trợ giải trình minh bạch các quy tắc đã được áp dụng.
/// </summary>
public class FeeCalculationResult
{
    public decimal BaseFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalFee { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public List<string> AppliedRules { get; set; } = new();
}
