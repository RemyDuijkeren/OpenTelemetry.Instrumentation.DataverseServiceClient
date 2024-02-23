using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Delete
{
    [Theory]
    [InlineData(null)] // Test entity is null
    [InlineData("")] // Test entity is empty
    [InlineData("Test")]
    public void CallsUnderlyingServiceCreateMethod(string? entityName)
    {
        // Arrange
        var mockedService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockedService);
        var id = Guid.NewGuid();

        // Act
        decorator.Delete(entityName!, id);

        // Assert
        mockedService.Received(1).Delete(entityName, id);
    }

    [SkippableTheory]
    [InlineData(null)] // Test entity is null
    [InlineData("")] // Test entity is empty
    public void ThrowFaultException_When_EntityNameIsNull_WithoutMocking(string? entityName)
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var serviceClient = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(serviceClient);
        var id = Guid.NewGuid();

        // Act
        Action act = () => decorator.Delete(entityName!, id);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }
}
