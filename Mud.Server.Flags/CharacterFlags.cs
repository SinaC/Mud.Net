using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(ICharacterFlags))]
public class CharacterFlags : Flags<ICharacterFlagValues>, ICharacterFlags
{
    public CharacterFlags(ICharacterFlagValues flagValues)
        : base(flagValues)
    {
    }
}
