using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You can see again.")]
    [AbilityDispellable("{0:N} is no longer blinded.")]
    public class Blindness : DefensiveSpellBase
    {
        public const string SpellName = "Blindness";

        private IAuraManager AuraManager { get; }

        public Blindness(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            BlindnessEffect effect = new BlindnessEffect(AuraManager);
            effect.Apply(Victim, Caster, SpellName, Level, 0);
        }
    }
}
