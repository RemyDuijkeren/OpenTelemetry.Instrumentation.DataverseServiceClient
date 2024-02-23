using Microsoft.Xrm.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using OpenTelemetry.Trace;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class AddDataverseServiceClientInstrumentation
{
    [Fact]
    public void AddDataverseActivitySourceToTracerProviderBuilder()
    {
        // Arrange
        var tracerBuilder = Substitute.For<TracerProviderBuilder>();

        // Act
        tracerBuilder.AddDataverseServiceClientInstrumentation();

        // Assert
        tracerBuilder.Received(1).AddSource(Arg.Is<string>(x =>  x == DataverseServiceClient.ServiceClientExtensions.DataverseTracer.Name));
    }

    [Fact]
    public void AddDataverseTracerToTracerProvider()
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
        var dataverseTracer = tracerProvider.GetTracer(DataverseServiceClient.ServiceClientExtensions.DataverseTracer.Name);

        dataverseTracer.Should().NotBeNull();
    }

    [Fact]
    public void ReturnDecorator_When_OrganizationServiceAsync2Registration()
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
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReturnDecorator_When_OrganizationServiceAsyncRegistration()
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
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReturnDecorator_When_OrganizationServiceRegistration()
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
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReturnDecorator_When_ServiceRegisteredAsInstance()
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
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReturnDecorator_When_ServiceRegisteredUsingFactoryMethod()
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
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReturnDecorator_When_ServiceRegisteredAsType()
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
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }
}
