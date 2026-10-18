namespace PricingService.Domain;

public sealed class Tariff
{
    public Tariff(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
    public BasePremiumRules BasePremiumRules { get; } = new();
    public DiscountMarkupRules DiscountMarkupRules { get; } = new();

    public PriceResult CalculatePrice(Calculation calculation)
    {
        ArgumentNullException.ThrowIfNull(calculation);
        if (!string.Equals(Code, calculation.ProductCode, StringComparison.Ordinal))
        {
            throw new ArgumentException("The calculation product does not match this tariff.",
                nameof(calculation));
        }

        var evaluator = new RuleExpressionEvaluator(calculation.Subject);
        var covers = new Dictionary<string, CoverPrice>(StringComparer.Ordinal);
        if (calculation.SelectedCovers.Count == 0)
        {
            return new PriceResult(covers);
        }

        var multiplier = DiscountMarkupRules.CalculateMultiplier(evaluator);
        foreach (var coverCode in calculation.SelectedCovers.Distinct(StringComparer.Ordinal))
        {
            var basePremium = BasePremiumRules.Calculate(coverCode, evaluator);
            var premium = decimal.Round(basePremium * multiplier, 2, MidpointRounding.AwayFromZero);
            covers.Add(coverCode, new CoverPrice(premium));
        }

        return new PriceResult(covers);
    }
}
