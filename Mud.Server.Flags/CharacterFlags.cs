using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class CharacterFlags : Flags<ICharacterFlagValues>, ICharacterFlags
    {
        public CharacterFlags()
            : base()
        {
        }

        public CharacterFlags(string flags)
            : base(flags)
        {
        }

        public CharacterFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
