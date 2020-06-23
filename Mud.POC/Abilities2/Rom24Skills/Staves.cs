using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.GameAction;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("brandish", "Ability", "Skill")]
    [Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
    public class Staves : ItemCastSpellSkillBase<IItemStaff>
    {
        public const string SkillName = "Staves";

        public Staves(IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
            : base(randomManager, abilityManager, itemManager)
        {
        }

        protected override bool Invoke()
        {
            bool success;
            User.Act(ActOptions.ToAll, "{0:N} brandish{0:v} {1}.", User, Item);
            int chance = 20 + (4 * Learned) / 5;
            if (User.Level < Item.Level
                || !RandomManager.Chance(chance))
            {
                User.Act(ActOptions.ToCharacter, "You fail to invoke {0}.", Item);
                User.Act(ActOptions.ToRoom, "...and nothing happens.");
                success = false;
            }
            else
            {
                CastSpells();
                success = true;
            }
            Item.Use();

            if (Item.CurrentChargeCount <= 0)
            {
                User.Act(ActOptions.ToAll, "{0:P} {1} blazes bright and is gone.", User, Item);
                ItemManager.RemoveItem(Item);
            }

            return success;
        }

        protected override string SetTargets(ISkillActionInput skillActionInput)
        {
            Item = User.GetEquipment<IItemStaff>(EquipmentSlots.OffHand);
            if (Item == null)
                return "You can brandish only with a staff.";
            if (Item.CurrentChargeCount == 0)
            {
                User.Act(ActOptions.ToAll, "{0:P} {1} blazes bright and is gone.", User, Item);
                ItemManager.RemoveItem(Item);
                return string.Empty; // stop but don't display anything
            }
            return SetupSpellForEachAvailableTargets(Item.SpellName, Item.SpellLevel, skillActionInput.RawParameters, skillActionInput.Parameters);
        }
    }
}
