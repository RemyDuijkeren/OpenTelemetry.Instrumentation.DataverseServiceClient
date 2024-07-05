using System.Diagnostics;
using System.ServiceModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public class GivenExistingEntity_WhenUpdate
{
    readonly Entity _entity;
    readonly IOrganizationServiceAsync2 _mockService;
    readonly OpenTelemetryServiceClientDecorator _decorator;
    readonly IDictionary<string, string?> _expectedTags;

    public GivenExistingEntity_WhenUpdate()
    {
        _entity = new Entity("TestEntity", Guid.NewGuid())
        {
            Attributes =
            {
                ["name"] = "testName",
                ["address1"] = "testAddress1",
            }
        };

        _mockService = Substitute.For<IOrganizationServiceAsync2>();
        _decorator = new OpenTelemetryServiceClientDecorator(_mockService);

        _expectedTags = new Dictionary<string, string?>
        {
            { ActivityTags.DbSystem, "dataverse" },
            { ActivityTags.DbName, "dataverse" },
            { ActivityTags.DbOperation, "UpdateAsync" },
            { ActivityTags.DbSqlTable, _entity.LogicalName },
            { ActivityTags.DataverseEntityId, _entity.Id.ToString() },
            { ActivityTags.DbStatement, _entity.ToUpdateStatement() }
        };
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task CallUpdateOnDecoratedService(ServiceCallMode serviceCallMode)
    {
        switch (serviceCallMode)
        {
            case ServiceCallMode.Sync:
                // Act
                _decorator.Update(_entity);
                // Assert
                _mockService.Received(1).Update(_entity);
                break;
            case ServiceCallMode.Async:
                // Act
                await _decorator.UpdateAsync(_entity);
                // Assert
                await _mockService.Received(1).UpdateAsync(_entity);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                // Act
                await _decorator.UpdateAsync(_entity, new CancellationToken());
                // Assert
                await _mockService.Received(1).UpdateAsync(_entity, Arg.Any<CancellationToken>());
                break;
        }
    }

    [Theory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task CallUpdateOnDecoratedService_WhenEntityIsNull(ServiceCallMode serviceCallMode)
    {
        switch (serviceCallMode)
        {
            case ServiceCallMode.Sync:
                // Act
                _decorator.Update(null!);
                // Assert
                _mockService.Received(1).Update(null);
                break;
            case ServiceCallMode.Async:
                // Act
                await _decorator.UpdateAsync(null!);
                // Assert
                await _mockService.Received(1).UpdateAsync(null);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                // Act
                await _decorator.UpdateAsync(null!, new CancellationToken());
                // Assert
                await _mockService.Received(1).UpdateAsync(null, Arg.Any<CancellationToken>());
                break;
        }
    }

    [SkippableTheory]
    [InlineData(ServiceCallMode.Sync)]
    [InlineData(ServiceCallMode.Async)]
    [InlineData(ServiceCallMode.AsyncWithCancellationToken)]
    public async Task ThrowFaultExceptionByDecoratedService_WhenEntityIsNull_WithoutMocking(ServiceCallMode serviceCallMode)
    {
        Skip.IfNot(TestContext.CanCreateXrmRealContext);

        // Arrange
        using TestContext testContext = new();
        var service = testContext.XrmRealContext.GetAsyncOrganizationService2();
        var decorator = new OpenTelemetryServiceClientDecorator(service);
        Entity entity = null!;

        if (serviceCallMode == ServiceCallMode.Sync)
        {
            // Act
            Action act1 = () => decorator.Update(entity);
            // Assert
            act1.Should().Throw<FaultException<OrganizationServiceFault>>().WithMessage("Required field 'Target' is missing");
        }
        else
        {
            // Act
            Func<Task> act2 = () => serviceCallMode switch
            {
                ServiceCallMode.Async => decorator.UpdateAsync(entity),
                ServiceCallMode.AsyncWithCancellationToken => decorator.UpdateAsync(entity, new CancellationToken()),
            };

            // Assert
            await act2.Should().ThrowAsync<FaultException<OrganizationServiceFault>>().WithMessage("Required field 'Target' is missing");
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
                _decorator.Update(_entity);
                break;
            case ServiceCallMode.Async:
                await _decorator.UpdateAsync(_entity);
                break;
            case ServiceCallMode.AsyncWithCancellationToken:
                await _decorator.UpdateAsync(_entity, new CancellationToken());
                break;
        }

        // Assert
        var activity = exportedItems.FirstOrDefault();

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Client);
        activity.Tags.Should().OnlyHaveUniqueItems();

        var operationName = serviceCallMode switch
        {
            ServiceCallMode.Sync => "Update",
            ServiceCallMode.Async => "UpdateAsync",
            ServiceCallMode.AsyncWithCancellationToken => "UpdateAsync"
        };

        _expectedTags[ActivityTags.DbOperation] = operationName;

        activity.OperationName.Should().Be($"{operationName} {_entity.LogicalName}");
        activity.Tags.Should().Contain(_expectedTags);
    }
}
