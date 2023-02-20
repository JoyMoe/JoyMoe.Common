using FluentValidation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidator<TValidator>(
        this IServiceCollection services,
        ServiceLifetime         lifetime = ServiceLifetime.Scoped)
        where TValidator : class {
        var implementationType = typeof(TValidator);
        var validatorType = implementationType.GetInterfaces().FirstOrDefault(t =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IValidator<>));

        if (validatorType == null) {
            throw new AggregateException(implementationType.Name + "is not implement with IValidator<>.");
        }

        var messageType = validatorType.GetGenericArguments().First();
        var serviceType = typeof(IValidator<>).MakeGenericType(messageType);

        services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        return services;
    }
}
