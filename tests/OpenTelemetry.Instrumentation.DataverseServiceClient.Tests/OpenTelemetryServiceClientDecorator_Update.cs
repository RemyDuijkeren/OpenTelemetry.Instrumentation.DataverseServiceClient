using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Update
{
    [Fact]
    public void CallsUnderlyingServiceUpdateMethod_When_EntityIsProvided()
    {
        // Arrange
        var mockedService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockedService);
        var entity = new Entity();

        // Act
        decorator.Update(entity);

        // Assert
        mockedService.Received(1).Update(entity);
    }

    [Fact]
    public void CallsUnderlyingServiceUpdateMethod_When_EntityIsNull()
    {
        // Arrange
        var mockedService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockedService);
        Entity entity = null!;

        // Act
        decorator.Update(entity);

        // Assert
        mockedService.Received(1).Update(entity);
    }

    [SkippableFact]
    public void ThrowFaultException_When_EntityIsNull_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var serviceClient = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(serviceClient);
        Entity entity = null!;

        // Act
        Action act = () => decorator.Update(entity);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }
}
