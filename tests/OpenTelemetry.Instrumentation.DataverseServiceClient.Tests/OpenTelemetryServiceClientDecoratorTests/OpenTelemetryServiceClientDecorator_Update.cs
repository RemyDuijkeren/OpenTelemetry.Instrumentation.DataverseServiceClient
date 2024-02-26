using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Update
{
    [Fact]
    public void CallsUnderlyingService_When_EntityIsProvided()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var entity = new Entity();

        // Act
        decorator.Update(entity);

        // Assert
        mockService.Received(1).Update(entity);
    }

    [Fact]
    public void CallsUnderlyingService_When_EntityIsNull()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        Entity entity = null!;

        // Act
        decorator.Update(entity);

        // Assert
        mockService.Received(1).Update(entity);
    }

    [SkippableFact]
    public void ThrowUnderlyingFaultException_When_EntityIsNull_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var client = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(client);
        Entity entity = null!;

        // Act
        Action act = () => decorator.Update(entity);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }
}
