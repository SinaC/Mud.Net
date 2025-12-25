using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mud.Common.Attributes;

public static class ExportInspector
{
    public static void Register(IServiceCollection services, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.CustomAttributes.Any())
        {
            return;
        }

        var exports = type.GetCustomAttributes<ExportAttribute>().ToArray();

        if (exports.Length == 0)
        {
            return;
        }

        var isSingleton = type.CustomAttributes.Any(a => a.AttributeType == typeof(SharedAttribute));

        if (isSingleton && exports.Length > 1)
        {
            if (exports.Any(x => x.ContractType is null || x.ContractName is not null))
                throw new InvalidOperationException($"Exported type {type.FullName} is declared as Shared with multiple exports. ContractType is mandatory and ContractName is forbidden for each Export");

            services.AddSingleton(type);
            foreach (var export in exports)
            {
                services.AddSingleton(export.ContractType!, x => x.GetRequiredService(type));
            }
        }
        else
        {
            foreach (var export in exports)
            {
                var fromType = export.ContractType ?? type;
                var contractName = export.ContractName;

                if (!fromType.IsAssignableFrom(type))
                    throw new InvalidOperationException($"Exported type {type.FullName} does not implement the contract type {fromType.FullName}.");

                if (isSingleton)
                {
                    if (string.IsNullOrWhiteSpace(contractName))
                    {
                        services.AddSingleton(fromType, type);
                    }
                    else
                    {
                        services.AddKeyedSingleton(fromType, contractName!, type);
                    }
                }
                else if (string.IsNullOrWhiteSpace(contractName))
                {
                    services.AddTransient(fromType, type);
                }
                else
                {
                    services.AddKeyedTransient(fromType, contractName!, type);
                }
            }
        }
    }
}
