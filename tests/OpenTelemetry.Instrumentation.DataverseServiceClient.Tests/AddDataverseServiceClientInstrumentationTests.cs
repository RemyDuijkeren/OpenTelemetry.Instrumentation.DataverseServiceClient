using Microsoft.Xrm.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using OpenTelemetry.Trace;

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

    [Fact]
    public void AddAsSourceOnTracerProviderBuilder_GivenDataverseTActivitySource()
    {
        // Arrange
        var tracerBuilder = Substitute.For<TracerProviderBuilder>();

        // Act
        tracerBuilder.AddDataverseServiceClientInstrumentation();

        // Assert
        tracerBuilder.Received(1).AddSource(Arg.Is<string>(x =>  x == ServiceClientExtensions.DataverseTracer.Name));
    }

    [Fact]
    public void IsTracerInTracerProvider_GivenDataverseActivitySource()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddDataverseServiceClientInstrumentation());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetRequiredService<TracerProvider>();
        var dataverseTracer = tracerProvider.GetTracer(ServiceClientExtensions.DataverseTracer.Name);

        dataverseTracer.Should().NotBeNull();
    }

    [Fact]
    public void ReturnsDecorator_GivenServiceRegisteredAsInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationServiceAsync2>();
        services.AddSingleton(serviceClientStub);

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
    public void ReturnsDecorator_GivenServiceRegisteredUsingFactoryMethod()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationServiceAsync2>();
        services.AddSingleton(_ => serviceClientStub);

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
    public void ReturnsDecorator_GivenServiceRegisteredAsType()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationServiceAsync2>();
        services.AddSingleton<IOrganizationService,NullServiceClient>();

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
