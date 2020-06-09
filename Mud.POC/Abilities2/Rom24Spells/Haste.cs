﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel yourself slow down.")]
    [AbilityDispellable("{0:N} is no longer moving so quickly.")]
    public class Haste : DefensiveSpellBase
    {
        public const string SpellName = "Haste";

        private IAuraManager AuraManager { get; }
        private IDispelManager DispelManager { get; }

        public Haste(IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
            DispelManager = dispelManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Haste)
                || Victim.GetAura(SpellName) != null
                || (Victim is INonPlayableCharacter npcVictim && npcVictim.OffensiveFlags.HasFlag(OffensiveFlags.Fast)))
            {
                if (Victim == Caster)
                    Caster.Send("You can't move any faster!");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} is already moving as fast as {0:e} can.", Victim);
                return;
            }
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Slow))
            {
                if (DispelManager.TryDispel(Level, Victim, Slow.SpellName) != TryDispelReturnValues.Dispelled)
                {
                    if (Victim != Caster)
                        Caster.Send("Spell failed.");
                    Victim.Send("You feel momentarily faster.");
                    return;
                }
                Victim.Act(ActOptions.ToRoom, "{0:N} is moving less slowly.", Victim);
                return;
            }
            int duration = Victim == Caster
                ? Level / 2
                : Level / 4;
            int modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Haste, Operator = AffectOperators.Or });
            Victim.Send("You feel yourself moving more quickly.");
            Victim.Act(ActOptions.ToRoom, "{0:N} is moving more quickly.", Victim);
            if (Caster != Victim)
                Caster.Send("Ok.");
        }
    }
}