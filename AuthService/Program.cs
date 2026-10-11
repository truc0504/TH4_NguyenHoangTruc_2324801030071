using AuthService;

var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddAuthService();
    var app = builder.Build();
    // Environment setup lab: business endpoints will be implemented in the next lab.
    app.MapGet("/health", () => Results.Ok(new { service = "AuthService", status = "ok", stage = "environment-setup" }));
    app.Run();
    public partial class Program { }
