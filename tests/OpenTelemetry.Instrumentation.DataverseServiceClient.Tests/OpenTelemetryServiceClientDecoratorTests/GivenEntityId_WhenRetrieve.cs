using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenEntityId_WhenRetrieve
{
    [Theory]
    [InlineData(null)] // Test entity is null
    [InlineData("")] // Test entity is empty
    [InlineData("Test")]
    public void RetrieveOnDecoratedService(string? entityName)
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var id = Guid.NewGuid();
        var columnSet = new ColumnSet();

        // Act
        decorator.Retrieve(entityName!, id, columnSet);

        // Assert
        mockService.Received(1).Retrieve(entityName, id, columnSet);
    }

    [SkippableFact]
    public void ThrowUnderlyingFaultException_When_EntityNameIsEmpty_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var service = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        var id = Guid.NewGuid();
        var columnSet = new ColumnSet();
        string entityName = "";

        // Act
        Action act = () => decorator.Retrieve(entityName, id, columnSet);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }

    [SkippableFact]
    public void ThrowUnderlyingNullReferenceException_When_EntityNameIsNull_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var service = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        var id = Guid.NewGuid();
        var columnSet = new ColumnSet();
        string entityName = null!;

        // Act
        Action act = () => decorator.Retrieve(entityName, id, columnSet);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}
