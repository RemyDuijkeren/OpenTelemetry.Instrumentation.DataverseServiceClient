using Microsoft.Xrm.Sdk;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenOrganizationRequest_WhenExecute
{
    [Fact]
    public void ExecuteOnDecoratedService_When_RequestIsProvided()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var organizationRequest = new OrganizationRequest();

        // Act
        decorator.Execute(organizationRequest);

        // Assert
        mockService.Received(1).Execute(organizationRequest);
    }

    [Fact]
    public void ExecuteOnDecoratedService_When_RequestIsNull()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        OrganizationRequest organizationRequest = null!;

        // Act
        decorator.Execute(organizationRequest);

        // Assert
        mockService.Received(1).Execute(organizationRequest);
    }

    [SkippableFact]
    public void ThrowUnderlyingNullReferenceException_When_RequestIsNull_WithoutMocking()
    {
        Skip.IfNot(TestContext.CanCreateXrmRealContext);

        // Arrange
        using TestContext testContext = new();
        var service = testContext.XrmRealContext.GetOrganizationService();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        OrganizationRequest organizationRequest = null!;

        // Act
        Action act = () => decorator.Execute(organizationRequest);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void ThrowUnderlyingNullReferenceException_When_RequestIsNull_WithXrmFakedContext()
    {
        // Arrange
        using TestContext testContext = new();
        var mockService = testContext.XrmFakedContext.GetOrganizationService();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        OrganizationRequest organizationRequest = null!;

        // Act
        Action act = () => decorator.Execute(organizationRequest);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}
