using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IWeaponFlags, IWeaponFlagValues>)), Shared]
public class WeaponFlagsFactory : IFlagFactory<IWeaponFlags, IWeaponFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public WeaponFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IWeaponFlags CreateInstance(string flags)
    {
        var WeaponFlags = ServiceProvider.GetRequiredService<IWeaponFlags>();
        WeaponFlags.Set(flags);
        return WeaponFlags;
    }

    public IWeaponFlags CreateInstance(params string[] flags)
    {
        var WeaponFlags = ServiceProvider.GetRequiredService<IWeaponFlags>();
        WeaponFlags.Set(flags);
        return WeaponFlags;
    }
}
