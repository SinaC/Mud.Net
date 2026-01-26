using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("zap", "Ability", "Skill")]
[Syntax("[cmd] <wand> [<target>]")]
[Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
[Help(
@"[cmd] zaps a magical wand at a target.  If the target is not specified,
and you are fighting someone, then that character is used for a target.
Wands have multiple charges, when a magical object has no more charges,
it will be consumed.
Magical items require training to use properly.  If your character lacks the
necessary skill to use an item, he will fail, possibly destroying it. Wands is
the use of wands and similar items ")]
[OneLineHelp("use of magical wands")]
public class Wands : ItemCastSpellSkillBase<IItemWand>
{
    private const string SkillName = "Wands";

    public Wands(ILogger<Wands> logger, IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
        : base(logger, randomManager, abilityManager, itemManager)
    {
    }

    protected override string ActionWord => "Zap";

    protected IEntity Target { get; set; } = default!;

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        Item = User.GetEquipment<IItemWand>(EquipmentSlots.OffHand)!;
        if (Item == null)
            return "You can zap only with a wand.";
        if (Item.CurrentChargeCount == 0)
        {
            User.Act(ActOptions.ToAll, "{0:P} {1} explodes into fragments.", User, Item);
            ItemManager.RemoveItem(Item);
            return string.Empty; // stop but don't display anything
        }
        if (Item.SpellName != null)
        {
            var setupResult = SetupSpellAndPredefinedTarget(Item.SpellName, Item.SpellLevel, out var target, skillActionInput.Parameters);
            if (setupResult != null)
                return setupResult;
            Target = target;
        }
        return null;
    }

    protected override bool Invoke()
    {
        bool success;
        if (Target != null)
            User.Act(ActOptions.ToAll, "%W%{0:N} zap{0:v} {1} with {2}.%x%", User, Target, Item);
        else
            User.Act(ActOptions.ToAll, "%W%{0:N} use{0:v} {1}.%x%", User, Item);
        int chance = 20 + (4 * Learned) / 5;
        if (User.Level < Item.Level
            || !RandomManager.Chance(chance))
        {
            User.Act(ActOptions.ToAll, "{0:P} efforts with {1} produce only smoke and sparks.", User, Item);
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
            User.Act(ActOptions.ToAll, "{0:P} {1} explodes into fragments.", User, Item);
            ItemManager.RemoveItem(Item);
        }

        return success;
    }
}
