using Microsoft.EntityFrameworkCore;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Converters;
using System.Reflection;

namespace Hector.BuildingBlocks.Persistence.Extensions;

public static class ModelConfigurationBuilderExtensions
{
    public static ModelConfigurationBuilder RegisterStronglyTypedIdConventions(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assembliesToScan)
    {
        if (assembliesToScan.Length == 0)
        {
            assembliesToScan = new[] { typeof(IStronglyTypedId).Assembly };
        }

        foreach (var assembly in assembliesToScan)
        {
            var stronglyTypedIdTypes = assembly.GetTypes()
                .Where(t => typeof(IStronglyTypedId).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var idType in stronglyTypedIdTypes)
            {
                var converterType = typeof(StronglyTypedIdEfValueConverter<>).MakeGenericType(idType);

                configurationBuilder
                    .Properties(idType)
                    .HaveConversion(converterType);
            }
        }

        return configurationBuilder;
    }
}
