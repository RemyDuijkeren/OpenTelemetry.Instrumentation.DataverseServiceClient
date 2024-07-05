using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenDecoratedServiceClient_Operator
{
    [SkippableFact]
    public void ReturnServiceClientFromDecorator_When_Cast()
    {
        Skip.IfNot(TestContext.CanCreateXrmRealContext);

        // Arrange
        using TestContext testContext = new();
        var service = testContext.XrmRealContext.GetAsyncOrganizationService2();
        var decorator = new OpenTelemetryServiceClientDecorator(service);

        // Act
        var convertedServiceClient = (ServiceClient)decorator;

        // Assert
        convertedServiceClient.Should().Be(service);
    }

    [SkippableFact]
    public void ReturnServiceClientFromDecorator_When_CallingInternalServiceClient()
    {
        Skip.IfNot(TestContext.CanCreateXrmRealContext);

        // Arrange
        using TestContext testContext = new();
        var service = testContext.XrmRealContext.GetAsyncOrganizationService2();
        var decorator = new OpenTelemetryServiceClientDecorator(service);

        // Act
        var convertedServiceClient = decorator.InternalServiceClient;

        // Assert
        convertedServiceClient.Should().Be(service);
    }

    [Fact]
    public void ThrowInvalidOperationException_When_CastButServiceIsNotServiceClient()
    {
        // Arrange
        var incompatibleService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(incompatibleService);

        // Act
        Action act = () => { var service = (ServiceClient)decorator; };

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Decorated client is not a ServiceClient. Cannot cast to ServiceClient!");
    }
}
