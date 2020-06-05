﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Fly", AbilityEffects.Buff, PulseWaitTime = 18)]
    [AbilityCharacterWearOffMessage("You slowly float to the ground.")]
    [AbilityDispellable("{0:N} falls to the ground!")]
    public class Fly : DefensiveSpellBase
    {
        private IAuraManager AuraManager { get; }

        public Fly(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
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
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(Level + 3), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Flying, Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToAll, "{0:P} feet rise off the ground.", Victim);
        }
    }
}
