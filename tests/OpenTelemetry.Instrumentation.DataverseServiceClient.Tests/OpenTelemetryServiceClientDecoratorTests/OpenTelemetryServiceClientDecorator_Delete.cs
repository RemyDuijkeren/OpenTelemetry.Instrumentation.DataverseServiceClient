using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Delete
{
    [Theory]
    [InlineData(null)] // Test entity is null
    [InlineData("")] // Test entity is empty
    [InlineData("Test")]
    public void CallsUnderlyingService(string? entityName)
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var id = Guid.NewGuid();

        // Act
        decorator.Delete(entityName!, id);

        // Assert
        mockService.Received(1).Delete(entityName, id);
    }

    [SkippableTheory]
    [InlineData(null)] // Test entity is null
    [InlineData("")] // Test entity is empty
    public void ThrowUnderlyingFaultException_When_EntityNameIsNull_WithoutMocking(string? entityName)
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var service = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        var id = Guid.NewGuid();

        // Act
        Action act = () => decorator.Delete(entityName!, id);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }
}
