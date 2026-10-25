using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    namespace DocumentService.Test;
    public class EnvironmentSetupTests
    {
        [Fact]
        public void ServiceRegistersMediatR()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));
            using var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetRequiredService<IMediator>());
        }
    }
