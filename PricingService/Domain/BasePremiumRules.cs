using FluentValidation;
using FluentValidation.Results;

namespace PricingService.Domain;

public sealed class BasePremiumRules
{
    private readonly List<BasePremiumRule> rules = new();

    internal IReadOnlyList<BasePremiumRule> Rules => rules;

    public void AddBasePriceRule(string coverCode, string? condition, string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coverCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        rules.Add(new BasePremiumRule(coverCode, condition, expression));
    }

    internal decimal Calculate(string coverCode, RuleExpressionEvaluator evaluator)
    {
        var matchingRules = rules
            .Where(rule => rule.CoverCode == coverCode && evaluator.Matches(rule.Condition))
            .ToArray();

        if (matchingRules.Length == 0)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("SelectedCovers",
                    $"No applicable pricing rule was found for cover '{coverCode}'.")
            });
        }

        if (matchingRules.Length > 1)
        {
            throw new InvalidOperationException(
                $"More than one base pricing rule applies to cover '{coverCode}'.");
        }

        return evaluator.EvaluatePremium(matchingRules[0].Expression);
    }
}

internal sealed record BasePremiumRule(string CoverCode, string? Condition, string Expression);
