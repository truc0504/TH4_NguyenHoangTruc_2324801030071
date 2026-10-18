using PricingService.Domain;

namespace PricingService.Data;

public sealed class DataStore : IDataStore
{
    public DataStore(ITariffRepository tariffs)
    {
        Tariffs = tariffs ?? throw new ArgumentNullException(nameof(tariffs));
    }

    public ITariffRepository Tariffs { get; }
}
