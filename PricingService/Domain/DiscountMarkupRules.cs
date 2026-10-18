namespace PricingService.Domain;

public sealed class DiscountMarkupRules
{
    private readonly List<DiscountMarkupRule> rules = new();

    internal IReadOnlyList<DiscountMarkupRule> Rules => rules;

    // A factor of 1.50 increases the premium by 50%; 0.90 discounts it by 10%.
    public void AddPercentMarkup(string condition, decimal multiplier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(condition);
        if (multiplier <= 0M)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier),
                "The premium multiplier must be greater than zero.");
        }

        rules.Add(new DiscountMarkupRule(condition, multiplier));
    }

    internal decimal CalculateMultiplier(RuleExpressionEvaluator evaluator)
    {
        var multiplier = 1M;
        foreach (var rule in rules)
        {
            if (evaluator.Matches(rule.Condition))
            {
                multiplier *= rule.Multiplier;
            }
        }

        return multiplier;
    }
}

internal sealed record DiscountMarkupRule(string Condition, decimal Multiplier);
