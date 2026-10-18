var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());
    var app = builder.Build();
    // Environment setup lab: business endpoints will be implemented in the next lab.
    app.MapGet("/health", () => Results.Ok(new { service = "OfferService", status = "ok", stage = "environment-setup" }));
    app.Run();
    public partial class Program { }
