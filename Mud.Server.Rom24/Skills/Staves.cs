using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("brandish", "Ability", "Skill")]
[Syntax("[cmd] <staff>")]
[Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
[Help(
@"[cmd] brandishes a magical staff. You must HOLD a staff before using [cmd].
Staves have multiple charges, when a magical object has no more charges,
it will be consumed.
Magical items require training to use properly.  If your character lacks the
necessary skill to use an item, he will fail, possibly destroying it. Staves is
the use of staves and similar devices")]
[OneLineHelp("use of magical staves")]
public class Staves : ItemCastSpellSkillBase<IItemStaff>
{
    private const string SkillName = "Staves";

    public Staves(ILogger<Staves> logger, IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
        : base(logger, randomManager, abilityManager, itemManager)
    {
    }

    protected override string ActionWord => "Brandish";

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        var item = User.GetEquipment<IItemStaff>(EquipmentSlots.OffHand);
        if (item == null)
            return "You can brandish only with a staff.";
        Item = item;
        if (Item.CurrentChargeCount == 0)
        {
            User.Act(ActOptions.ToAll, "{0:P} {1} blazes bright and is gone.", User, Item);
            ItemManager.RemoveItem(Item);
            return string.Empty; // stop but don't display anything
        }
        if (Item.SpellName != null)
            return SetupSpellForEachAvailableTargets(Item.SpellName, Item.SpellLevel, skillActionInput.Parameters);
        return null;
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
}
