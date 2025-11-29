using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IAssistFlags, IAssistFlagValues>)), Shared]
public class AssistFlagsFactory : IFlagFactory<IAssistFlags, IAssistFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public AssistFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IAssistFlags CreateInstance(string flags)
    {
        var assistFlags = ServiceProvider.GetRequiredService<IAssistFlags>();
        assistFlags.Set(flags);
        return assistFlags;
    }

    public IAssistFlags CreateInstance(params string[] flags)
    {
        var assistFlags = ServiceProvider.GetRequiredService<IAssistFlags>();
        assistFlags.Set(flags);
        return assistFlags;
    }
}
