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
    public void ReplaceWithDecorator_When_OrganizationServiceAsync2Registration()
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

        orgService.Should().NotBeNull();
        orgService.Should().NotBe(serviceClientStub);
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReplaceWithDecorator_When_OrganizationServiceAsyncRegistration()
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

        orgService.Should().NotBeNull();
        orgService.Should().NotBe(serviceClientStub);
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReplaceWithDecorator_When_OrganizationServiceRegistration()
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

        orgService.Should().NotBeNull();
        orgService.Should().NotBe(serviceClientStub);
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReplaceWithDecorator_When_ServiceRegisteredAsInstance()
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
    public void ReplaceWithDecorator_When_ServiceRegisteredUsingFactoryMethod()
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

        orgService.Should().NotBeNull();
        orgService.Should().NotBe(serviceClientStub);
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReplaceWithDecorator_When_ServiceRegisteredAsType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IOrganizationService,NullServiceClient>();

        // Act
        services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddDataverseServiceClientInstrumentation());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var orgService = serviceProvider.GetRequiredService<IOrganizationService>();

        orgService.Should().NotBeNull();
        orgService.Should().NotBeOfType<NullServiceClient>();
        orgService.Should().BeOfType<OpenTelemetryServiceClientDecorator>();
    }

    [Fact]
    public void ReplaceWithSingletonDecorator_When_ServiceRegisteredAsSingleton()
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

        orgService.Should().NotBeNull();
        orgService.Should().NotBe(serviceClientStub);
        serviceProvider.GetRequiredService<IOrganizationServiceAsync2>().Should().BeSameAs(orgService);
    }

    [Fact]
    public void ReplaceWithScopedDecorator_When_ServiceRegisteredAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationServiceAsync2>();
        services.AddScoped<IOrganizationServiceAsync2>(_ => serviceClientStub);

        // Act
        services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddDataverseServiceClientInstrumentation());

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        var scope1 = serviceProvider.CreateScope();
        var scope2 = serviceProvider.CreateScope();

        var orgService1 = scope1.ServiceProvider.GetRequiredService<IOrganizationServiceAsync2>();
        var orgService2 = scope2.ServiceProvider.GetRequiredService<IOrganizationServiceAsync2>();

        orgService1.Should().NotBeNull();
        orgService2.Should().NotBeNull();
        orgService1.Should().NotBe(serviceClientStub);
        orgService2.Should().NotBe(serviceClientStub);

        orgService1.Should().NotBeSameAs(orgService2);
    }

    [Fact]
    public void ReplaceWithTransientDecorator_When_ServiceRegisteredAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceClientStub = Substitute.For<IOrganizationServiceAsync2>();
        services.AddTransient<IOrganizationServiceAsync2>(_ => serviceClientStub);

        // Act
        services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddDataverseServiceClientInstrumentation());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var orgService = serviceProvider.GetRequiredService<IOrganizationServiceAsync2>();

        orgService.Should().NotBeNull();
        orgService.Should().NotBe(serviceClientStub);
        serviceProvider.GetRequiredService<IOrganizationServiceAsync2>().Should().NotBeSameAs(orgService);
    }
}
