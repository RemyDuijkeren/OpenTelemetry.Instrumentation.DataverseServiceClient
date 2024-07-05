using System.Diagnostics;
using System.ServiceModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenNewEntity_WhenCreate
{
    readonly Entity _entity;
    readonly Entity _createdEntity;
    readonly IOrganizationServiceAsync2 _mockService;
    readonly OpenTelemetryServiceClientDecorator _decorator;
    readonly IDictionary<string, string?> _expectedTags;

    public GivenNewEntity_WhenCreate()
    {
        _entity = new Entity("TestEntity")
        {
            Attributes =
            {
                ["name"] = "testName",
                ["address1"] = "testAddress1",
            }
        };

        _createdEntity = new Entity(_entity.LogicalName, Guid.NewGuid()) { Attributes = _entity.Attributes };

        _mockService = Substitute.For<IOrganizationServiceAsync2>();
        _mockService.Create(Arg.Any<Entity>()).Returns(_createdEntity.Id);
        _mockService.CreateAsync(Arg.Any<Entity>()).Returns(_createdEntity.Id);
        _mockService.CreateAsync(Arg.Any<Entity>(), default).Returns(_createdEntity.Id);
        _mockService.CreateAndReturnAsync(Arg.Any<Entity>(), default).Returns(_createdEntity);

        _decorator = new OpenTelemetryServiceClientDecorator(_mockService);

        _expectedTags = new Dictionary<string, string?>
        {
            { ActivityTags.DbSystem, "dataverse" },
            { ActivityTags.DbName, "dataverse" },
            { ActivityTags.DbOperation, "CreateAsync" },
            { ActivityTags.DbSqlTable, _entity.LogicalName },
            { ActivityTags.DataverseEntityId, _createdEntity.Id.ToString() },
            { ActivityTags.DbStatement, _entity.ToInsertStatement() }
        };
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken, true)]
    public async Task CallCreateOnDecoratedService(ServiceCallMode serviceCallMode, bool returnCreatedEntity = false)
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
            case ServiceCallMode.AsyncWithCancellationToken when returnCreatedEntity:
                // Act
                await _decorator.CreateAndReturnAsync(_entity, new CancellationToken());
                // Assert
                await _mockService.Received(1).CreateAndReturnAsync(_entity, Arg.Any<CancellationToken>());
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
    [InlineData(ServiceCallMode.AsyncWithCancellationToken, true)]
    public async Task CallCreateOnDecoratedService_WhenEntityIsNull(ServiceCallMode serviceCallMode, bool returnCreatedEntity = false)
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
            case ServiceCallMode.AsyncWithCancellationToken when returnCreatedEntity:
                // Act
                await _decorator.CreateAndReturnAsync(null!, new CancellationToken());
                // Assert
                await _mockService.Received(1).CreateAndReturnAsync(null, Arg.Any<CancellationToken>());
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
    [InlineData(ServiceCallMode.AsyncWithCancellationToken, true)]
    public async Task ThrowFaultExceptionByDecoratedService_WhenEntityIsNull_WithoutMocking(ServiceCallMode serviceCallMode, bool returnCreatedEntity = false)
    {
        Skip.IfNot(TestContext.CanCreateXrmRealContext);

        // Arrange
        using TestContext testContext = new();
        var service = testContext.XrmRealContext.GetAsyncOrganizationService2();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        Entity entity = null!;

        // Act
        Func<Task> act = () => serviceCallMode switch
        {
            ServiceCallMode.Sync => Task.FromResult(decorator.Create(entity)),
            ServiceCallMode.Async => decorator.CreateAsync(entity),
            ServiceCallMode.AsyncWithCancellationToken when returnCreatedEntity => decorator.CreateAndReturnAsync(entity, new CancellationToken()),
            ServiceCallMode.AsyncWithCancellationToken => decorator.CreateAsync(entity, new CancellationToken()),
        };

        // Assert
        await act.Should().ThrowAsync<FaultException<OrganizationServiceFault>>().WithMessage("Required field 'Target' is missing");
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task ReturnCreatedId(ServiceCallMode serviceCallMode)
    {
        // Act
        Guid createdId = serviceCallMode switch
        {
            ServiceCallMode.Sync => _decorator.Create(_entity),
            ServiceCallMode.Async => await _decorator.CreateAsync(_entity),
            ServiceCallMode.AsyncWithCancellationToken => await _decorator.CreateAsync(_entity, new CancellationToken()),
        };

        // Assert
        createdId.Should().Be(_createdEntity.Id);
    }

    [Fact]
    public async Task ReturnCreatedEntity()
    {
        // Act
        Entity createdEntity = await _decorator.CreateAndReturnAsync(_entity, new CancellationToken());

        // Assert
        createdEntity.Should().BeEquivalentTo(_createdEntity);
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken, true)]
    public async Task ExportActivityWithTags_WhenExporterIsEnabled(ServiceCallMode serviceCallMode, bool returnCreatedEntity = false)
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
            case ServiceCallMode.AsyncWithCancellationToken when returnCreatedEntity:
                await _decorator.CreateAndReturnAsync(_entity, new CancellationToken());
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
            ServiceCallMode.AsyncWithCancellationToken when returnCreatedEntity => "CreateAndReturnAsync",
            ServiceCallMode.AsyncWithCancellationToken => "CreateAsync"
        };

        _expectedTags[ActivityTags.DbOperation] = operationName;

        activity.OperationName.Should().Be($"{operationName} {_entity.LogicalName}");
        activity.Tags.Should().Contain(_expectedTags);
    }
}
