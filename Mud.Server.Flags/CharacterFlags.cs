using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class CharacterFlags : Flags<ICharacterFlagValues>, ICharacterFlags
    {
        public CharacterFlags(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public CharacterFlags(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public CharacterFlags(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }
    }
}
