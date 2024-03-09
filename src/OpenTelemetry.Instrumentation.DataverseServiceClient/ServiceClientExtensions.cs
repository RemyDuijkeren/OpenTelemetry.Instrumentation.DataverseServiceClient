using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient;

/// <summary>Contains extension methods for working with the ServiceClient class.</summary>
public static class ServiceClientExtensions
{
    const string DataverseSystem = "dataverse";

    public static readonly ActivitySource DataverseTracer =
        new("OpenTelemetry.Instrumentation.DataverseServiceClient", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0");

    /// <summary>Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.</summary>
    /// <param name="service">The <see cref="IOrganizationService"/> to start the Activity for</param>
    /// <param name="entityName">The logical name of the entity we operating on</param>
    /// <param name="statement">The optional statement associated with the activity.</param>
    /// <param name="operation">The optional name of the operation associated with the activity. If not specified, the caller's member name is used.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity. Returns null if the activity could not be started.</returns>
    public static Activity? StartDataverseActivity(this IOrganizationService service, string? entityName = null, string? statement = null,
        [CallerMemberName] string? operation = null) =>
        StartActivityInternal(service, $"{operation} {entityName}", operation, entityName, null, statement);

    /// <summary>Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.</summary>
    /// <param name="service">The <see cref="IOrganizationService"/> to start the Activity for</param>
    /// <param name="entityName">The logical name of the entity we operating on</param>
    /// <param name="entityId">The id of the entity we operating on</param>
    /// <param name="statement">The optional statement associated with the activity.</param>
    /// <param name="operation">The optional name of the operation associated with the activity. If not specified, the caller's member name is used.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity. Returns null if the activity could not be started.</returns>
    public static Activity? StartDataverseActivity(this IOrganizationService service, string? entityName, Guid entityId, string? statement = null,
        [CallerMemberName] string? operation = null) =>
        StartActivityInternal(service, $"{operation} {entityName}", operation, entityName, entityId, statement);

    /// <summary>Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.</summary>
    /// <param name="service">The <see cref="IOrganizationService"/> to start the Activity for</param>
    /// <param name="entity">The <see cref="Entity"/> for which the activity is started.</param>
    /// <param name="statement">The optional statement associated with the activity.</param>
    /// <param name="operation">The optional name of the operation associated with the activity. If not specified, the caller's member name is used.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity. Returns null if the activity could not be started.</returns>
    public static Activity? StartDataverseActivity(this IOrganizationService service, Entity? entity, string? statement = null,
        [CallerMemberName] string? operation = null) =>
        StartActivityInternal(service, $"{operation} {entity?.LogicalName}", operation, entity?.LogicalName, entity?.Id, statement);

    /// <summary>Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity events, returns null otherwise.</summary>
    /// <param name="service">The <see cref="IOrganizationService"/> to start the Activity for</param>
    /// <param name="entityReference">The <see cref="EntityReference"/> for which the activity is started.</param>
    /// <param name="statement">The statement for the activity. Default is null.</param>
    /// <param name="operation">The name of the calling method. This parameter is automatically populated by the compiler. Default is null.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity. Returns null if the activity could not be started.</returns>
    public static Activity? StartDataverseActivity(this IOrganizationService service, EntityReference? entityReference, string? statement = null,
        [CallerMemberName] string? operation = null) =>
        StartActivityInternal(service, $"{operation} {entityReference?.LogicalName}", operation, entityReference?.LogicalName, entityReference?.Id, statement);

    static Activity? StartActivityInternal(this IOrganizationService service, string spanName, string? operation, string? entityName, Guid? entityId,
        string? statement = null)
    {
        var activity = DataverseTracer.StartActivity(name: spanName, kind: ActivityKind.Client, tags: CreateConnectionLevelTags(service));
        if (activity is null) return activity;

        if (operation is not null) activity.SetTag(ActivityTags.DbOperation, operation);
        if (entityName is not null) activity.SetTag(ActivityTags.DbSqlTable, entityName);
        if (statement is not null) activity.SetTag(ActivityTags.DbStatement, statement);
        if (entityId is not null) activity.SetTag(ActivityTags.DataverseEntityId, entityId.ToString());

        return activity;
    }

    static Dictionary<string, object?> CreateConnectionLevelTags(IOrganizationService service)
    {
        ServiceClient? serviceClient = GetServiceClient(service);

        return serviceClient is null
            ? new Dictionary<string, object?> { [ActivityTags.DbSystem] = DataverseSystem, [ActivityTags.DbName] = DataverseSystem }
            : new Dictionary<string, object?>
            {
                [ActivityTags.ServerAddress] = serviceClient.ConnectedOrgUriActual.Host,
                [ActivityTags.DbSystem] = DataverseSystem,
                [ActivityTags.DbName] = serviceClient.OrganizationDetail.UrlName,
                [ActivityTags.DbUser] = serviceClient.OAuthUserId,
                [ActivityTags.DataverseOrgId] = serviceClient.ConnectedOrgId.ToString(),
                [ActivityTags.DataverseOrgVersion] = serviceClient.ConnectedOrgVersion,
                [ActivityTags.DataverseOrgType] = serviceClient.OrganizationDetail.OrganizationType,
                [ActivityTags.DataverseOrgFriendlyName] = serviceClient.ConnectedOrgFriendlyName,
                [ActivityTags.DataverseSdkVersion] = serviceClient.SdkVersionProperty,
                [ActivityTags.DataverseSchemaType] = serviceClient.OrganizationDetail.SchemaType,
                [ActivityTags.DataverseAuthType] = serviceClient.ActiveAuthenticationType,
                [ActivityTags.DataverseGeo] = serviceClient.OrganizationDetail.Geo
            };
    }

    static ServiceClient? GetServiceClient(IOrganizationService service) =>
        service switch
        {
            OpenTelemetryServiceClientDecorator decorator => decorator.InternalServiceClient,
            ServiceClient serviceClient => serviceClient,
            _ => null,
        };
}
