using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IOffensiveFlags, IOffensiveFlagValues>)), Shared]
public class OffensiveFlagsFactory : IFlagFactory<IOffensiveFlags, IOffensiveFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public OffensiveFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IOffensiveFlags CreateInstance(string? flags)
    {
        var offensiveFlags = ServiceProvider.GetRequiredService<IOffensiveFlags>();
        if (flags != null)
            offensiveFlags.Set(flags);
        return offensiveFlags;
    }

    public IOffensiveFlags CreateInstance(params string[] flags)
    {
        var OffensiveFlags = ServiceProvider.GetRequiredService<IOffensiveFlags>();
        OffensiveFlags.Set(flags);
        return OffensiveFlags;
    }
}
