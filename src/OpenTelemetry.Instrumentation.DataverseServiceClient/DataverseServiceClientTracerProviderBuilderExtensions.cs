using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using OpenTelemetry.Trace;

namespace RemyDuijkeren.OpenTelemetry.Instrumentation.DataverseServiceClient;

public static class DataverseServiceClientTracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddDataverseServiceClientInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(ServiceClientExtensions.DataverseTracer.Name);

        builder.ConfigureServices(services =>
        {
            TryReplaceOrganizationServiceRegistration<IOrganizationServiceAsync2>(services);
            TryReplaceOrganizationServiceRegistration<IOrganizationServiceAsync>(services);
            TryReplaceOrganizationServiceRegistration<IOrganizationService>(services);
        });

        return builder;
    }

    static void TryReplaceOrganizationServiceRegistration<TService>(IServiceCollection services)
        where TService : class, IOrganizationService
    {
        ServiceDescriptor? originalServiceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TService));

        if (originalServiceDescriptor == null) return;

        // Remove the original registration.
        services.Remove(originalServiceDescriptor);

        // Add the decorator, which uses the original implementation factory to create instances.
        if (originalServiceDescriptor.ImplementationInstance != null)
        {
            // if the service was registered as an instance
            var originalService = (TService)originalServiceDescriptor.ImplementationInstance;
            services.AddSingleton<TService>(_ =>
                // Here, we assume that OpenTelemetryServiceClientWrapper has a generic overload for TService as well
                new OpenTelemetryServiceClientWrapper(originalService) as TService
            );
        }
        else if (originalServiceDescriptor.ImplementationFactory != null)
        {
            // if the service was registered using factory method
            services.AddSingleton<TService>(serviceProvider =>
            {
                var originalService = (TService)originalServiceDescriptor.ImplementationFactory.Invoke(serviceProvider);
                return new OpenTelemetryServiceClientWrapper(originalService) as TService;
            });
        }
        else if (originalServiceDescriptor.ImplementationType != null)
        {
            // if the service was registered as a type (should not be possible because ServiceClient doesnt have a public constructor without parameters)
            services.AddSingleton<TService>(serviceProvider =>
            {
                var originalService = Activator.CreateInstance(originalServiceDescriptor.ImplementationType) as TService;
                return new OpenTelemetryServiceClientWrapper(originalService) as TService;
            });
        }
    }
}
