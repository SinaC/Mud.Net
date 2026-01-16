using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class CharacterFlags : DataStructures.Flags.Flags, ICharacterFlags
{
    public CharacterFlags(params string[] flags)
        : base(flags)
    {
    }
}
