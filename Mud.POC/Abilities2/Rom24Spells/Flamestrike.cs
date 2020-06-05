using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Flamestrike", AbilityEffects.Damage)]
    public class Flamestrike : DamageSpellBase
    {
        public Flamestrike(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override SchoolTypes DamageType => SchoolTypes.Fire;
        protected override int DamageValue => RandomManager.Dice(6 + Level / 2, 8);
        protected override string DamageNoun => "flamestrike";
    }
}
