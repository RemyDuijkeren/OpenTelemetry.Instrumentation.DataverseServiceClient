using System.Text.Json;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Model;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public static class ServiceClientHelper
{
    const string DataverseConnectionOptions = "DATAVERSE_CONNECTION_OPTIONS";

    public static ServiceClient CreateFromEnvVar()
    {
        // Example of the JSON string that should be stored in the environment variable (without enters)
        // string json = """{ "AuthenticationType": 7, "ClientId": "...", "ClientSecret": "...", "ServiceUri": "https://....crm4.dynamics.com/" }""";

        var jsonString = Environment.GetEnvironmentVariable(DataverseConnectionOptions);
        if (jsonString == null)
            throw new InvalidOperationException($"Environment variable not found: {DataverseConnectionOptions}");

        var options = JsonSerializer.Deserialize<ConnectionOptions>(jsonString);
        return new ServiceClient(options);
    }

    public static bool EnvVarConnectionOptionsExists => Environment.GetEnvironmentVariable(DataverseConnectionOptions) != null;
}
