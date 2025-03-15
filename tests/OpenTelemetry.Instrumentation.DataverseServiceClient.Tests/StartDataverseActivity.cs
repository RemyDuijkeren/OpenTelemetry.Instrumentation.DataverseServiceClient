using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class StartDataverseActivity
{
    [Fact]
    public void NotThrowException_When_EntityIsNull()
    {
        // Arrange
        var service = Substitute.For<IOrganizationService>();
        Entity entity = null!;

        // Act
        Action act = () => service.StartDataverseActivity(entity, null, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotThrowException_When_EntityReferenceIsNull()
    {
        // Arrange
        var service = Substitute.For<IOrganizationService>();
        EntityReference entityReference = null!;

        // Act
        Action act = () => service.StartDataverseActivity(entityReference, null, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotThrowException_When_EntityNameIsNull()
    {
        // Arrange
        var service = Substitute.For<IOrganizationService>();
        string? entityName = null;

        // Act
        Action act = () => service.StartDataverseActivity(entityName, null, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotThrowException_When_EntityNameIsNull_And_EntityIdIsDefault()
    {
        // Arrange
        var service = Substitute.For<IOrganizationService>();
        string? entityName = null;

        // Act
        Action act = () => service.StartDataverseActivity(entityName, Guid.Empty, null, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void TraceActivityWithExpectedTags_When_EntityAndOtherParams()
    {
        // Arrange
        var service = Substitute.For<IOrganizationService>();
        var entity = new Entity { LogicalName = "TestEntity", Id = Guid.NewGuid() };
        var statement = "TestStatement";
        var operation = "TestOperation";

        var exportedItems = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                                      .AddDataverseServiceClientInstrumentation()
                                      .AddInMemoryExporter(exportedItems)
                                      .Build();

        // Act
        using (var act = service.StartDataverseActivity(entity, statement, operation)) { }

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be($"{operation} {entity.LogicalName}");
        activity.Kind.Should().Be(ActivityKind.Client);

        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbOperation).Value.Should().Be(operation);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbSqlTable).Value.Should().Be(entity.LogicalName);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbStatement).Value.Should().Be(statement);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DataverseEntityId).Value.Should().Be(entity.Id.ToString());
    }

    [Fact]
    public void TraceActivityWithExpectedTags_When_EntityReferenceAndOtherParams()
    {
        // Arrange
        var service = Substitute.For<IOrganizationService>();
        var entity = new EntityReference { LogicalName = "TestEntity", Id = Guid.NewGuid() };
        var statement = "TestStatement";
        var operation = "TestOperation";

        var exportedItems = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                                      .AddDataverseServiceClientInstrumentation()
                                      .AddInMemoryExporter(exportedItems)
                                      .Build();

        // Act
        using (var act = service.StartDataverseActivity(entity, statement, operation)) { }

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be($"{operation} {entity.LogicalName}");
        activity.Kind.Should().Be(ActivityKind.Client);

        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbOperation).Value.Should().Be(operation);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbSqlTable).Value.Should().Be(entity.LogicalName);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbStatement).Value.Should().Be(statement);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DataverseEntityId).Value.Should().Be(entity.Id.ToString());
    }

    [Fact]
    public void TraceActivityWithExpectedTags_When_EntityNameAndOtherParams()
    {
        // Arrange
        var service = Substitute.For<IOrganizationService>();
        var entityName = "TestEntity";
        var statement = "TestStatement";
        var operation = "TestOperation";

        var exportedItems = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                                      .AddDataverseServiceClientInstrumentation()
                                      .AddInMemoryExporter(exportedItems)
                                      .Build();

        // Act
        using (var act = service.StartDataverseActivity(entityName, statement, operation)) { }

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be($"{operation} {entityName}");
        activity.Kind.Should().Be(ActivityKind.Client);

        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbOperation).Value.Should().Be(operation);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbSqlTable).Value.Should().Be(entityName);
        activity.Tags.SingleOrDefault(tag => tag.Key == ActivityTags.DbStatement).Value.Should().Be(statement);
    }
}
