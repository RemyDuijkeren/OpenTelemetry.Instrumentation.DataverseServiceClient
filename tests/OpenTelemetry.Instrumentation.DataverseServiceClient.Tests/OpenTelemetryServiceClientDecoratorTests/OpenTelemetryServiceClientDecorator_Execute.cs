using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Execute
{
    [Fact]
    public void CallsUnderlyingServiceRetrieveMethod_When_RequestIsProvided()
    {
        // Arrange
        var mockedService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockedService);
        var organizationRequest = new OrganizationRequest();

        // Act
        decorator.Execute(organizationRequest);

        // Assert
        mockedService.Received(1).Execute(organizationRequest);
    }

    [Fact]
    public void CallsUnderlyingServiceRetrieveMethod_When_RequestIsNull()
    {
        // Arrange
        var mockedService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockedService);
        OrganizationRequest organizationRequest = null!;

        // Act
        decorator.Execute(organizationRequest);

        // Assert
        mockedService.Received(1).Execute(organizationRequest);
    }

    [SkippableFact]
    public void ThrowNullReferenceException_When_RequestIsNull_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var serviceClient = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(serviceClient);
        OrganizationRequest organizationRequest = null!;

        // Act
        Action act = () => decorator.Execute(organizationRequest);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}
