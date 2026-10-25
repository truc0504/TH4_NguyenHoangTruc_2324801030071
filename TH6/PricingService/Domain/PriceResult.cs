using System.Collections.ObjectModel;

namespace PricingService.Domain;

public sealed class PriceResult
{
    public PriceResult(IDictionary<string, CoverPrice> covers)
    {
        ArgumentNullException.ThrowIfNull(covers);
        Covers = new ReadOnlyDictionary<string, CoverPrice>(
            new Dictionary<string, CoverPrice>(covers, StringComparer.Ordinal));
        TotalPremium = Covers.Values.Sum(cover => cover.Price);
    }

    public decimal TotalPremium { get; }
    public IReadOnlyDictionary<string, CoverPrice> Covers { get; }
}

public sealed record CoverPrice(decimal Price);
