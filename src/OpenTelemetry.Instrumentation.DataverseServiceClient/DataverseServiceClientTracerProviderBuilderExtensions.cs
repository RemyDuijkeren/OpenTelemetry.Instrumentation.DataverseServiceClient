using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.DataverseServiceClient;

public static class DataverseServiceClientTracerProviderBuilderExtensions
{
    /// <summary>Adds Dataverse Service Client instrumentation to the <see cref="TracerProviderBuilder"/>.</summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to add the instrumentation to.</param>
    /// <returns>The supplied <see cref="TracerProviderBuilder"/> for chaining.</returns>
    /// <remarks>
    /// This method adds Dataverse <see cref="ActivitySource"/> to the <see cref="TracerProviderBuilder"/> and
    /// replaces any registered <see cref="IOrganizationService"/>, <see cref="IOrganizationServiceAsync"/> or
    /// <see cref="IOrganizationServiceAsync2"/> by decorating it with the <see cref="OpenTelemetryServiceClientDecorator"/>.
    /// </remarks>
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

    /// <summary>Replaces the registration of the organization service with the specified type, by decorating it with the <see cref="OpenTelemetryServiceClientDecorator"/>.</summary>
    /// <typeparam name="TService">The type of service to replace which at least implements <see cref="IOrganizationService"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <remarks>
    /// This method removes the original registration of the specified service type, and adds a new registration that decorates the original service with the
    /// <see cref="OpenTelemetryServiceClientDecorator"/>. The original registration must be present in the service collection.
    /// The lifetime of the new registration matches the lifetime of the original registration.
    /// </remarks>
    static void ReplaceOrganizationServiceRegistration<TService>(IServiceCollection services)
        where TService : class, IOrganizationService
    {
        ServiceDescriptor? originalServiceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TService));
        if (originalServiceDescriptor == null) return;

        services.Remove(originalServiceDescriptor);

        switch (originalServiceDescriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton(DecoratorImplementationFactory<TService>(originalServiceDescriptor));
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(DecoratorImplementationFactory<TService>(originalServiceDescriptor));
                break;
            case ServiceLifetime.Transient:
                services.AddTransient(DecoratorImplementationFactory<TService>(originalServiceDescriptor));
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unexpected ServiceLifetime {originalServiceDescriptor.Lifetime}");
        }
    }

    /// <summary>Factory method implementation that returns the given <typeparamref name="TService"/>, by decoration it with the <see cref="OpenTelemetryServiceClientDecorator"/>.</summary>
    /// <typeparam name="TService">The type of service for which the decorator implementation is created which at least implements <see cref="IOrganizationService"/>.</typeparam>
    /// <param name="originalServiceDescriptor">The original service descriptor of <typeparamref name="TService"/>.</param>
    /// <returns>A factory delegate that creates the decorator implementation that can be used to add services to <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <paramref name="originalServiceDescriptor"/> is null.</exception>
    static Func<IServiceProvider, TService> DecoratorImplementationFactory<TService>(ServiceDescriptor originalServiceDescriptor)
        where TService : class, IOrganizationService => serviceProvider =>
    {
        TService originalService = GetOriginalService<TService>(originalServiceDescriptor, serviceProvider) ??
                                   throw new InvalidOperationException($"Could not create original service client instance of type {typeof(TService)}!");

        return new OpenTelemetryServiceClientDecorator(originalService) as TService ??
               throw new InvalidOperationException($"Could not create {typeof(OpenTelemetryServiceClientDecorator)} instance decorating original service client instance of type {typeof(TService)}!");
    };

    /// <summary>Gets the original service client instance of type <typeparamref name="TService"/> from the provided service descriptor and service provider.</summary>
    /// <typeparam name="TService">The type of the original service client which at least implements <see cref="IOrganizationService"/>.</typeparam>
    /// <param name="originalServiceDescriptor">The service descriptor containing information about creating the original service client.</param>
    /// <param name="serviceProvider">The service provider used to instantiate the original service client.</param>
    /// <returns>The original service client instance of type <typeparamref name="TService"/>, or null if it could not be created.</returns>
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
