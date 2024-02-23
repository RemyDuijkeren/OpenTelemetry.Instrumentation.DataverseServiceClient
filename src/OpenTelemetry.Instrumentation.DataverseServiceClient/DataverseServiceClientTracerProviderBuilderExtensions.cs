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
            ReplaceOrganizationServiceRegistration<IOrganizationServiceAsync2>(services);
            ReplaceOrganizationServiceRegistration<IOrganizationServiceAsync>(services);
            ReplaceOrganizationServiceRegistration<IOrganizationService>(services);
        });

        return builder;
    }

    static void ReplaceOrganizationServiceRegistration<TService>(IServiceCollection services)
        where TService : class, IOrganizationService
    {
        ServiceDescriptor? originalServiceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TService));
        if (originalServiceDescriptor == null) return;

        services.Remove(originalServiceDescriptor);

        // Add the decorator, which uses the original service description to create instances.
        var implementationFactory = (IServiceProvider serviceProvider) =>
        {
            TService originalService = GetOriginalService<TService>(originalServiceDescriptor, serviceProvider) ??
                                       throw new InvalidOperationException($"Could not create original service client instance of type {typeof(TService)}!");

            return new OpenTelemetryServiceClientDecorator(originalService) as TService ??
                   throw new InvalidOperationException($"Could not create {typeof(OpenTelemetryServiceClientDecorator)} instance!");
        };

        services.AddSingleton(implementationFactory);

        // TODO: use the original service descriptor lifetime instead of Singleton?
        // switch (originalServiceDescriptor.Lifetime)
        // {
        //     case ServiceLifetime.Singleton:
        //         services.AddSingleton(implementationFactory);
        //         break;
        //     case ServiceLifetime.Scoped:
        //         services.AddScoped(implementationFactory);
        //         break;
        //     case ServiceLifetime.Transient:
        //         services.AddTransient(implementationFactory);
        //         break;
        // }
    }

    static TService? GetOriginalService<TService>(ServiceDescriptor originalServiceDescriptor, IServiceProvider serviceProvider)
        where TService : class, IOrganizationService
    {
        if (originalServiceDescriptor.ImplementationInstance != null)
            return (TService)originalServiceDescriptor.ImplementationInstance;

        if (originalServiceDescriptor.ImplementationFactory != null)
            return (TService)originalServiceDescriptor.ImplementationFactory.Invoke(serviceProvider);

        if (originalServiceDescriptor.ImplementationType != null)
            return Activator.CreateInstance(originalServiceDescriptor.ImplementationType) as TService;

        return null;
    }
}
