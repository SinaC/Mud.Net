using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Table;
using System.Reflection.Metadata;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

// 0/ if sleeping/blind/room is dark
// 1/ else if no parameter, look in room
// 2/ else if 1st parameter is 'in' or 'on', search item (matching 2nd parameter) in the room, then inventory, then equipment, and display its content
// 3/ else if a character can be found in room (matching 1st parameter), display character info
// 4/ else if an item can be found in inventory+room (matching 1st parameter), display item description or extra description
// 5/ else, if an extra description can be found in room (matching 1st parameter), display it
// 6/ else, if 1st parameter is a direction, display if there is an exit/door
[CharacterCommand("look", "Information", Priority = 0), MinPosition(Positions.Resting)]
[Alias("read")]
[Syntax(
    "[cmd]",
    "[cmd] in <container>",
    "[cmd] in <corpse>",
    "[cmd] <character>",
    "[cmd] <item>",
    "[cmd] <keyword>",
    "[cmd] <direction>")]
[Help(@"[cmd] looks at something and sees what you can see.")]
public class Look : CharacterGameAction
{
    private ILogger<Look> Logger { get; }
    private IAbilityManager AbilityManager { get; }
    private ITableValues TableValues { get; }
    private IWiznet Wiznet { get; }

    public Look(ILogger<Look> logger, IAbilityManager abilityManager, ITableValues tableValues, IWiznet wiznet)
    {
        Logger = logger;
        AbilityManager = abilityManager;
        TableValues = tableValues;
        Wiznet = wiznet;
    }

