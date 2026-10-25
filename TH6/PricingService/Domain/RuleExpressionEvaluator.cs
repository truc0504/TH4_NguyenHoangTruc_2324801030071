using DynamicExpresso;

namespace PricingService.Domain;

internal sealed class RuleExpressionEvaluator
{
    private readonly Interpreter interpreter = new();

    public RuleExpressionEvaluator(IReadOnlyDictionary<string, object> subject)
    {
        foreach (var answer in subject)
        {
            interpreter.SetVariable(answer.Key, answer.Value);
        }
    }

    public bool Matches(string? condition)
    {
        return string.IsNullOrWhiteSpace(condition) || interpreter.Eval<bool>(condition);
    }

    public decimal EvaluatePremium(string expression)
    {
        return interpreter.Eval<decimal>(expression);
    }
}
