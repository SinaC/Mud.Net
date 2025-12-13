using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory)), Shared]
public class FlagsFactory : IFlagFactory
{
    private IServiceProvider ServiceProvider { get; }

    public FlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public TFlag CreateInstance<TFlag, TFlagValues>(string? flags)
        where TFlag : IFlags<string, TFlagValues>
        where TFlagValues : IFlagValues<string>
    {
        var instance = ServiceProvider.GetRequiredService<TFlag>();
        if (flags != null)
            instance.Set(flags);
        return instance;
    }


    public TFlag CreateInstance<TFlag, TFlagValues>(params string[] flags)
        where TFlag : IFlags<string, TFlagValues>
        where TFlagValues : IFlagValues<string>
    {
        var instance = ServiceProvider.GetRequiredService<TFlag>();
        instance.Set(flags);
        return instance;
    }
}
