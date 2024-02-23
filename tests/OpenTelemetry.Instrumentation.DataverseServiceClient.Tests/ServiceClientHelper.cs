using System.Text.Json;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Model;
using Microsoft.Xrm.Sdk;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public static class ServiceClientHelper
{
    const string DataverseConnectionOptions = "DATAVERSE_CONNECTION_OPTIONS";

    public static ServiceClient CreateFromEnvVar()
    {
        var jsonString = Environment.GetEnvironmentVariable(DataverseConnectionOptions);
        if (jsonString == null)
            throw new InvalidOperationException($"Environment variable not found: {DataverseConnectionOptions}");

        var options = JsonSerializer.Deserialize<ConnectionOptions>(jsonString);
        return new ServiceClient(options);
    }

    public static IOrganizationService CreateNullServiceClient() => new NullServiceClient();
    public static bool EnvVarConnectionOptionsExists => Environment.GetEnvironmentVariable(DataverseConnectionOptions) != null;
}
