using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient;

public static class ServiceClientExtensions
{
    // Microsoft.PowerPlatform.Dataverse.Client ?
    public static readonly ActivitySource DataverseTracer = new("OpenTelemetry.Instrumentation.DataverseServiceClient", "1.0.0");
    const string DataverseSystem = "dataverse";

    /// <summary>Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.</summary>
    /// <param name="service">The <see cref="IOrganizationService"/> to start the Activity for</param>
    /// <param name="entityName">The logical name of the entity we operating on</param>
    /// <param name="statement">The optional statement associated with the activity.</param>
    /// <param name="operation">The optional name of the operation associated with the activity. If not specified, the caller's member name is used.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity. Returns null if the activity could not be started.</returns>
    public static Activity? StartDataverseActivity(this IOrganizationService service, string? entityName = null, string? statement = null, [CallerMemberName] string? operation = null)
    {
        Activity? activity = DataverseTracer.StartActivity(name: $"{operation} {entityName}",
            kind: ActivityKind.Client, tags: CreateInitialTags(service));

        activity?.SetTag(ActivityTags.DbOperation, operation);
        activity?.SetTag(ActivityTags.DbSqlTable, entityName);
        activity?.SetTag(ActivityTags.DbStatement, statement);

        return activity;
    }

    /// <summary>Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.</summary>
    /// <param name="service">The <see cref="IOrganizationService"/> to start the Activity for</param>
    /// <param name="entity">The <see cref="Entity"/> for which the activity is started.</param>
    /// <param name="statement">The optional statement associated with the activity.</param>
    /// <param name="operation">The optional name of the operation associated with the activity. If not specified, the caller's member name is used.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity. Returns null if the activity could not be started.</returns>
    public static Activity? StartDataverseActivity(this IOrganizationService service, Entity entity, string? statement = null, [CallerMemberName] string? operation = null)
    {
        Activity? activity = DataverseTracer.StartActivity(name: $"{operation} {entity.LogicalName}",
            kind: ActivityKind.Client, tags: CreateInitialTags(service));

        activity?.SetTag(ActivityTags.DbOperation, operation);
        activity?.SetTag(ActivityTags.DbSqlTable, entity.LogicalName);
        activity?.SetTag(ActivityTags.DbStatement, statement ?? $"EntityId: {entity.Id}");

        return activity;
    }

    /// <summary>Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.</summary>
    /// <param name="service">The <see cref="IOrganizationService"/> to start the Activity for</param>
    /// <param name="entityReference">The <see cref="EntityReference"/> for which the activity is started.</param>
    /// <param name="statement">The statement for the activity. Default is null.</param>
    /// <param name="operation">The name of the calling method. This parameter is automatically populated by the compiler. Default is null.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity. Returns null if the activity could not be started.</returns>
    public static Activity? StartDataverseActivity(this IOrganizationService service, EntityReference entityReference, string? statement = null, [CallerMemberName] string? operation = null)
    {
        Activity? activity = DataverseTracer.StartActivity(name: $"{operation} {entityReference.LogicalName}",
            kind: ActivityKind.Client, tags: CreateInitialTags(service));

        activity?.SetTag(ActivityTags.DbOperation, operation);
        activity?.SetTag(ActivityTags.DbSqlTable, entityReference.LogicalName);
        activity?.SetTag(ActivityTags.DbStatement, statement ?? $"EntityId: {entityReference.Id}");

        return activity;
    }

    static Dictionary<string, object?> CreateInitialTags(IOrganizationService service) =>
        service is ServiceClient client
            ? new Dictionary<string, object?>
            {
                [ActivityTags.ServerAddress] = client.ConnectedOrgUriActual,
                [ActivityTags.DbUrl] = client.ConnectedOrgUriActual,
                [ActivityTags.DbSystem] = DataverseSystem,
                [ActivityTags.DbName] = client.OrganizationDetail.UrlName,
                [ActivityTags.DbUser] = client.OAuthUserId,
                [ActivityTags.DbDataverseOrganizationId] = client.ConnectedOrgId.ToString(),
                [ActivityTags.DbDataverseOrganizationVersion] = client.ConnectedOrgVersion,
                [ActivityTags.DbDataverseGeo] = client.OrganizationDetail.Geo
            }
            : new Dictionary<string, object?>
            {
                [ActivityTags.DbSystem] = DataverseSystem,
                [ActivityTags.DbName] = DataverseSystem
            };
}
