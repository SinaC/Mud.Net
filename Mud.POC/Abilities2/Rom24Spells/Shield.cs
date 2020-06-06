using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Shield", AbilityEffects.Buff, PulseWaitTime = 18)]
    [AbilityCharacterWearOffMessage("Your force shield shimmers then fades away.")]
    [AbilityDispellable("The shield protecting {0:n} vanishes.")]
    public class Shield : CharacterBuffSpellBase
    {
        public Shield(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override string SelfAlreadyAffectedMessage => "You are already shielded from harm.";
        protected override string NotSelfAlreadyAffectedMessage => "{0:N} is already protected by a shield.";
        protected override string VictimAffectMessage => "You are surrounded by a force shield.";
        protected override string CasterAffectMessage => "{0:N} {0:b} surrounded by a force shield.";
        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
            => (Level, TimeSpan.FromMinutes(8 + Level),
            new[]
            {
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add }
            });
    }
}
