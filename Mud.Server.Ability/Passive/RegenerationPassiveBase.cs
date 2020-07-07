using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Passive
{
    public abstract class RegenerationPassiveBase : PassiveBase, IRegenerationPassive
    {
        protected RegenerationPassiveBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public virtual int HitGainModifier(ICharacter user, int baseHitGain) => 0;

        public virtual int ResourceGainModifier(ICharacter user, ResourceKinds resourceKind, int baseResourceGain) => 0;
    }
}
