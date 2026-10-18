namespace PricingService.Domain;

public interface ITariffRepository
{
    Task<Tariff> this[string productCode] { get; }
}
