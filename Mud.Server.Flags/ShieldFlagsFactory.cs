using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IShieldFlags, IShieldFlagValues>)), Shared]
public class ShieldFlagsFactory : IFlagFactory<IShieldFlags, IShieldFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public ShieldFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IShieldFlags CreateInstance(string flags)
    {
        var ShieldFlags = ServiceProvider.GetRequiredService<IShieldFlags>();
        ShieldFlags.Set(flags);
        return ShieldFlags;
    }

    public IShieldFlags CreateInstance(params string[] flags)
    {
        var ShieldFlags = ServiceProvider.GetRequiredService<IShieldFlags>();
        ShieldFlags.Set(flags);
        return ShieldFlags;
    }
}
