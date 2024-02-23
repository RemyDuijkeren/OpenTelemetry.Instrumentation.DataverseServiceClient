using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Operator
{
    [SkippableFact]
    public void ReturnServiceClientFromDecorator_When_Cast()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var serviceClient = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(serviceClient);

        // Act
        var convertedServiceClient = (ServiceClient)decorator;

        // Assert
        convertedServiceClient.Should().BeEquivalentTo(serviceClient);
    }

    [Fact]
    public void ThrowInvalidOperationException_When_CastButServiceIsNotServiceClient()
    {
        // Arrange
        var incompatibleService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(incompatibleService);

        // Act
        Action act = () => { var serviceClient = (ServiceClient)decorator; };

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Decorated client is not a ServiceClient.");
    }
}
