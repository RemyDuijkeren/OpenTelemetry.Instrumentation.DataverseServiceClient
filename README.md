# Dataverse ServiceClient Instrumentation for OpenTelemetry .NET

[![NuGet](https://img.shields.io/nuget/v/RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.svg)](https://www.nuget.org/packages/RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient)
[![NuGet](https://img.shields.io/nuget/dt/RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient.svg)](https://www.nuget.org/packages/RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient)

This is an [Instrumentation Library](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/glossary.md#instrumentation-library), which instruments [Dataverse ServiceClient](https://github.com/microsoft/PowerPlatform-DataverseServiceClient) and
collect traces about incoming Dataverse requests.

This component is based on the [v1.23](https://github.com/open-telemetry/semantic-conventions/tree/v1.23.0/docs/database)
of database semantic conventions. For details on the default set of attributes that
are added, checkout [Traces](#traces) sections below.

## Getting Started

### Step 1: Install Package

Add a reference to the [`RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient`](https://www.nuget.org/packages/RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient)
package. Also, add any other instrumentations & exporters you will need.

```shell
dotnet add package RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient
```

### Step 2: Enable Dataverse ServiceClient Instrumentation at application startup

Dataverse ServiceClient instrumentation must be enabled at application startup. This is typically done in the
`ConfigureServices` of your `Startup` class. Both examples below enables OpenTelemetry by calling `AddOpenTelemetry()`
on `IServiceCollection`.
This extension method requires adding the package [`OpenTelemetry.Extensions.Hosting`](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Extensions.Hosting/README.md)
to the application. This ensures instrumentations are disposed when the host is shutdown.

#### Traces

The following example demonstrates adding Dataverse ServiceClient instrumentation with the extension method
`WithTracing()` on `OpenTelemetryBuilder`. then extension method `AddDataverseServiceClientInstrumentation()` on
`TracerProviderBuilder` to the application. This example also sets up the Console Exporter, which requires adding the
package [`OpenTelemetry.Exporter.Console`](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.Console)
to the application.

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

public void ConfigureServices(IServiceCollection services)
{
    services.AddOpenTelemetry()
        .WithTracing(builder => builder
            .AddDataverseServiceClientInstrumentation()
            .AddConsoleExporter());
}
```

Following list of attributes are added by default on activity. See [db-spans](https://github.com/open-telemetry/semantic-conventions/blob/v1.23.0/docs/database/database-spans.md)
for more details about each individual attribute:

* `db.operation`
* `db.sql.table`
* `db.statement`
* `db.system`
* `db.name`
* `db.connection_string`
* `db.user`
* `db.dataverse.organization_id`
* `db.dataverse.organization_version`
* `db.dataverse.geo`
* `error.type`
* `server.address`
* `server.port`

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
