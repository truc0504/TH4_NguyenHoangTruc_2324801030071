namespace PricingService.Domain;

public interface IDataStore
{
    ITariffRepository Tariffs { get; }
}
