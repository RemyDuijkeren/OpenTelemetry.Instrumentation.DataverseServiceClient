using System.Text.Json;
using FakeXrmEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Model;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public sealed class TestContext : IDisposable
{
    const string DataverseConnectionOptions = "DATAVERSE_CONNECTION_OPTIONS";

    // use static to create the ServiceClient from the environment variable only once (very costly operation)
    static readonly Lazy<ServiceClient> s_lazyServiceClientFromEnvVar = new(() => {
        // Example of the JSON string that should be stored in the environment variable (without enters)
        // string json = """{ "AuthenticationType": 7, "ClientId": "...", "ClientSecret": "...", "ServiceUri": "https://....crm4.dynamics.com/" }""";
        string? jsonString = Environment.GetEnvironmentVariable(DataverseConnectionOptions);
        if (jsonString == null)
            throw new InvalidOperationException($"Environment variable not found: {DataverseConnectionOptions}");

        ConnectionOptions? options = JsonSerializer.Deserialize<ConnectionOptions>(jsonString);
        return new ServiceClient(options);
    });

    readonly Lazy<IXrmFakedContext> _lazyFakedContext = new(() =>
        MiddlewareBuilder
            .New()
            .AddCrud()
            .UseCrud()
            .SetLicense(FakeXrmEasyLicense.NonCommercial)
            .Build());

    readonly Lazy<IXrmRealContext> _lazyRealContext = new(() =>
        new XrmRealContext(s_lazyServiceClientFromEnvVar.Value, s_lazyServiceClientFromEnvVar.Value, s_lazyServiceClientFromEnvVar.Value)
        {
            LicenseContext = FakeXrmEasyLicense.NonCommercial
        });

    public static bool CanCreateXrmRealContext => Environment.GetEnvironmentVariable(DataverseConnectionOptions) != null;

    public IXrmFakedContext XrmFakedContext => _lazyFakedContext.Value;

    public IXrmRealContext XrmRealContext => _lazyRealContext.Value;

    #region IDisposable Disposable pattern

    bool _disposedValue = false; // To detect redundant calls

    void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~TestContext() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion
}
