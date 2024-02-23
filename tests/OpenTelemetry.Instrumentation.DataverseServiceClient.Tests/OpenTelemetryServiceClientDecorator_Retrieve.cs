using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Retrieve
{
    [Theory]
    [InlineData(null)] // Test entity is null
    [InlineData("")] // Test entity is empty
    [InlineData("Test")]
    public void CallsUnderlyingServiceRetrieveMethod(string entityName)
    {
        // Arrange
        var mockedService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockedService);
        var id = Guid.NewGuid();
        var columnSet = new ColumnSet();

        // Act
        decorator.Retrieve(entityName, id, columnSet);

        // Assert
        mockedService.Received(1).Retrieve(entityName, id, columnSet);
    }

    [SkippableFact]
    public void ThrowFaultException_When_EntityNameIsEmpty_WithoutMocking1()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var serviceClient = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(serviceClient);
        var id = Guid.NewGuid();
        var columnSet = new ColumnSet();
        string entityName = "";

        // Act
        Action act = () => decorator.Retrieve(entityName, id, columnSet);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }

    [SkippableFact]
    public void ThrowNullReferenceException_When_EntityNameIsNull_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var serviceClient = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(serviceClient);
        var id = Guid.NewGuid();
        var columnSet = new ColumnSet();
        string entityName = null!;

        // Act
        Action act = () => decorator.Retrieve(entityName, id, columnSet);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}
