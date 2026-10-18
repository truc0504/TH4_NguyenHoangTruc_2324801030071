using DynamicExpresso.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using PricingService.Api.Commands;
using PricingService.Domain;

namespace PricingService.Commands;

public sealed class CalculatePriceHandler : IRequestHandler<CalculatePriceCommand, CalculatePriceResult>
{
    private readonly IDataStore dataStore;
    private readonly CalculatePriceCommandValidator validator = new();

    public CalculatePriceHandler(IDataStore dataStore)
    {
        this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    public async Task<CalculatePriceResult> Handle(
        CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var subject = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var answer in request.Answers)
        {
            if (answer is null || string.IsNullOrWhiteSpace(answer.QuestionCode) ||
                answer.GetAnswer() is not { } value)
            {
                throw InvalidField(nameof(request.Answers),
                    "Each answer must contain a question code and a value.");
            }

            if (!subject.TryAdd(answer.QuestionCode, value))
            {
                throw InvalidField(nameof(request.Answers),
                    $"Question '{answer.QuestionCode}' was answered more than once.");
            }
        }

        if (request.SelectedCovers.Any(string.IsNullOrWhiteSpace))
        {
            throw InvalidField(nameof(request.SelectedCovers), "Cover codes must not be empty.");
        }

        Tariff tariff;
        try
        {
            tariff = await dataStore.Tariffs[request.ProductCode].WaitAsync(cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            throw InvalidField(nameof(request.ProductCode),
                $"No tariff exists for product '{request.ProductCode}'.");
        }

        var calculation = new Calculation(request.ProductCode, request.PolicyFrom, request.PolicyTo,
            request.SelectedCovers, subject);

        PriceResult price;
        try
        {
            price = tariff.CalculatePrice(calculation);
        }
        catch (UnknownIdentifierException exception)
        {
            throw InvalidField(nameof(request.Answers),
                $"The supplied answers do not provide a variable required by the tariff: {exception.Message}");
        }

        return new CalculatePriceResult
        {
            TotalPrice = price.TotalPremium,
            CoverPrices = price.Covers.ToDictionary(cover => cover.Key, cover => cover.Value.Price,
                StringComparer.Ordinal)
        };
    }

    private static ValidationException InvalidField(string propertyName, string message)
    {
        return new ValidationException(new[] { new ValidationFailure(propertyName, message) });
    }
}
