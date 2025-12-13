using Microsoft.Extensions.DependencyInjection;
using Mud.Common.Attributes;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IFlagFactory<ICharacterFlags, ICharacterFlagValues>)), Shared]
public class CharacterFlagsFactory : IFlagFactory<ICharacterFlags, ICharacterFlagValues>
{
    private IServiceProvider ServiceProvider { get; }

    public CharacterFlagsFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public ICharacterFlags CreateInstance(string? flags)
    {
        var characterFlags = ServiceProvider.GetRequiredService<ICharacterFlags>();
        if (flags != null)
            characterFlags.Set(flags);
        return characterFlags;
    }

    public ICharacterFlags CreateInstance(params string[] flags)
    {
        var characterFlags = ServiceProvider.GetRequiredService<ICharacterFlags>();
        characterFlags.Set(flags);
        return characterFlags;
    }
}
