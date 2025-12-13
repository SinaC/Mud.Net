using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<IItemFlags, IItemFlagValues>)), Shared]
public class ItemFlagsFactory : IFlagFactory<IItemFlags, IItemFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public ItemFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IItemFlags CreateInstance(string? flags)
    {
        var itemFlags = ServiceProvider.GetRequiredService<IItemFlags>();
        if (flags != null)
            itemFlags.Set(flags);
        return itemFlags;
    }

    public IItemFlags CreateInstance(params string[] flags)
    {
        var ItemFlags = ServiceProvider.GetRequiredService<IItemFlags>();
        ItemFlags.Set(flags);
        return ItemFlags;
    }
}
