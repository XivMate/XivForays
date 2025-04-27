using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace XivMate.DataGathering.Forays.Dalamud.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllTypesImplementing<TInterface>(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() }; // Default to calling assembly
        }

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                                .Where(t => typeof(TInterface).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in types)
            {
                services.AddScoped(typeof(TInterface), type); // Or AddSingleton, AddTransient
            }
        }

        return services;
    }

    public static IServiceCollection AddAllTypesImplementing<TInterface>(this IServiceCollection services)
    {
        return services.AddAllTypesImplementing<TInterface>(null);
    }
}