    protected bool IsRoomDark { get; set; }
    protected bool DisplayRoom { get; set; }
    protected IItemDrinkContainer DrinkContainer { get; set; } = default!;
    protected IContainer ItemContainer { get; set; } = default!;
    protected ICharacter Victim { get; set; } = default!;
    protected string ItemDescription { get; set; } = default!;
    protected string RoomExtraDescription { get; set; } = default!;
    protected ExitDirections? Direction { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // 0: sleeping/blind/dark room
        if (Actor.Position < Positions.Sleeping)
            return "You can't see anything but stars!";
        if (Actor.Position == Positions.Sleeping)
            return "You can't see anything, you're sleeping!";
        if (Actor.CharacterFlags.IsSet("Blind"))
            return "You can't see a thing!";

        if (Actor.Room.IsDark && !(Actor is IPlayableCharacter { IsImmortal: true }))
        {
            IsRoomDark = true;
            return null;
        }

        // 1: room+exits+chars+items
        if (actionInput.Parameters.Length == 0)
        {
            Logger.LogDebug("DoLook(1): room");
            DisplayRoom = true;
            return null;
        }

        // 2: container in room then inventory then equipment
        if (actionInput.Parameters[0].Value == "in" || actionInput.Parameters[0].Value == "on" || actionInput.Parameters[0].Value == "into")
        {
            Logger.LogDebug("DoLook(2): container in room, inventory, equipment");

            if (actionInput.Parameters.Length == 1)
                return "Look in what?";
            // search in room, then in inventory(unequipped), then in equipment
            var containerItem = FindHelpers.FindItemHere(Actor, actionInput.Parameters[1]);
            if (containerItem == null)
                return StringHelpers.ItemNotFound;
            Logger.LogDebug("DoLook(2): found in {containedInto}", containerItem.ContainedInto?.DebugName ?? "???");
            if (containerItem is IItemDrinkContainer drinkContainer)
            {
                DrinkContainer = drinkContainer;
                return null;
            }
            if (containerItem is not IContainer itemContainer)
                return "This is not a container.";
            ItemContainer = itemContainer;
            return null;
        }

        // 3: character in room
        Victim = FindHelpers.FindByName(Actor.Room.People.Where(Actor.CanSee), actionInput.Parameters[0])!;
        if (Victim != null)
        {
            Logger.LogDebug("DoLook(3): character in room");
            return null;
        }

        int count = 0; // this will count how many matching keywords we found in item and room extra descriptions
        // 4: search among inventory/equipment/room.content if an item has extra description or name equals to parameters
        bool itemFound = FindItemByExtraDescriptionOrName(actionInput.Parameters[0], ref count, out string itemDescription);
        if (itemFound)
        {
            Logger.LogDebug("DoLook(4): item in inventory+equipment+room -> {item}", itemDescription);
            ItemDescription = itemDescription;
            return null;
        }

        // 5: extra description in room
        if (Actor.Room.ExtraDescriptions != null && Actor.Room.ExtraDescriptions.Any())
        {
            // TODO: try to use ElementAtOrDefault
            foreach (var extraDescription in Actor.Room.ExtraDescriptions)
            {
                if (actionInput.Parameters[0].Tokens.All(x => StringCompareHelpers.AnyStringStartsWith(extraDescription.Keywords, x))
                        && ++count == actionInput.Parameters[0].Count)
                {
                    RoomExtraDescription = extraDescription.Description;
                    Logger.LogDebug("DoLook(5): extra description in room -> {room}", RoomExtraDescription);
                    return null;
                }
            }
        }

        // 6: direction
        if (ExitDirectionsExtensions.TryFindDirection(actionInput.Parameters[0].Value, out ExitDirections direction))
        {
            Logger.LogDebug("DoLook(6): direction");
            Direction = direction;
            return null;
        }
        return StringHelpers.ItemNotFound;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (IsRoomDark)
        {
            StringBuilder sb = new();
            sb.AppendLine("It is pitch black ... ");
            //// display items
            //ItemsHelpers.AppendItems(sb, Actor.Room.Content.Where(Actor.CanSee), Actor, false, false);
            // display characters
            foreach (var victim in Actor.Room.People.Where(x => x != Actor))
            {
                //  (see act_info.C:714 show_char_to_char)
                if (Actor.CanSee(victim)) // see act_info.C:375 show_char_to_char_0)
                    victim.AppendInRoom(sb, Actor);
                else if (Actor.Room.IsDark && victim.CharacterFlags.IsSet("Infrared"))
                    sb.AppendLine("You see glowing red eyes watching YOU!");
            }
            Actor.Send(sb);
        }
        else if (DisplayRoom)
        {
            StringBuilder sb = new();
            Actor.Room.Append(sb, Actor);
            Actor.Send(sb);
        }
        else if (DrinkContainer != null)
        {
            if (DrinkContainer.IsEmpty)
                Actor.Send("It's empty.");
            else
            {
                string left = DrinkContainer.LiquidLeft < DrinkContainer.MaxLiquid / 4
                    ? "less than half-"
                    : DrinkContainer.LiquidLeft < 3 * DrinkContainer.MaxLiquid / 4
                        ? "about half-"
                        : "more than half-";
                if (DrinkContainer.LiquidName != null)
                {
                    var (_, color, _, _, _, _, _) = TableValues.LiquidInfo(DrinkContainer.LiquidName);
                    Actor.Send("It's {0}filled with a {1} liquid.", left, color);
                }
                else
                {
                    Wiznet.Log($"Invalid liquid name {DrinkContainer.LiquidName} item {DrinkContainer.DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    Actor.Send("It's {0}filled with a transparent liquid.", left);
                }
            }
        }
        else if (ItemContainer != null)
        {
            if (ItemContainer is ICloseable closeable && closeable.IsClosed)
            {
                Actor.Send("It's closed.");
                return;
            }
            StringBuilder sb = new();
            sb.AppendFormatLine("{0} holds:", ItemContainer.RelativeDisplayName(Actor));
            ItemsHelpers.AppendItems(sb, ItemContainer.Content.Where(x => Actor.CanSee(x)), Actor, true, true);
            Actor.Send(sb);
        }
        else if (Victim != null)
        {
            Actor.Act(ActOptions.ToRoom, "{0} looks at {1:f}.", Actor, Victim);
            StringBuilder sb = new();
            var peek = CheckPeek();
            Victim.Append(sb, Actor, peek);
            Actor.Send(sb);
        }
        else if (ItemDescription != null)
        {
            Actor.Send(ItemDescription, false);
        }
        else if (RoomExtraDescription != null)
        {
            Actor.Send(RoomExtraDescription, false);
        }
        else if (Direction.HasValue) // should always be true
        {
            var exit = Actor.Room[Direction.Value];

            if (exit?.Destination == null)
                Actor.Send("Nothing special there.");
            else
            {
                Actor.Send(exit.Description ?? "Nothing special there.", false);
                if (exit.Keywords.Any())
                {
                    string exitName = exit.Keywords.FirstOrDefault() ?? "door";
                    if (exit.IsClosed)
                        Actor.Send("The {0} is closed.", exitName);
                    else if (exit.IsDoor)
                        Actor.Send("The {0} is open.", exitName);
                }
            }
        }
    }

    protected bool FindItemByExtraDescriptionOrName(ICommandParameter parameter, ref int count, out string description) // Find by extra description then name (search in inventory, then equipment, then in room)
    {
        description = null!;
        foreach (var item in Actor.Inventory.Where(x => Actor.CanSee(x))
            .Concat(Actor.Equipments.Where(x => x.Item != null && Actor.CanSee(x.Item)).Select(x => x.Item!))
            .Concat(Actor.Room.Content.Where(x => Actor.CanSee(x))))
        {
            // Search in item extra description keywords
            if (item.ExtraDescriptions != null)
            {
                foreach (var extraDescription in item.ExtraDescriptions)
                    if (parameter.Tokens.All(x => StringCompareHelpers.AnyStringStartsWith(extraDescription.Keywords, x))
                        && ++count == parameter.Count)
                    {
                        description = extraDescription.Description;
                        return true;
                    }
            }
            // Search in item keywords
            if (StringCompareHelpers.AllStringsStartsWith(item.Keywords, parameter.Tokens)
                && ++count == parameter.Count)
            {
                StringBuilder sb = new();
                description = item.Append(sb, Actor, false).AppendLine().ToString();
                return true;
            }
        }
        return false;
    }

    private bool CheckPeek()
    {
        if (Actor is IPlayableCharacter pc)
        {
            var (percentage, abilityLearned) = Actor.GetAbilityLearnedAndPercentage("peek");
            if (abilityLearned is not null && percentage > 0)
            {
                var peekAbility = AbilityManager.CreateInstance<IPassive>("peek");
                if (peekAbility is not null)
                {
                    return peekAbility.IsTriggered(pc, null!, true, out _, out _);
                }
            }
        }
        return false;
    }
}
