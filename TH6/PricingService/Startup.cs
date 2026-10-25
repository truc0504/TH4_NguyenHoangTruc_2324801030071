using Steeltoe.Discovery.Client;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Npgsql;
using PricingService.Commands;
using PricingService.Data;
using PricingService.Domain;

namespace PricingService;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDiscoveryClient(Configuration);

        services.AddControllers(options =>
            {
                // Validation is defined by CalculatePriceCommandValidator, including null lists.
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddNewtonsoftJson(options =>
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

        services.AddMediatR(options =>
            options.RegisterServicesFromAssemblyContaining<CalculatePriceHandler>());

        // Uses the existing lab database by default. Configuration (including Testcontainers)
        // can override this through ConnectionStrings:DefaultConnection.
        var connectionString = Configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=lab_netmicro_pricing;Username=lab_user;Password=lab_pass";
        services.AddSingleton<NpgsqlDataSource>(_ => NpgsqlDataSource.Create(connectionString));
        services.AddSingleton<ITariffRepository, TariffRepository>();
        services.AddScoped<IDataStore, DataStore>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (ValidationException exception) when (!context.Response.HasStarted)
            {
                var errors = exception.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray());

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "The pricing request is invalid."
                }, cancellationToken: context.RequestAborted);
            }
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/health", () => Results.Ok(new
            {
                service = "PricingService",
                status = "ok",
                stage = "environment-setup"
            }));
        });
    }
}
