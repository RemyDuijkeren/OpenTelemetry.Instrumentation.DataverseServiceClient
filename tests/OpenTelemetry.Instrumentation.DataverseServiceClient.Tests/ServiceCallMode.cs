namespace OpenTelemetry.Instrumentation.DataverseServiceClient.Tests;

public enum ServiceCallMode
{
    Sync,
    Async,
    AsyncWithCancellationToken
}
