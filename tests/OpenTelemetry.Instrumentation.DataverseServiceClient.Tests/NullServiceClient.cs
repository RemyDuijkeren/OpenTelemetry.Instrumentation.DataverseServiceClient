using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

class NullServiceClient : IOrganizationService
{
    public Guid Create(Entity entity) => Guid.Empty;
    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => new() { LogicalName = entityName, Id = id };
    public void Update(Entity entity) { }
    public void Delete(string entityName, Guid id) { }
    public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }
    public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }
    public OrganizationResponse Execute(OrganizationRequest request) => new();
    public EntityCollection RetrieveMultiple(QueryBase query) => new();
}
