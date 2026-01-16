using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class CharacterFlags : DataStructures.Flags.Flags, ICharacterFlags
{
    public CharacterFlags(params string[] flags)
        : base(flags)
    {
    }
}
