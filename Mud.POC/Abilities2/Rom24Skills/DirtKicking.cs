﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using Mud.Server.GameAction;
using System;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("dirt", "Abilities", "Skills", "Combat")]
    [Skill(SkillName, AbilityEffects.Damage | AbilityEffects.Debuff, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
    [AbilityCharacterWearOffMessage("You rub the dirt out of your eyes.")]
    public class DirtKicking : OffensiveSkillBase
    {
        public const string SkillName = "Dirt Kicking";

        private IAuraManager AuraManager { get; }

        public DirtKicking(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            INonPlayableCharacter npcUser = User as INonPlayableCharacter;
            if (Learned == 0
                || (npcUser != null && !npcUser.OffensiveFlags.HasFlag(OffensiveFlags.DirtKick)))
                return "You get your feet dirty.";

            if (Victim == User)
                return "Very funny.";

            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Blind))
                return User.ActPhrase("{0:e}'s already been blinded.", Victim);

            if (Victim.IsSafe(User))
                return "Not on that victim.";

            // TODO: check kill stealing

            if (User.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcUser?.Master == Victim)
                return User.ActPhrase("But {0:N} is your friend!", Victim);

            return null;
        }

        protected override bool Invoke()
        {
            int chance = Learned;
            // modifiers
            // dexterity
            chance += User[CharacterAttributes.Dexterity];
            chance -= 2 * Victim[CharacterAttributes.Dexterity];
            // speed
            if ((User as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || User.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((Victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || Victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 25;
            // level
            chance += (User.Level - Victim.Level) * 2;
            // sloppy hack to prevent false zeroes
            if (chance % 5 == 0)
                chance += 1;
            // terrain
            switch (User.Room.SectorType)
            {
                case SectorTypes.Inside: chance -= 20; break;
                case SectorTypes.City: chance -= 10; break;
                case SectorTypes.Field: chance += 5; break;
                case SectorTypes.Forest: break;
                case SectorTypes.Hills: break;
                case SectorTypes.Mountain: chance -= 10; break;
                case SectorTypes.WaterSwim: chance = 0; break;
                case SectorTypes.WaterNoSwim: chance = 0; break;
                case SectorTypes.Burning: chance = 0; break;
                case SectorTypes.Air: chance = 0; break;
                case SectorTypes.Desert: chance += 10; break;
                case SectorTypes.Underwater: chance = 0; break;
                default: chance = 0; break;
            }
            //
            if (chance == 0)
            {
                User.Send("There isn't any dirt to kick.");
                return false;
            }
            // now the attack
            if (RandomManager.Chance(chance))
            {
                Victim.Act(ActOptions.ToRoom, "{0:N} is blinded by the dirt in {0:s} eyes!", Victim);
                Victim.Act(ActOptions.ToCharacter, "{0:N} kicks dirt in your eyes!", User);
                Victim.Send("You can't see a thing!");

                int damage = RandomManager.Range(2, 5);
                Victim.AbilityDamage(User, damage, SchoolTypes.None, "kicked dirt", false);
                // TODO check killer

                AuraManager.AddAura(Victim, SkillName, User, User.Level, TimeSpan.FromSeconds(1)/*originally 0*/, AuraFlags.NoDispel, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Or });
                return true;
            }
            else
            {
                Victim.AbilityDamage(User, 0, SchoolTypes.None, "kicked dirt", true);
                // TODO: check_killer(ch,Victim);
                return false;
            }
        }
    }
}
