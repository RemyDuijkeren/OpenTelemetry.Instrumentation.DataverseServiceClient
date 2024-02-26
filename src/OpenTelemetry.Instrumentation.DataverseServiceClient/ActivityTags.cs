namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient;

// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md
static class ActivityTags
{
    // Connection-level attributes
    public const string ServerAddress = "server.address"; // "shopdb.example.com"
    public const string ServerPort = "server.port"; // 3306

    public const string DbSystem = "db.system"; // mysql, dataverse
    public const string DbInstance = "db.instance.id";
    public const string DbName = "db.name"; // "ShopDb"
    public const string DbConnectionString = "db.connection_string"; // "Server=shopdb.example.com;Database=ShopDb;Uid=billing_user;TableCache=true;UseCompression=True;MinimumPoolSize=10;MaximumPoolSize=50;"
    public const string DbUser = "db.user"; // "billing_user"

    public const string DataverseOrgId = "db.dataverse.org_id";
    public const string DataverseOrgVersion = "db.dataverse.org_version"; // 9.2.24014.198
    public const string DataverseOrgType = "db.dataverse.org_type"; // Customer, Partner, etc.
    public const string DataverseSchemaType = "db.dataverse.schema_type"; // Customer, Partner, etc.
    public const string DataverseOrgFriendlyName = "db.dataverse.org_friendly_name"; // "Contoso Production"
    public const string DataverseGeo = "db.dataverse.geo"; // EMEA, US, etc.
    public const string DataverseSdkVersion = "db.dataverse.sdk_version"; // 9.2.49.6443
    public const string DataverseAuthType = "db.dataverse.auth_type"; // ClientSecret, OAuth

    // Call-level attributes
    public const string DbOperation = "db.operation"; // findAndModify; HMSET; SELECT
    public const string DbStatement = "db.statement"; // "SELECT * FROM orders WHERE order_id = 'o4711'"
    public const string DbSqlTable = "db.sql.table"; // "orders", "public.users"
    public const string DataverseEntityId = "db.dataverse.entity_id";

    // https://github.com/open-telemetry/semantic-conventions/blob/v1.24.0/docs/exceptions/exceptions-spans.md
    public const string ExceptionEventName = "exception";
    public const string ExceptionType = "exception.type";
    public const string ExceptionMessage = "exception.message";
    public const string ExceptionStacktrace = "exception.stacktrace";
    public const string ErrorType = "error.type";
}
