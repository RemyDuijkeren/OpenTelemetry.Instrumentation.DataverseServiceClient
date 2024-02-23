using System.Diagnostics;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient;

/// <summary>Decorate <see cref="IOrganizationServiceAsync2" /> interface with OpenTelemetry instrumentation.</summary>
public class OpenTelemetryServiceClientDecorator : IOrganizationServiceAsync2
{
    readonly IOrganizationService _service;

    /// <summary>Decorate <see cref="IOrganizationServiceAsync2" /> interface with OpenTelemetry instrumentation.</summary>
    public OpenTelemetryServiceClientDecorator(IOrganizationService? serviceClient)
    {
        _service = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
    }

    IOrganizationService Service => _service;

    IOrganizationServiceAsync ServiceAsync => _service as IOrganizationServiceAsync ??
                                              throw new InvalidOperationException($"Wrapped service client is not a {typeof(IOrganizationServiceAsync)}!");

    IOrganizationServiceAsync2 ServiceAsync2 => _service as IOrganizationServiceAsync2 ??
                                                throw new InvalidOperationException($"Wrapped service client is not a {typeof(IOrganizationServiceAsync2)}!");

    Guid ConnectedOrgId => _service is ServiceClient client ? client.ConnectedOrgId : Guid.Empty;

    /// <inheritdoc />
    public Guid Create(Entity entity)
    {
        using Activity? activity = Service.StartDataverseActivity(entity);
        var id = Service.Create(entity);
        return id;
    }

    /// <inheritdoc />
    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
    {
        using Activity? activity = Service.StartDataverseActivity(new EntityReference(entityName, id));
        return Service.Retrieve(entityName, id, columnSet);
    }

    /// <inheritdoc />
    public void Update(Entity entity)
    {
        using Activity? activity = Service.StartDataverseActivity(entity);
        Service.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(string entityName, Guid id)
    {
        using Activity? activity = Service.StartDataverseActivity(new EntityReference(entityName, id));
        Service.Delete(entityName, id);
    }

    /// <inheritdoc />
    public OrganizationResponse Execute(OrganizationRequest request)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (request is null)
        {
            using Activity? act = Service.StartDataverseActivity();
            return Service.Execute(request);
        }

        string? entityName = request.Parameters.TryGetValue("LogicalName", out var logicalName) ? logicalName?.ToString() : null;
        if (entityName is null)
        {
            if (request.Parameters.TryGetValue("Target", out var target))
            {
                entityName = target switch
                {
                    Entity entity => entity.LogicalName,
                    EntityReference entityReference => entityReference.LogicalName,
                    _ => target?.GetType().Name
                };
            }
        }

        using Activity? activity = Service.StartDataverseActivity(entityName: entityName, operation: request.RequestName);
        return Service.Execute(request);
    }

    /// <inheritdoc />
    public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = Service.StartDataverseActivity(new EntityReference(entityName, entityId));
        Service.Associate(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = Service.StartDataverseActivity(new EntityReference(entityName, entityId));
        Service.Disassociate(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public EntityCollection RetrieveMultiple(QueryBase query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        switch (query)
        {
            case QueryExpression queryExpression:
            {
                // TODO: format queryExpression to a statement to be used in the activity
                using Activity? activity = Service.StartDataverseActivity(queryExpression.EntityName);
                return Service.RetrieveMultiple(query);
            }
            case QueryByAttribute queryByAttribute:
            {
                // TODO: format queryByAttribute to a statement to be used in the activity
                using Activity? activity = Service.StartDataverseActivity(queryByAttribute.EntityName);
                return Service.RetrieveMultiple(query);
            }
            case FetchExpression fetchExpression:
            {
                using Activity? activity = Service.StartDataverseActivity(statement: fetchExpression.Query);
                return Service.RetrieveMultiple(query);
            }
            default:
                return Service.RetrieveMultiple(query);
        }
    }

    /// <inheritdoc />
    public Task<Guid> CreateAsync(Entity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        using Activity? activity = ServiceAsync.StartDataverseActivity(entity);
        return ServiceAsync.CreateAsync(entity);
    }

    /// <inheritdoc />
    public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet)
    {
        if (entityName is null) throw new ArgumentNullException(nameof(entityName));
        using Activity? activity = ServiceAsync.StartDataverseActivity(new EntityReference(entityName, id));
        return ServiceAsync.RetrieveAsync(entityName, id, columnSet);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Entity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        using Activity? activity = ServiceAsync.StartDataverseActivity(entity);
        return ServiceAsync.UpdateAsync(entity);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string entityName, Guid id)
    {
        if (entityName is null) throw new ArgumentNullException(nameof(entityName));
        using Activity? activity = ServiceAsync.StartDataverseActivity(new EntityReference(entityName, id));
        return ServiceAsync.DeleteAsync(entityName, id);
    }

    /// <inheritdoc />
    public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        string? entityName = request.Parameters.TryGetValue("LogicalName", out var logicalName) ? logicalName?.ToString() : null;
        if (entityName is null)
        {
            if (request.Parameters.TryGetValue("Target", out var target))
            {
                entityName = target switch
                {
                    Entity entity => entity.LogicalName,
                    EntityReference entityReference => entityReference.LogicalName,
                    _ => target?.GetType().Name
                };
            }
        }

        using Activity? activity1 = ServiceAsync.StartDataverseActivity(entityName: entityName, operation: request.RequestName);
        return ServiceAsync.ExecuteAsync(request);
    }

