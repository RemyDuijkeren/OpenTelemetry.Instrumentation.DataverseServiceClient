using System.Diagnostics;
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
        Skip.IfNot(TestContext.EnvVarConnectionOptionsExists);

        // Arrange
        var service = TestContext.CreateServiceClientFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);

        if (serviceCallMode == ServiceCallMode.Sync)
        {
            // Act
            Action act1 = () => decorator.Delete(null!, Guid.NewGuid());
            // Assert
            act1.Should().Throw<FaultException<OrganizationServiceFault>>().WithMessage("Required member 'LogicalName' missing for field 'Target'");
        }
        else
        {
            // Act
            Func<Task> act2 = () => serviceCallMode switch
            {
                ServiceCallMode.Async => decorator.DeleteAsync(null!, Guid.NewGuid()),
                ServiceCallMode.AsyncWithCancellationToken => decorator.DeleteAsync(null!, Guid.NewGuid(), new CancellationToken()),
            };

            // Assert
            await act2.Should().ThrowAsync<FaultException<OrganizationServiceFault>>().WithMessage("Required member 'LogicalName' missing for field 'Target'");
        }
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
            ServiceCallMode.AsyncWithCancellationToken => "DeleteAsync"
        };

        _expectedTags[ActivityTags.DbOperation] = operationName;

        activity.OperationName.Should().Be($"{operationName} {_entityRef.LogicalName}");
        activity.Tags.Should().Contain(_expectedTags);
    }
}
