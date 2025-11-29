using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IIRVFlags, IIRVFlagValues>)), Shared]
public class IRVFlagsFactory : IFlagFactory<IIRVFlags, IIRVFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public IRVFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IIRVFlags CreateInstance(string flags)
    {
        var irvFlags = ServiceProvider.GetRequiredService<IIRVFlags>();
        irvFlags.Set(flags);
        return irvFlags;
    }

    public IIRVFlags CreateInstance(params string[] flags)
    {
        var irvFlags = ServiceProvider.GetRequiredService<IIRVFlags>();
        irvFlags.Set(flags);
        return irvFlags;
    }
}
