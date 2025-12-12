using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Special;
using System.Reflection;

namespace Mud.Server.Specials
{
    [Export(typeof(ISpecialBehaviorManager)), Shared]
    public class SpecialBehaviorManager : ISpecialBehaviorManager
    {
        private ILogger<SpecialBehaviorManager> Logger { get; }
        private IServiceProvider ServiceProvider { get; }

        private Dictionary<string, Type> SpecialBehaviorsByName { get; }

        public SpecialBehaviorManager(ILogger<SpecialBehaviorManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;

            var iSpecialBehaviorType = typeof(ISpecialBehavior);
            SpecialBehaviorsByName = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iSpecialBehaviorType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attribute = t.GetCustomAttribute<SpecialBehaviorAttribute>()! })
            .Where(x => x.attribute != null)
            .ToDictionary(x => x.attribute.Name, x => x.executionType);
        }

        public IReadOnlyCollection<string> Specials => SpecialBehaviorsByName.Keys.ToArray();

        public ISpecialBehavior? CreateInstance(string name)
        {
            if (!SpecialBehaviorsByName.TryGetValue(name, out var specialBehaviorType))
            {
                Logger.LogError("SpecialBehaviorManager: special behavior {name} not found.", name);
                return null;
            }

            var special = ServiceProvider.GetRequiredService(specialBehaviorType);
            if (special is not ISpecialBehavior instance)
            {
                Logger.LogError("SpecialBehaviorManager: special behavior {specialBehaviorType} cannot be created or is not of type {expectedSpecialBehaviorType}", specialBehaviorType.FullName ?? "???", typeof(ISpecialBehavior).FullName ?? "???");
                return null;
            }
            return instance;
        }
    }
}
