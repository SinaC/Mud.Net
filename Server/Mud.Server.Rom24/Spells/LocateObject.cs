using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Parser.Interfaces;
using System.Text;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Detection, PulseWaitTime = 18)]
[Syntax("cast [spell] <name>")]
[Help(
@"This spell reveals the location of all objects with the given name.")]
[OneLineHelp("finds a specific item")]
public class LocateObject : SpellBase
{
    private const string SpellName = "Locate Object";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private IItemManager ItemManager { get; }
    private IParser Parser { get; }

    public LocateObject(ILogger<LocateObject> logger, IRandomManager randomManager, IItemManager itemManager, IParser parser) 
        : base(logger, randomManager)
    {
        ItemManager = itemManager;
        Parser = parser;
    }
    protected string ItemName { get; set; } = default!;

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        ItemName = Parser.JoinParameters(spellActionInput.Parameters);
        if (string.IsNullOrWhiteSpace(ItemName))
            return "Locate what?";
        return null;
    }

    protected override void Invoke()
    {
        StringBuilder sb = new ();
        var hasHolylight = Caster.ImmortalMode.IsSet("Holylight");
        int maxFound = hasHolylight
            ? 200
            : Level * 2;
        int number = 0;
        var foundItems = FindHelpers.FindAllByName(ItemManager.Items.Where(x => Caster.CanSee(x) && !x.ItemFlags.IsSet("NoLocate") && x.Level <= Caster.Level && RandomManager.Range(1, 100) <= 2 * Level), ItemName);
        foreach (var item in foundItems)
        {
            IItem outOfItemContainer = item;
            // Get container until container is not an item anymore (room or character)
            int maxDepth = 500;
            while (outOfItemContainer.ContainedInto is IItem container)
            {
                outOfItemContainer = container;
                maxDepth--;
                if (maxDepth <= 0) // avoid infinite loop if something goes wrong in container
                    break;
            }

            if (item.ContainedInto is IRoom room)
            {
                if (hasHolylight)
                    sb.AppendFormatLine("One is in {0} (room {1})", room.DisplayName, room.Blueprint?.Id.ToString() ?? "???");
                else
                    sb.AppendFormatLine("One is in {0}", room.DisplayName);
            }
            else if (item.ContainedInto is ICharacter character && Caster.CanSee(character))
                sb.AppendFormatLine("One is carried by {0}", character.DisplayName);
            else if (item.EquippedBy != null && Caster.CanSee(item.EquippedBy))
                sb.AppendFormatLine("One is carried by {0}", item.EquippedBy.DisplayName);

            number++;
            if (number >= maxFound)
                break;
        }
        if (number == 0)
            Caster.Send("Nothing like that in heaven or earth.");
        else
            Caster.Page(sb);
    }
}