    /// <inheritdoc />
    public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = ServiceAsync.StartDataverseActivity(new EntityReference(entityName, entityId));
        return ServiceAsync.AssociateAsync(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = ServiceAsync.StartDataverseActivity(new EntityReference(entityName, entityId));
        return ServiceAsync.DisassociateAsync(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        switch (query)
        {
            case QueryExpression queryExpression:
            {
                using Activity? activity = ServiceAsync.StartDataverseActivity(queryExpression.EntityName);
                return ServiceAsync.RetrieveMultipleAsync(query);
            }
            case QueryByAttribute queryByAttribute:
            {
                using Activity? activity = ServiceAsync.StartDataverseActivity(queryByAttribute.EntityName);
                return ServiceAsync.RetrieveMultipleAsync(query);
            }
            case FetchExpression fetchExpression:
            {
                using Activity? activity = ServiceAsync.StartDataverseActivity(statement: fetchExpression.Query);
                return ServiceAsync.RetrieveMultipleAsync(query);
            }
            default:
                return ServiceAsync.RetrieveMultipleAsync(query);
        }
    }

    /// <inheritdoc />
    public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities,
        CancellationToken cancellationToken)
    {
        using Activity? activity = ServiceAsync2.StartDataverseActivity(new EntityReference(entityName, entityId));
        return ServiceAsync2.AssociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Guid> CreateAsync(Entity entity, CancellationToken cancellationToken)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        using Activity? activity = ServiceAsync2.StartDataverseActivity(entity);
        return ServiceAsync2.CreateAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Entity> CreateAndReturnAsync(Entity entity, CancellationToken cancellationToken)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        using Activity? activity = ServiceAsync2.StartDataverseActivity(entity);
        return ServiceAsync2.CreateAndReturnAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string entityName, Guid id, CancellationToken cancellationToken)
    {
        if (entityName is null) throw new ArgumentNullException(nameof(entityName));
        using Activity? activity = ServiceAsync2.StartDataverseActivity(new EntityReference(entityName, id));
        return ServiceAsync2.DeleteAsync(entityName, id, cancellationToken);
    }

    /// <inheritdoc />
    public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities,
        CancellationToken cancellationToken)
    {
        using Activity? activity = ServiceAsync2.StartDataverseActivity(new EntityReference(entityName, entityId));
        return ServiceAsync2.DisassociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        string? entityName = request.Parameters.TryGetValue("LogicalName", out var logicalName) ? logicalName?.ToString() : null;
        if (entityName is null)
        {
            if (request.Parameters.TryGetValue("Target", out var target))
            {
                entityName = target switch
                {
                    Entity entity => entity.LogicalName,
                    EntityReference entityReference => entityReference.LogicalName,
                    _ => target?.GetType().Name
                };
            }
        }

        using Activity? activity1 = ServiceAsync2.StartDataverseActivity(entityName: entityName, operation: request.RequestName);
        return ServiceAsync2.ExecuteAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet, CancellationToken cancellationToken)
    {
        if (entityName is null) throw new ArgumentNullException(nameof(entityName));
        using Activity? activity = ServiceAsync2.StartDataverseActivity(new EntityReference(entityName, id));
        return ServiceAsync2.RetrieveAsync(entityName, id, columnSet, cancellationToken);
    }

    /// <inheritdoc />
    public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, CancellationToken cancellationToken)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        switch (query)
        {
            case QueryExpression queryExpression:
            {
                using Activity? activity = ServiceAsync2.StartDataverseActivity(queryExpression.EntityName);
                return ServiceAsync2.RetrieveMultipleAsync(query, cancellationToken);
            }
            case QueryByAttribute queryByAttribute:
            {
                using Activity? activity = ServiceAsync2.StartDataverseActivity(queryByAttribute.EntityName);
                return ServiceAsync2.RetrieveMultipleAsync(query, cancellationToken);
            }
            case FetchExpression fetchExpression:
            {
                using Activity? activity = ServiceAsync2.StartDataverseActivity(statement: fetchExpression.Query);
                return ServiceAsync2.RetrieveMultipleAsync(query, cancellationToken);
            }
            default:
                return ServiceAsync2.RetrieveMultipleAsync(query, cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task UpdateAsync(Entity entity, CancellationToken cancellationToken)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        using Activity? activity = ServiceAsync2.StartDataverseActivity(entity);
        return ServiceAsync2.UpdateAsync(entity, cancellationToken);
    }

    public static explicit operator ServiceClient(OpenTelemetryServiceClientDecorator decorator) =>
        decorator._service as ServiceClient ?? throw new InvalidOperationException("Decorated client is not a ServiceClient.");
}
