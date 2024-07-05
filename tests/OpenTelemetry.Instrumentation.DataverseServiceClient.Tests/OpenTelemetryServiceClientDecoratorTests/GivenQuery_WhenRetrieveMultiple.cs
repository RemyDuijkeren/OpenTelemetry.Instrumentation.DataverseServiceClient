using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenQuery_WhenRetrieveMultiple
{
    [Fact]
    public void RetrieveMultipleOnDecoratedService_When_QueryIsNull()
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
    public void RetrieveMultipleOnDecoratedService_When_QueryExpressionProvided()
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
    public void RetrieveMultipleOnDecoratedService_When_QueryByAttributeProvided()
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
    public void RetrieveMultipleOnDecoratedService_When_FetchExpressionProvided()
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
    public void RetrieveMultipleOnDecoratedService_When_UnknownQueryTypeProvided()
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
        Skip.IfNot(TestContext.CanCreateXrmRealContext);

        // Arrange
        using TestContext testContext = new();
        var service = testContext.XrmRealContext.GetAsyncOrganizationService2();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        QueryExpression query = null!;

        // Act
        Action act = () => decorator.RetrieveMultiple(query);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }
}
