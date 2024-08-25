using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenExistingEntity_WhenDelete
{
    readonly EntityReference _entityRef;
    readonly IOrganizationServiceAsync2 _mockService;
    readonly OpenTelemetryServiceClientDecorator _decorator;
    readonly IDictionary<string, string?> _expectedTags;

    public GivenExistingEntity_WhenDelete()
    {
        _entityRef = new EntityReference("TestEntity", Guid.NewGuid());

        _mockService = Substitute.For<IOrganizationServiceAsync2>();
        _decorator = new OpenTelemetryServiceClientDecorator(_mockService);

        _expectedTags = new Dictionary<string, string?>
        {
            { ActivityTags.DbSystem, "dataverse" },
            { ActivityTags.DbName, "dataverse" },
            { ActivityTags.DbOperation, "DeleteAsync" },
            { ActivityTags.DbSqlTable, _entityRef.LogicalName },
            { ActivityTags.DataverseEntityId, _entityRef.Id.ToString() },
            { ActivityTags.DbStatement, $"DELETE FROM testentity WHERE testentityid = '{_entityRef.Id}'" }
        };
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync, "TestEntity")]
    [InlineData(ServiceCallMode.Sync, "")]
    [InlineData(ServiceCallMode.Sync, null)]
    [InlineData(ServiceCallMode.Async, "TestEntity")]
    [InlineData(ServiceCallMode.Async, "")]
    [InlineData(ServiceCallMode.Async, null)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken, "TestEntity")]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken, "")]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken, null)]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification ="Forcing null for test")]
    public async Task CallDeleteOnDecoratedService(ServiceCallMode serviceCallMode, string entityName)
    {
        switch (serviceCallMode)
        {
            case ServiceCallMode.Sync:
                // Act
                _decorator.Delete(entityName, _entityRef.Id);
                // Assert
                _mockService.Received(1).Delete(entityName, _entityRef.Id);
                break;
            case ServiceCallMode.Async:
                // Act
                await _decorator.DeleteAsync(entityName, _entityRef.Id);
                // Assert
                await _mockService.Received(1).DeleteAsync(entityName, _entityRef.Id);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                // Act
                await _decorator.DeleteAsync(entityName, _entityRef.Id, new CancellationToken());
                // Assert
                await _mockService.Received(1).DeleteAsync(entityName, _entityRef.Id, Arg.Any<CancellationToken>());
                break;
        }
    }

    [SkippableTheory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task ThrowFaultExceptionByDecoratedService_WhenEntityNameIsNull_WithoutMocking(ServiceCallMode serviceCallMode)
    {
        Skip.IfNot(TestContext.CanCreateXrmRealContext);

        // Arrange
        using TestContext testContext = new();
        var service = testContext.XrmRealContext.GetAsyncOrganizationService2();
        var decorator = new OpenTelemetryServiceClientDecorator(service);

        // Act
        Func<Task> act = () => serviceCallMode switch
        {
            ServiceCallMode.Sync => Task.Run(() => decorator.Delete(null!, Guid.NewGuid())),
            ServiceCallMode.Async => decorator.DeleteAsync(null!, Guid.NewGuid()),
            ServiceCallMode.AsyncWithCancellationToken => decorator.DeleteAsync(null!, Guid.NewGuid(), new CancellationToken()),
            _ => throw new FluentAssertions.Execution.AssertionFailedException($"Unexpected ServiceCallMode: {serviceCallMode}")
        };

        // Assert
        await act.Should().ThrowAsync<FaultException<OrganizationServiceFault>>().WithMessage("Required member 'LogicalName' missing for field 'Target'");
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task ExportActivityWithTags_WhenExporterIsEnabled(ServiceCallMode serviceCallMode)
    {
        // Arrange
        var exportedItems = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddDataverseServiceClientInstrumentation()
            .AddInMemoryExporter(exportedItems)
            .Build();

        // Act
        switch (serviceCallMode)
        {
            case ServiceCallMode.Sync:
                _decorator.Delete(_entityRef.LogicalName, _entityRef.Id);
                break;
            case ServiceCallMode.Async:
                await _decorator.DeleteAsync(_entityRef.LogicalName, _entityRef.Id);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                await _decorator.DeleteAsync(_entityRef.LogicalName, _entityRef.Id, new CancellationToken());
                break;
        }

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Client);
        activity.Tags.Should().OnlyHaveUniqueItems();

        var operationName = serviceCallMode switch
        {
            ServiceCallMode.Sync => "Delete",
            ServiceCallMode.Async => "DeleteAsync",
            ServiceCallMode.AsyncWithCancellationToken => "DeleteAsync",
            _ => throw new FluentAssertions.Execution.AssertionFailedException($"Unexpected ServiceCallMode: {serviceCallMode}")
        };

        _expectedTags[ActivityTags.DbOperation] = operationName;

        activity.OperationName.Should().Be($"{operationName} {_entityRef.LogicalName}");
        activity.Tags.Should().Contain(_expectedTags);
    }
}
