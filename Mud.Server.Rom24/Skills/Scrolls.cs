using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("recite", "Ability", "Skill")]
[Syntax("[cmd] <scroll> [<target>]")]
[Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
[Help(
@"[cmd] recites a magical scroll; the <target> is optional, depending on the
nature of the scroll. Scrolls have a single use and will be consume after use.
Magical items require training to use properly.  If your character lacks the
necessary skill to use an item, he will fail, possibly destroying it. Scroll is 
the reading of magical scrolls and books")]
public class Scrolls : ItemCastSpellSkillBase<IItemScroll>
{
    private const string SkillName = "Scrolls";

    public Scrolls(ILogger<Scrolls> logger, IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
        : base(logger, randomManager, abilityManager, itemManager)
    {
    }

    protected override string ActionWord => "Recite";

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        if (skillActionInput.Parameters.Length == 0)
            return "Recite what?";

        var item = FindHelpers.FindByName(User.Inventory.Where(User.CanSee), skillActionInput.Parameters[0]);
        if (item == null)
            return "You do not have that scroll.";

        if (item is not IItemScroll itemScroll)
            return "You can recite only scrolls.";
        Item = itemScroll;

        if (User.Level < Item.Level)
            return "This scroll is too complex for you to comprehend.";

        // scroll found, remove it from parameters
        var newParameters = skillActionInput.Parameters.Skip(1).ToArray();

        // perform setup on each spell
        StringBuilder sb = new ();
        if (!string.IsNullOrWhiteSpace(Item.FirstSpellName))
        {
            var result = SetupSpell(Item.FirstSpellName, Item.Level, newParameters);
            if (result != null)
                sb.AppendFormatAndLineIfNotEmpty(result);
        }
        if (!string.IsNullOrWhiteSpace(Item.SecondSpellName))
        {
            var result = SetupSpell(Item.SecondSpellName, Item.Level, newParameters);
            if (result != null)
                sb.AppendFormatAndLineIfNotEmpty(result);
        }
        if (!string.IsNullOrWhiteSpace(Item.ThirdSpellName))
        {
            var result = SetupSpell(Item.ThirdSpellName, Item.Level, newParameters);
            if (result != null)
                sb.AppendFormatAndLineIfNotEmpty(result);
        }
        if (!string.IsNullOrWhiteSpace(Item.FourthSpellName))
        {
            var result = SetupSpell(Item.FourthSpellName, Item.Level, newParameters);
            if (result != null)
                sb.AppendFormatAndLineIfNotEmpty(result);
        }
        if (sb.Length > 0)
            return sb.ToString();
        return null;
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
}
