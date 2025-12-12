using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IRoomFlags, IRoomFlagValues>)), Shared]
public class RoomFlagsFactory : IFlagFactory<IRoomFlags, IRoomFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public RoomFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IRoomFlags CreateInstance(string? flags)
    {
        var roomFlags = ServiceProvider.GetRequiredService<IRoomFlags>();
        if (flags != null)
            roomFlags.Set(flags);
        return roomFlags;
    }

    public IRoomFlags CreateInstance(params string[] flags)
    {
        var RoomFlags = ServiceProvider.GetRequiredService<IRoomFlags>();
        RoomFlags.Set(flags);
        return RoomFlags;
    }
}
