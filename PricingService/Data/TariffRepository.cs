using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using PricingService.Domain;

namespace PricingService.Data;

public sealed class TariffRepository : ITariffRepository, IDisposable
{
    private readonly NpgsqlDataSource dataSource;
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool initialized;

    public TariffRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public Task<Tariff> this[string productCode] => GetAsync(productCode);

    public async Task<Tariff> GetAsync(string productCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productCode);
        await InitializeAsync(cancellationToken);

        await using var command = dataSource.CreateCommand(
            "SELECT definition::text FROM pricing_tariffs WHERE code = @code");
        command.Parameters.AddWithValue("code", productCode);
        var definition = await command.ExecuteScalarAsync(cancellationToken) as string;
        if (definition is null)
        {
            throw new KeyNotFoundException($"No tariff exists for product '{productCode}'.");
        }

        var document = JObject.Parse(definition);
        var tariff = new Tariff(productCode);
        var baseRules = document["BasePremiumRules"]?.ToObject<List<BasePremiumRule>>()
            ?? throw new InvalidDataException($"Tariff '{productCode}' has no base premium rules.");
        var markupRules = document["DiscountMarkupRules"]?.ToObject<List<DiscountMarkupRule>>()
            ?? throw new InvalidDataException($"Tariff '{productCode}' has no adjustment rules.");

        foreach (var rule in baseRules)
        {
            tariff.BasePremiumRules.AddBasePriceRule(rule.CoverCode, rule.Condition, rule.Expression);
        }

        foreach (var rule in markupRules)
        {
            tariff.DiscountMarkupRules.AddPercentMarkup(rule.Condition, rule.Multiplier);
        }

        return tariff;
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (initialized)
            {
                return;
            }

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using (var createTable = new NpgsqlCommand("""
                CREATE TABLE IF NOT EXISTS pricing_tariffs (
                    code text PRIMARY KEY,
                    definition jsonb NOT NULL
                )
                """, connection, transaction))
            {
                await createTable.ExecuteNonQueryAsync(cancellationToken);
            }

            // Seed rule definitions, never precomputed prices. Existing tariffs are preserved.
            foreach (var tariff in CreateInitialTariffs())
            {
                var definition = JsonConvert.SerializeObject(new
                {
                    BasePremiumRules = tariff.BasePremiumRules.Rules,
                    DiscountMarkupRules = tariff.DiscountMarkupRules.Rules
                });

                await using var insert = new NpgsqlCommand("""
                    INSERT INTO pricing_tariffs (code, definition)
                    VALUES (@code, @definition)
                    ON CONFLICT (code) DO NOTHING
                    """, connection, transaction);
                insert.Parameters.AddWithValue("code", tariff.Code);
                insert.Parameters.AddWithValue("definition", NpgsqlDbType.Jsonb, definition);
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            initialized = true;
        }
        finally
        {
            initializationLock.Release();
        }
    }

    private static IEnumerable<Tariff> CreateInitialTariffs()
    {
        // These are the product rules supplied in TariffFactory, independent of the test assembly.
        var travel = new Tariff("TRI");
        travel.BasePremiumRules.AddBasePriceRule("C1", null,
            "(NUM_OF_ADULTS) * (DESTINATION == \"EUR\" ? 26.00M : 34.00M)");
        travel.BasePremiumRules.AddBasePriceRule("C2", null,
            "(NUM_OF_ADULTS + NUM_OF_CHILDREN) * 26.00M");
        travel.BasePremiumRules.AddBasePriceRule("C3", null,
            "(NUM_OF_ADULTS + NUM_OF_CHILDREN) * 10.00M");
        travel.DiscountMarkupRules.AddPercentMarkup("DESTINATION == \"WORLD\"", 1.5M);
        yield return travel;

        var house = new Tariff("HSI");
        house.BasePremiumRules.AddBasePriceRule("C1", "TYP == \"APT\"", "AREA * 1.25M");
        house.BasePremiumRules.AddBasePriceRule("C1", "TYP == \"HOUSE\"", "AREA * 1.50M");
        house.BasePremiumRules.AddBasePriceRule("C2", "TYP == \"APT\"", "AREA * 0.25M");
        house.BasePremiumRules.AddBasePriceRule("C2", "TYP == \"HOUSE\"", "AREA * 0.45M");
        house.BasePremiumRules.AddBasePriceRule("C3", null, "30M");
        house.BasePremiumRules.AddBasePriceRule("C4", null, "50M");
        house.DiscountMarkupRules.AddPercentMarkup("FLOOD == \"YES\"", 1.50M);
        house.DiscountMarkupRules.AddPercentMarkup("NUM_OF_CLAIM > 1", 1.25M);
        yield return house;

        var farm = new Tariff("FAI");
        farm.BasePremiumRules.AddBasePriceRule("C1", null, "10M");
        farm.BasePremiumRules.AddBasePriceRule("C2", null, "20M");
        farm.BasePremiumRules.AddBasePriceRule("C3", null, "30M");
        farm.BasePremiumRules.AddBasePriceRule("C4", null, "40M");
        farm.DiscountMarkupRules.AddPercentMarkup("FLOOD == \"YES\"", 1.50M);
        farm.DiscountMarkupRules.AddPercentMarkup("NUM_OF_CLAIM > 2", 2.00M);
        yield return farm;

        var car = new Tariff("CAR");
        car.BasePremiumRules.AddBasePriceRule("C1", null, "100M");
        car.DiscountMarkupRules.AddPercentMarkup("NUM_OF_CLAIM > 2", 1.50M);
        yield return car;
    }

    public void Dispose()
    {
        initializationLock.Dispose();
    }
}
