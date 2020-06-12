using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Affects;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24.Rom24Skills
{
    [Command("envenom", "Abilities", "Skills", "Enchantments")]
    [Skill(SkillName, AbilityEffects.Enchantment, PulseWaitTime = 36, LearnDifficultyMultiplier = 4)]
    [AbilityItemWearOffMessage("The poison on {0} dries up.")]
    public class Envenom : ItemInventorySkillBase
    {
        public const string SkillName = "Envenom";

        private IAuraManager AuraManager { get; }

        public Envenom(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (Learned < 1)
                return "Are you crazy? You'd poison yourself!";

            var poisonable = Item as IItemPoisonable;
            if (poisonable != null)
            {
                if (poisonable.ItemFlags.HasFlag(ItemFlags.Bless) || poisonable.ItemFlags.HasFlag(ItemFlags.BurnProof))
                    return User.ActPhrase("You fail to poison {0}.", poisonable);
                // poisonable found
                return null;
            }
            var weapon = Item as IItemWeapon;
            if (weapon != null)
            {
                if (weapon.DamageType == SchoolTypes.Bash)
                    return "You can only envenom edged weapons.";
                if (weapon.WeaponFlags == WeaponFlags.Poison)
                    User.ActPhrase("{0} is already envenomed.", weapon);
                if (weapon.WeaponFlags != WeaponFlags.None
                    || weapon.ItemFlags.HasFlag(ItemFlags.Bless)
                    || weapon.ItemFlags.HasFlag(ItemFlags.BurnProof))
                    User.ActPhrase("You can't seem to envenom {0}.", weapon);
                // weapon found
                return null;
            }

            return User.ActPhrase("You can't poison {0}.", Item);
        }

        protected override bool Invoke()
        {
            if (Item is IItemPoisonable poisonable)
                return Invoke(poisonable);
            if (Item is IItemWeapon weapon)
                return Invoke(weapon);
            return false;
        }

        private bool Invoke(IItemPoisonable poisonable)
        {
            if (RandomManager.Chance(Learned))
            {
                User.Act(ActOptions.ToAll, "{0:N} treats {1} with deadly poison.", User, poisonable);
                poisonable.Poison();
                poisonable.Recompute();
                return true;
            }
            User.Act(ActOptions.ToCharacter, "You fail to poison {0}.", poisonable);
            return false;
        }

        private bool Invoke(IItemWeapon weapon)
        {
            int percent = RandomManager.Range(1, 100);
            if (RandomManager.Chance(percent))
            {
                int level = (User.Level * percent) / 100;
                int duration = (User.Level * percent) / (2 * 100);
                AuraManager.AddAura(weapon, SkillName, User, level, TimeSpan.FromMinutes(duration), AuraFlags.NoDispel, true,
                    new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Poison, Operator = AffectOperators.Or });
                User.Act(ActOptions.ToAll, "{0:N} coat{0:v} {1} with deadly venom.", User, weapon);
                return true;
            }
            User.Act(ActOptions.ToCharacter, "You fail to envenom {0}.", weapon);
            return false;
        }
    }
}
