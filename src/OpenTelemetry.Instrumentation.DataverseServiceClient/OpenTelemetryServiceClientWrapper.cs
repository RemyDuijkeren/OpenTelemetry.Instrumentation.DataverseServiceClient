using System.Diagnostics;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient;

/// <summary>Decorate <see cref="IOrganizationServiceAsync2" /> interface with OpenTelemetry instrumentation.</summary>
public class OpenTelemetryServiceClientWrapper : IOrganizationServiceAsync2
{
    readonly IOrganizationServiceAsync2 _client;
    Guid ConnectedOrgId { get; }

    public OpenTelemetryServiceClientWrapper(IOrganizationServiceAsync2 serviceClient)
    {
        _client = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));

        ConnectedOrgId = serviceClient is ServiceClient client
            ? client.ConnectedOrgId
            : Guid.Empty;
    }

    /// <inheritdoc />
    public Guid Create(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using Activity? activity = _client.StartDataverseActivity(entity);
        return _client.Create(entity);
    }

    /// <inheritdoc />
    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, id));
        return _client.Retrieve(entityName, id, columnSet);
    }

    /// <inheritdoc />
    public void Update(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using Activity? activity = _client.StartDataverseActivity(entity);
        _client.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(string entityName, Guid id)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, id));
        _client.Delete(entityName, id);
    }

    /// <inheritdoc />
    public OrganizationResponse Execute(OrganizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

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

        using Activity? activity = _client.StartDataverseActivity(entityName: entityName, operation: request.RequestName);
        return _client.Execute(request);
    }

    /// <inheritdoc />
    public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, entityId));
        _client.Associate(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, entityId));
        _client.Disassociate(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public EntityCollection RetrieveMultiple(QueryBase query)
    {
        ArgumentNullException.ThrowIfNull(query);

        switch (query)
        {
            case QueryExpression queryExpression:
            {
                // TODO: format queryExpression to a statement to be used in the activity
                using Activity? activity = _client.StartDataverseActivity(queryExpression.EntityName);
                return _client.RetrieveMultiple(query);
            }
            case QueryByAttribute queryByAttribute:
            {
                // TODO: format queryByAttribute to a statement to be used in the activity
                using Activity? activity = _client.StartDataverseActivity(queryByAttribute.EntityName);
                return _client.RetrieveMultiple(query);
            }
            case FetchExpression fetchExpression:
            {
                using Activity? activity = _client.StartDataverseActivity(statement: fetchExpression.Query);
                return _client.RetrieveMultiple(query);
            }
            default:
                return _client.RetrieveMultiple(query);
        }
    }

    /// <inheritdoc />
    public Task<Guid> CreateAsync(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using Activity? activity = _client.StartDataverseActivity(entity);
        return _client.CreateAsync(entity);
    }

    /// <inheritdoc />
    public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, id));
        return _client.RetrieveAsync(entityName, id, columnSet);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using Activity? activity = _client.StartDataverseActivity(entity);
        return _client.UpdateAsync(entity);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string entityName, Guid id)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, id));
        return _client.DeleteAsync(entityName, id);
    }

    /// <inheritdoc />
    public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

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

        using Activity? activity1 = _client.StartDataverseActivity(entityName: entityName, operation: request.RequestName);
        return _client.ExecuteAsync(request);
    }

    /// <inheritdoc />
    public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, entityId));
        return _client.AssociateAsync(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, entityId));
        return _client.DisassociateAsync(entityName, entityId, relationship, relatedEntities);
    }

    /// <inheritdoc />
    public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query)
    {
        ArgumentNullException.ThrowIfNull(query);

        switch (query)
        {
            case QueryExpression queryExpression:
            {
                using Activity? activity = _client.StartDataverseActivity(queryExpression.EntityName);
                return _client.RetrieveMultipleAsync(query);
            }
            case QueryByAttribute queryByAttribute:
            {
                using Activity? activity = _client.StartDataverseActivity(queryByAttribute.EntityName);
                return _client.RetrieveMultipleAsync(query);
            }
            case FetchExpression fetchExpression:
            {
                using Activity? activity = _client.StartDataverseActivity(statement: fetchExpression.Query);
                return _client.RetrieveMultipleAsync(query);
            }
            default:
                return _client.RetrieveMultipleAsync(query);
        }
    }

    /// <inheritdoc />
    public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities,
        CancellationToken cancellationToken)
    {
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, entityId));
        return _client.AssociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Guid> CreateAsync(Entity entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using Activity? activity = _client.StartDataverseActivity(entity);
        return _client.CreateAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Entity> CreateAndReturnAsync(Entity entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using Activity? activity = _client.StartDataverseActivity(entity);
        return _client.CreateAndReturnAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string entityName, Guid id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, id));
        return _client.DeleteAsync(entityName, id, cancellationToken);
    }

    /// <inheritdoc />
    public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities,
        CancellationToken cancellationToken)
    {
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, entityId));
        return _client.DisassociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

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

        using Activity? activity1 = _client.StartDataverseActivity(entityName: entityName, operation: request.RequestName);
        return _client.ExecuteAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        using Activity? activity = _client.StartDataverseActivity(new EntityReference(entityName, id));
        return _client.RetrieveAsync(entityName, id, columnSet, cancellationToken);
    }

    /// <inheritdoc />
    public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        switch (query)
        {
            case QueryExpression queryExpression:
            {
                using Activity? activity = _client.StartDataverseActivity(queryExpression.EntityName);
                return _client.RetrieveMultipleAsync(query, cancellationToken);
            }
            case QueryByAttribute queryByAttribute:
            {
                using Activity? activity = _client.StartDataverseActivity(queryByAttribute.EntityName);
                return _client.RetrieveMultipleAsync(query, cancellationToken);
            }
            case FetchExpression fetchExpression:
            {
                using Activity? activity = _client.StartDataverseActivity(statement: fetchExpression.Query);
                return _client.RetrieveMultipleAsync(query, cancellationToken);
            }
            default:
                return _client.RetrieveMultipleAsync(query, cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task UpdateAsync(Entity entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        using Activity? activity = _client.StartDataverseActivity(entity);
        return _client.UpdateAsync(entity, cancellationToken);
    }

    public static implicit operator ServiceClient(OpenTelemetryServiceClientWrapper wrapper) =>
        wrapper._client as ServiceClient ?? throw new InvalidOperationException("Wrapped client is not a ServiceClient.");
}
