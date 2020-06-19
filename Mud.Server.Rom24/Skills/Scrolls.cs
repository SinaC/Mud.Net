using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System.Linq;
using System.Text;

namespace Mud.Server.Rom24.Skills
{
    [Command("recite", "Abilities", "Skills")]
    [Syntax("[cmd] <scroll> [<target>]")]
    [Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
    public class Scrolls : ItemCastSpellSkillBase<IItemScroll>
    {
        public const string SkillName = "Scrolls";

        public Scrolls(IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
            : base(randomManager, abilityManager, itemManager)
        {
        }

        protected override bool Invoke()
        {
            User.Act(ActOptions.ToAll, "{0:N} recite{0:v} {1}.", User, Item);

            int chance = 20 + (4 * Learned) / 5;
            if (!RandomManager.Chance(chance))
            {
                User.Send("You mispronounce a syllable.");
                ItemManager.RemoveItem(Item);
                return false;
            }

            CastSpells();

            ItemManager.RemoveItem(Item);
            return true;
        }

        protected override string SetTargets(ISkillActionInput skillActionInput)
        {
            if (skillActionInput.Parameters.Length == 0)
                return "Recite what?";

            IItem item = FindHelpers.FindByName(User.Inventory.Where(User.CanSee), skillActionInput.Parameters[0]);
            if (item == null)
                return "You do not have that scroll.";

            Item = item as IItemScroll;
            if (Item == null)
                return "You can recite only scrolls.";

            if (User.Level < Item.Level)
                return "This scroll is too complex for you to comprehend.";

            // scroll found, remove it from parameters
            var parameters = CommandHelpers.SkipParameters(skillActionInput.Parameters, 1);

            // perform setup on each spell
            StringBuilder sb = new StringBuilder();
            string result;
            if (!string.IsNullOrWhiteSpace(Item.FirstSpellName))
            {
                result = SetupSpell(Item.FirstSpellName, Item.Level, parameters.rawParameters, parameters.parameters);
                if (result != null)
                    sb.AppendFormatAndLineIfNotEmpty(result);
            }
            if (!string.IsNullOrWhiteSpace(Item.SecondSpellName))
            {
                result = SetupSpell(Item.SecondSpellName, Item.Level, parameters.rawParameters, parameters.parameters);
                if (result != null)
                    sb.AppendFormatAndLineIfNotEmpty(result);
            }
            if (!string.IsNullOrWhiteSpace(Item.ThirdSpellName))
            {
                result = SetupSpell(Item.ThirdSpellName, Item.Level, parameters.rawParameters, parameters.parameters);
                if (result != null)
                    sb.AppendFormatAndLineIfNotEmpty(result);
            }
            if (!string.IsNullOrWhiteSpace(Item.FourthSpellName))
            {
                result = SetupSpell(Item.FourthSpellName, Item.Level, parameters.rawParameters, parameters.parameters);
                if (result != null)
                    sb.AppendFormatAndLineIfNotEmpty(result);
            }
            if (sb.Length > 0)
                return sb.ToString();
            return null;
        }
    }
}
