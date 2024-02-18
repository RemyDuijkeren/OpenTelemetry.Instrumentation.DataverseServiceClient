using Microsoft.Xrm.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class AddDataverseServiceClientInstrumentationTests
{
    [Fact]
    public void ReturnsDecorator_GivenOrganizationServiceAsync2Registration()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationServiceAsync2>();
        services.AddSingleton<IOrganizationServiceAsync2>(_ => serviceClientStub);

        // Act
        services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddDataverseServiceClientInstrumentation());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var orgService = serviceProvider.GetRequiredService<IOrganizationServiceAsync2>();

        orgService.Should().NotBe(serviceClientStub);
        orgService.Should().BeOfType<OpenTelemetryServiceClientWrapper>();
    }

    [Fact]
    public void ReturnsDecorator_GivenOrganizationServiceAsyncRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationServiceAsync>();
        services.AddSingleton<IOrganizationServiceAsync>(_ => serviceClientStub);

        // Act
        services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddDataverseServiceClientInstrumentation());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var orgService = serviceProvider.GetRequiredService<IOrganizationServiceAsync>();

        orgService.Should().NotBe(serviceClientStub);
        orgService.Should().BeOfType<OpenTelemetryServiceClientWrapper>();
    }

    [Fact]
    public void ReturnsDecorator_GivenOrganizationServiceRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationService>();
        services.AddSingleton<IOrganizationService>(_ => serviceClientStub);

        // Act
        services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddDataverseServiceClientInstrumentation());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var orgService = serviceProvider.GetRequiredService<IOrganizationService>();

        orgService.Should().NotBe(serviceClientStub);
        orgService.Should().BeOfType<OpenTelemetryServiceClientWrapper>();
    }
}
