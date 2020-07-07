using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Passive
{
    public abstract class HitAvoidancePassiveBase : PassiveBase, IHitAvoidancePassive
    {
        protected abstract string AvoiderPhrase { get; }
        protected abstract string AggressorPhrase { get; }

        protected HitAvoidancePassiveBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public bool Avoid(ICharacter avoider, ICharacter aggressor, SchoolTypes damageType)
        {
            bool isTriggered = IsTriggered(avoider, aggressor, true, out _, out _);
            if (isTriggered)
            {
                avoider.Act(ActOptions.ToCharacter, AvoiderPhrase, aggressor);
                aggressor.Act(ActOptions.ToCharacter, AggressorPhrase, avoider);
                if (aggressor.Fighting == null)
                    avoider.AbilityDamage(aggressor, 0, damageType, "hit", false); // starts fight, remove sneak/invis, ...

            }
            return isTriggered;
        }
    }
}
