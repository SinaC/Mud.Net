using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Affects;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;
using System;

namespace Mud.Server.Rom24.Skills
{
    [CharacterCommand("berserk", "Ability", "Skill", "Combat")]
    [Skill(SkillName, AbilityEffects.Buff, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
    [AbilityCharacterWearOffMessage("You feel your pulse slow down.")]
    public class Berserk : NoTargetSkillBase
    {
        public const string SkillName = "Berserk";

        private IAuraManager AuraManager { get; }

        public Berserk(IRandomManager randomManager, IAuraManager auraManager) 
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (Learned == 0
                || (User is INonPlayableCharacter npcUser && !npcUser.OffensiveFlags.HasFlag(OffensiveFlags.Berserk)))
                return "You turn red in the face, but nothing happens.";

            if (User.CharacterFlags.HasFlag(CharacterFlags.Berserk)
                || User.GetAura(SkillName) != null
                || User.GetAura(Frenzy.SpellName) != null)
                return "You get a little madder.";

            if (User.CharacterFlags.HasFlag(CharacterFlags.Calm))
                return "You're feeling to mellow to berserk.";

            if (User[ResourceKinds.Mana] < 50)
                return "You can't get up enough energy.";

            return null;
        }

        protected override bool Invoke()
        {
            int chance = Learned;

            // modifiers
            if (User.Fighting != null)
                chance += 10;

            // Below 50%, hp helps, above hurts
            int hpPercent = (100 * User.HitPoints) / User.MaxHitPoints;
            chance += 25 - hpPercent / 2;

            //
            if (RandomManager.Chance(chance))
            {
                User.UpdateResource(ResourceKinds.Mana, 50);
                User.UpdateMovePoints(User.MovePoints / 2);
                User.UpdateHitPoints(User.Level * 2);

                User.Send("Your pulse races as you are consumed by rage!");
                User.Act(ActOptions.ToRoom, "{0:N} gets a wild look in {0:s} eyes.", User);

                int duration = RandomManager.Fuzzy(User.Level / 8);
                int modifier = Math.Max(1, User.Level / 5);
                int acModifier = Math.Max(10, 10 * (User.Level / 5));
                AuraManager.AddAura(User, SkillName, User, User.Level, TimeSpan.FromMinutes(duration), AuraFlags.NoDispel, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Berserk, Operator = AffectOperators.Or },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = acModifier, Operator = AffectOperators.Add });
                return true;
            }
            else
            {
                User.UpdateResource(ResourceKinds.Mana, 25);
                User.UpdateMovePoints(User.MovePoints / 2);

                User.Send("Your pulse speeds up, but nothing happens.");

                return false;
            }
        }
    }
}
