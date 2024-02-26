using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_RetrieveMultiple
{
    [Fact]
    public void CallsUnderlyingService_When_QueryIsNull()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        QueryExpression query = null!;

        // Act
        decorator.RetrieveMultiple(query);

        // Assert
        mockService.Received(1).RetrieveMultiple(query);
    }

    [Fact]
    public void CallsUnderlyingService_When_QueryExpressionProvided()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var query = new QueryExpression();

        // Act
        decorator.RetrieveMultiple(query);

        // Assert
        mockService.Received(1).RetrieveMultiple(query);
    }

    [Fact]
    public void CallsUnderlyingService_When_QueryByAttributeProvided()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var query = new QueryByAttribute();

        // Act
        decorator.RetrieveMultiple(query);

        // Assert
        mockService.Received(1).RetrieveMultiple(query);
    }

    [Fact]
    public void CallsUnderlyingService_When_FetchExpressionProvided()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var query = new FetchExpression();

        // Act
        decorator.RetrieveMultiple(query);

        // Assert
        mockService.Received(1).RetrieveMultiple(query);
    }

    [Fact]
    public void CallsUnderlyingService_When_UnknownQueryTypeProvided()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var query = AutoFaker.Generate<QueryBase>();

        // Act
        decorator.RetrieveMultiple(query);

        // Assert
        mockService.Received(1).RetrieveMultiple(query);
    }

    [SkippableFact]
    public void ThrowUnderlyingFaultException_When_QueryIsNull_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var service = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        QueryExpression query = null!;

        // Act
        Action act = () => decorator.RetrieveMultiple(query);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }
}
