using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Execute
{
    [Fact]
    public void CallsUnderlyingService_When_RequestIsProvided()
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
    public void CallsUnderlyingService_When_RequestIsNull()
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
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var service = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        OrganizationRequest organizationRequest = null!;

        // Act
        Action act = () => decorator.Execute(organizationRequest);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}
