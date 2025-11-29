using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IBodyParts, IBodyPartValues>)), Shared]
public class BodyPartsFactory : IFlagFactory<IBodyParts, IBodyPartValues>
{
    private IServiceProvider ServiceProvider { get; }

    public BodyPartsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IBodyParts CreateInstance(string flags)
    {
        var bodyPartFlags = ServiceProvider.GetRequiredService<IBodyParts>();
        bodyPartFlags.Set(flags);
        return bodyPartFlags;
    }

    public IBodyParts CreateInstance(params string[] flags)
    {
        var bodyPartFlags = ServiceProvider.GetRequiredService<IBodyParts>();
        bodyPartFlags.Set(flags);
        return bodyPartFlags;
    }
}
