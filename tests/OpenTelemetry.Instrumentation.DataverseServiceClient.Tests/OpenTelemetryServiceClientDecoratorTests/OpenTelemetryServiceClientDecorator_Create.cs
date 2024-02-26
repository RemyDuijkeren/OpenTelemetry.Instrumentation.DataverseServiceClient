using System.Diagnostics;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class OpenTelemetryServiceClientDecorator_Create
{
    [Fact]
    public void CallsUnderlyingService_When_EntityIsProvided()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        var entity = new Entity();

        // Act
        decorator.Create(entity);

        // Assert
        mockService.Received(1).Create(entity);
    }

    [Fact]
    public void CallsUnderlyingService_When_EntityIsNull()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var decorator = new OpenTelemetryServiceClientDecorator(mockService);
        Entity entity = null!;

        // Act
        decorator.Create(entity);

        // Assert
        mockService.Received(1).Create(entity);
    }

    [SkippableFact]
    public void ThrowUnderlyingFaultException_When_EntityIsNull_WithoutMocking()
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var service = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        Entity entity = null!;

        // Act
        Action act = () => decorator.Create(entity);

        // Assert
        act.Should().Throw<FaultException<OrganizationServiceFault>>();
    }

    [Fact]
    public void TraceInsertStatement_When_Entity()
    {
        // Arrange
        var mockService = Substitute.For<IOrganizationService>();
        var randomGuid = Guid.NewGuid();
        mockService.Create(Arg.Any<Entity>()).Returns(randomGuid);

        var decorator = new OpenTelemetryServiceClientDecorator(mockService);

        var exportedItems = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                                      .AddDataverseServiceClientInstrumentation()
                                      .AddInMemoryExporter(exportedItems)
                                      .Build();

        Entity entity = new("TestEntity")
        {
            Attributes =
            {
                ["name"] = "testName",
                ["address1"] = "testAddress1",
            }
        };

        // Act
        decorator.Create(entity);

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be($"Create {entity.LogicalName}");
        activity.Kind.Should().Be(ActivityKind.Client);

        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbOperation).Value.Should().Be("Create");
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbSqlTable).Value.Should().Be(entity.LogicalName);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DataverseEntityId).Value.Should().Be(randomGuid.ToString());
        //activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbStatement).Value.Should().Be("INSERT INTO TestEntity (name, address1) VALUES (testName, testAddress1)");
    }
}
