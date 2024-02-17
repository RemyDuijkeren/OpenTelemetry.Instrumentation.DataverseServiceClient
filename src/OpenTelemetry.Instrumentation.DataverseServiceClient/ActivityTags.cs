namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient;

static class ActivityTags
{
    public const string DbOperation = "db.operation";
    public const string DbSqlTable = "db.sql.table";
    public const string DbStatement = "db.statement";
    public const string DbSystem = "db.system";
    public const string DbName = "db.name";
    public const string DbConnectionString = "db.connection_string";
    public const string DbUser = "db.user";
    public const string DbDataverseOrganizationId = "db.dataverse.organization_id";
    public const string DbDataverseOrganizationVersion = "db.dataverse.organization_version";
    public const string DbDataverseGeo = "db.dataverse.geo";
    public const string ErrorType = "error.type";
    public const string ServerAddress = "server.address";
    public const string ServerPort = "server.port";
}
