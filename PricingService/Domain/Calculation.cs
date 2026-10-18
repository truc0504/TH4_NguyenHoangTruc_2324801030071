using System.Collections.ObjectModel;

namespace PricingService.Domain;

public sealed class Calculation
{
    public Calculation(
        string productCode,
        DateTimeOffset policyFrom,
        DateTimeOffset policyTo,
        IEnumerable<string> selectedCovers,
        IDictionary<string, object> subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productCode);
        ArgumentNullException.ThrowIfNull(selectedCovers);
        ArgumentNullException.ThrowIfNull(subject);

        ProductCode = productCode;
        PolicyFrom = policyFrom;
        PolicyTo = policyTo;
        SelectedCovers = Array.AsReadOnly(selectedCovers.ToArray());
        Subject = new ReadOnlyDictionary<string, object>(
            new Dictionary<string, object>(subject, StringComparer.Ordinal));
    }

    public string ProductCode { get; }
    public DateTimeOffset PolicyFrom { get; }
    public DateTimeOffset PolicyTo { get; }
    public IReadOnlyList<string> SelectedCovers { get; }
    public IReadOnlyDictionary<string, object> Subject { get; }
}
