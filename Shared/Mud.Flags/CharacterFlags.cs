using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class CharacterFlags(params string[] flags) : DataStructures.Flags.Flags(flags), ICharacterFlags
{
}
