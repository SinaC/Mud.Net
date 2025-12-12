using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IActFlags, IActFlagValues>)), Shared]
public class ActFlagsFactory : IFlagFactory<IActFlags, IActFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public ActFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IActFlags CreateInstance(string? flags)
    {
        var actFlags = ServiceProvider.GetRequiredService<IActFlags>();
        if (flags != null)
            actFlags.Set(flags);
        return actFlags;
    }

    public IActFlags CreateInstance(params string[] flags)
    {
        var actFlags = ServiceProvider.GetRequiredService<IActFlags>();
        actFlags.Set(flags);
        return actFlags;
    }
}
