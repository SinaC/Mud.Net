﻿using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You have lost your peace of mind.")]
    [AbilityDispellable("{0:N} no longer looks so peaceful...")]
    public class Calm : NoTargetSpellBase
    {
        public const string SpellName = "Calm";

        private IAuraManager AuraManager { get; }

        public Calm(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            // Stops all fighting in the room

            // Sum/Max/Count of fighting people in room
            int count = 0;
            int maxLevel = 0;
            int sumLevel = 0;
            foreach (ICharacter character in Caster.Room.People.Where(x => x.Fighting != null))
            {
                count++;
                if (character is INonPlayableCharacter)
                    sumLevel += character.Level;
                else
                    sumLevel += character.Level / 2;
                maxLevel = Math.Max(maxLevel, character.Level);
            }

            // Compute chance of stopping combat
            int chance = 4 * Level - maxLevel + 2 * count;
            // Always works if immortal
            if (Caster is IPlayableCharacter pcCaster && pcCaster.IsImmortal)
                sumLevel = 0;
            // Harder to stop large fights
            if (RandomManager.Range(0, chance) < sumLevel)
                return;
            //
            foreach (ICharacter victim in Caster.Room.People)
            {
                INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;

                // IsNpc, immune magic or undead
                if (npcVictim != null && (npcVictim.Immunities.IsSet("Magic") || npcVictim.ActFlags.IsSet("Undead")))
                    continue;

                // Is affected by berserk, calm or frenzy
                if (victim.CharacterFlags.IsSet("Berserk") || victim.CharacterFlags.IsSet("Calm") || victim.GetAura("Frenzy") != null)
                    continue;

                victim.Send("A wave of calm passes over you.");

                if (victim.Fighting != null)
                    victim.StopFighting(false);

                int modifier = npcVictim != null
                    ? -5
                    : -2;
                int duration = Level / 4;
                AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add, },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add, },
                    new CharacterFlagsAffect { Modifier = new CharacterFlags("Calm"), Operator = AffectOperators.Or });
            }
        }
    }
}
