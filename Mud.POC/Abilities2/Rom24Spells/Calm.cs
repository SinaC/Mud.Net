using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class Calm : SpellBase, IAbilityCharacterBuff, IAbilityDispellable
    {
        public override int Id => 7;

        public override string Name => "Calm";

        public override AbilityEffects Effects => AbilityEffects.Debuff;

        public string CharacterWearOffMessage => "You have lost your peace of mind.";

        public string DispelRoomMessage => "{0:N} no longer looks so peaceful...";

        private IAuraManager AuraManager { get; }

        public Calm(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            // Stops all fighting in the room

            // Sum/Max/Count of fighting people in room
            int count = 0;
            int maxLevel = 0;
            int sumLevel = 0;
            foreach (ICharacter character in caster.Room.People.Where(x => x.Fighting != null))
            {
                count++;
                if (character is INonPlayableCharacter)
                    sumLevel += character.Level;
                else
                    sumLevel += character.Level / 2;
                maxLevel = Math.Max(maxLevel, character.Level);
            }

            // Compute chance of stopping combat
            int chance = 4 * level - maxLevel + 2 * count;
            // Always works if immortal
            if (caster is IPlayableCharacter pcCaster && pcCaster.IsImmortal)
                sumLevel = 0;
            // Harder to stop large fights
            if (RandomManager.Range(0, chance) < sumLevel)
                return;
            //
            foreach (ICharacter victim in caster.Room.People)
            {
                INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;

                // IsNpc, immune magic or undead
                if (npcVictim != null && (npcVictim.Immunities.HasFlag(IRVFlags.Magic) || npcVictim.ActFlags.HasFlag(ActFlags.Undead)))
                    continue;

                // Is affected by berserk, calm or frenzy
                if (victim.CharacterFlags.HasFlag(CharacterFlags.Berserk) || victim.CharacterFlags.HasFlag(CharacterFlags.Calm) || victim.GetAura("Frenzy") != null)
                    continue;

                victim.Send("A wave of calm passes over you.");

                if (victim.Fighting != null && victim.Position == Positions.Fighting)
                    victim.StopFighting(false);

                int modifier = npcVictim != null
                    ? -5
                    : -2;
                int duration = level / 4;
                AuraManager.AddAura(victim, this, caster, level, TimeSpan.FromHours(duration), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add, },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add, },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Calm, Operator = AffectOperators.Or });
            }
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters) => AbilityTargetResults.Ok;
    }
}
