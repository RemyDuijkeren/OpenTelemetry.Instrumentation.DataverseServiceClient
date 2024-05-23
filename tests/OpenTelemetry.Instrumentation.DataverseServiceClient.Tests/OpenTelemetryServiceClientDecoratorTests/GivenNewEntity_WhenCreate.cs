using System.Diagnostics;
using System.ServiceModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenNewEntity_WhenCreate
{
    readonly Entity _entity;
    readonly Guid _createdEntityId;
    readonly IOrganizationServiceAsync2 _mockService;
    readonly OpenTelemetryServiceClientDecorator _decorator;
    IEnumerable<KeyValuePair<string, string?>> _expectedTags;

    public GivenNewEntity_WhenCreate()
    {
        _entity = new("TestEntity")
        {
            Attributes =
            {
                ["name"] = "testName",
                ["address1"] = "testAddress1",
            }
        };

        _createdEntityId = Guid.NewGuid();

        _mockService = Substitute.For<IOrganizationServiceAsync2>();
        _mockService.Create(Arg.Any<Entity>()).Returns(_createdEntityId);
        _mockService.CreateAsync(Arg.Any<Entity>()).Returns(_createdEntityId);
        _mockService.CreateAsync(Arg.Any<Entity>(), default).Returns(_createdEntityId);

        _decorator = new OpenTelemetryServiceClientDecorator(_mockService);

        _expectedTags = [
            new KeyValuePair<string, string?>(ActivityTags.DbSystem, "dataverse"),
            new KeyValuePair<string, string?>(ActivityTags.DbName, "dataverse"),
            new KeyValuePair<string, string?>(ActivityTags.DbOperation, "CreateAsync"),
            new KeyValuePair<string, string?>(ActivityTags.DbSqlTable, _entity.LogicalName),
            new KeyValuePair<string, string?>(ActivityTags.DataverseEntityId, _createdEntityId.ToString()),
            new KeyValuePair<string, string?>(ActivityTags.DbStatement, "INSERT INTO TestEntity (name, address1) VALUES (testName, testAddress1)")
        ];
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task CallCreateOrCreateAsyncOnDecoratedService(ServiceCallMode serviceCallMode)
    {
        switch (serviceCallMode)
        {
            case ServiceCallMode.Sync:
                // Act
                _decorator.Create(_entity);
                // Assert
                _mockService.Received(1).Create(_entity);
                break;
            case ServiceCallMode.Async:
                // Act
                await _decorator.CreateAsync(_entity);
                // Assert
                await _mockService.Received(1).CreateAsync(_entity);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                // Act
                await _decorator.CreateAsync(_entity, new CancellationToken());
                // Assert
                await _mockService.Received(1).CreateAsync(_entity, Arg.Any<CancellationToken>());
                break;
        }
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task CallCreateOrCreateAsyncOnDecoratedService_WhenEntityIsNull(ServiceCallMode serviceCallMode)
    {
        switch (serviceCallMode)
        {
            case ServiceCallMode.Sync:
                // Act
                _decorator.Create(null!);
                // Assert
                _mockService.Received(1).Create(null);
                break;
            case ServiceCallMode.Async:
                // Act
                await _decorator.CreateAsync(null!);
                // Assert
                await _mockService.Received(1).CreateAsync(null);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                // Act
                await _decorator.CreateAsync(null!, new CancellationToken());
                // Assert
                await _mockService.Received(1).CreateAsync(null, Arg.Any<CancellationToken>());
                break;
        }
    }

    [SkippableTheory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task ThrowUnderlyingFaultException_WhenEntityIsNull_WithoutMocking(ServiceCallMode serviceCallMode)
    {
        Skip.IfNot(ServiceClientHelper.EnvVarConnectionOptionsExists);

        // Arrange
        var service = ServiceClientHelper.CreateFromEnvVar();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        Entity entity = null!;

        // Act
        Func<Task> act = () => serviceCallMode switch
        {
            ServiceCallMode.Sync => Task.FromResult(decorator.Create(entity)),
            ServiceCallMode.Async => decorator.CreateAsync(entity),
            ServiceCallMode.AsyncWithCancellationToken => decorator.CreateAsync(entity, new CancellationToken()),
        };

        // Assert
        await act.Should().ThrowAsync<FaultException<OrganizationServiceFault>>();
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task ExportActivityWithTags_WhenCreateAsync2(ServiceCallMode serviceCallMode)
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
                _decorator.Create(_entity);
                break;
            case ServiceCallMode.Async:
                await _decorator.CreateAsync(_entity);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                await _decorator.CreateAsync(_entity, new CancellationToken());
                break;
        }

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Client);
        activity.Tags.Should().OnlyHaveUniqueItems();

        var operationName = serviceCallMode switch
        {
            ServiceCallMode.Sync => "Create",
            ServiceCallMode.Async => "CreateAsync",
            ServiceCallMode.AsyncWithCancellationToken => "CreateAsync"
        };

        _expectedTags = _expectedTags.Select(tag => tag.Key == ActivityTags.DbOperation ? new KeyValuePair<string, string?>(ActivityTags.DbOperation, operationName) : tag);

        activity.OperationName.Should().Be($"{operationName} {_entity.LogicalName}");
        activity.Tags.Should().Contain(_expectedTags);
    }
}
