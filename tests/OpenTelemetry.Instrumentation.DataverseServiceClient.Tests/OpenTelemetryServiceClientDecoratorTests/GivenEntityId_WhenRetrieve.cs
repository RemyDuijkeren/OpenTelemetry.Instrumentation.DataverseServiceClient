using System.Diagnostics;
using System.ServiceModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenEntityId_WhenRetrieve
{
    readonly EntityReference _entityRef;
    readonly ColumnSet _columnSet;
    readonly IOrganizationServiceAsync2 _mockService;
    readonly OpenTelemetryServiceClientDecorator _decorator;
    readonly IDictionary<string, string?> _expectedTags;

    public GivenEntityId_WhenRetrieve()
    {
        _entityRef = new EntityReference("TestEntity", Guid.NewGuid());
        _columnSet = new ColumnSet("attribute1", "attribute2", "attribute3");

        _mockService = Substitute.For<IOrganizationServiceAsync2>();
        _decorator = new OpenTelemetryServiceClientDecorator(_mockService);

        _expectedTags = new Dictionary<string, string?>
        {
            { ActivityTags.DbSystem, "dataverse" },
            { ActivityTags.DbName, "dataverse" },
            { ActivityTags.DbOperation, "RetrieveAsync" },
            { ActivityTags.DbSqlTable, _entityRef.LogicalName },
            { ActivityTags.DataverseEntityId, _entityRef.Id.ToString() },
            { ActivityTags.DbStatement, $"SELECT {string.Join(", ", _columnSet.ToSqlColumns())} FROM testentity WHERE testentityid = '{_entityRef.Id}'" }
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
    public async Task CallRetrieveOnDecoratedService(ServiceCallMode serviceCallMode, string entityName)
    {
        switch (serviceCallMode)
        {
            case ServiceCallMode.Sync:
                // Act
                _decorator.Retrieve(entityName, _entityRef.Id, _columnSet);
                // Assert
                _mockService.Received(1).Retrieve(entityName, _entityRef.Id, _columnSet);
                break;
            case ServiceCallMode.Async:
                // Act
                await _decorator.RetrieveAsync(entityName, _entityRef.Id, _columnSet);
                // Assert
                await _mockService.Received(1).RetrieveAsync(entityName, _entityRef.Id, _columnSet);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                // Act
                await _decorator.RetrieveAsync(entityName, _entityRef.Id, _columnSet, new CancellationToken());
                // Assert
                await _mockService.Received(1).RetrieveAsync(entityName, _entityRef.Id, _columnSet, Arg.Any<CancellationToken>());
                break;
        }
    }

    [SkippableTheory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task ThrowNullReferenceExceptionByDecoratedService_WhenEntityNameIsNull_WithoutMocking(ServiceCallMode serviceCallMode)
    {
        Skip.IfNot(TestContext.EnvVarConnectionOptionsExists);

        // Arrange
        var service = TestContext.CreateServiceClientFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);

        if (serviceCallMode == ServiceCallMode.Sync)
        {
            // Act
            Action act1 = () => decorator.Retrieve(null!, Guid.NewGuid(), new ColumnSet());
            // Assert
            act1.Should().Throw<NullReferenceException>();
        }
        else
        {
            // Act
            Func<Task> act2 = () => serviceCallMode switch
            {
                ServiceCallMode.Async => decorator.RetrieveAsync(null!, Guid.NewGuid(), new ColumnSet()),
                ServiceCallMode.AsyncWithCancellationToken => decorator.RetrieveAsync(null!, Guid.NewGuid(), new ColumnSet(), new CancellationToken()),
            };

            // Assert
            await act2.Should().ThrowAsync<NullReferenceException>();
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
                _decorator.Retrieve(_entityRef.LogicalName, _entityRef.Id, _columnSet);
                break;
            case ServiceCallMode.Async:
                await _decorator.RetrieveAsync(_entityRef.LogicalName, _entityRef.Id, _columnSet);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                await _decorator.RetrieveAsync(_entityRef.LogicalName, _entityRef.Id, _columnSet, new CancellationToken());
                break;
        }

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Client);
        activity.Tags.Should().OnlyHaveUniqueItems();

        var operationName = serviceCallMode switch
        {
            ServiceCallMode.Sync => "Retrieve",
            ServiceCallMode.Async => "RetrieveAsync",
            ServiceCallMode.AsyncWithCancellationToken => "RetrieveAsync"
        };

        _expectedTags[ActivityTags.DbOperation] = operationName;

        activity.OperationName.Should().Be($"{operationName} {_entityRef.LogicalName}");
        activity.Tags.Should().Contain(_expectedTags);
    }
}
