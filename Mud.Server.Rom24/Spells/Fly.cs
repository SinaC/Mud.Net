using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 18)]
    [AbilityCharacterWearOffMessage("You slowly float to the ground.")]
    [AbilityDispellable("{0:N} falls to the ground!")]
    public class Fly : DefensiveSpellBase
    {
        public const string SpellName = "Fly";

        private IAuraManager AuraManager { get; }

        public Fly(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Flying))
            {
                if (Victim == Caster)
                    Caster.Send("You are already airborne.");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} doesn't need your help to fly.", Victim);
                return;
            }
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level + 3), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Flying, Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0:P} feet rise off the ground.", Victim);
        }
    }
}
