using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;

namespace Mud.Server.Character.Information
{
    // 0/ if sleeping/blind/room is dark
    // 1/ else if no parameter, look in room
    // 2/ else if 1st parameter is 'in' or 'on', search item (matching 2nd parameter) in the room, then inventory, then equipment, and display its content
    // 3/ else if a character can be found in room (matching 1st parameter), display character info
    // 4/ else if an item can be found in inventory+room (matching 1st parameter), display item description or extra description
    // 5/ else, if an extra description can be found in room (matching 1st parameter), display it
    // 6/ else, if 1st parameter is a direction, display if there is an exit/door
    [CharacterCommand("look", "Information", Priority = 0, MinPosition = Positions.Resting)]
    [Syntax(
        "[cmd]",
        "[cmd] in <container>",
        "[cmd] in <corpse>",
        "[cmd] <character>",
        "[cmd] <item>",
        "[cmd] <keyword>",
        "[cmd] <direction>")]
    public class Look : CharacterGameAction
    {
        private ITableValues TableValues { get; }

        public bool IsRoomDark { get; protected set; }
        public bool DisplayRoom { get; protected set; }
        public IItemDrinkContainer DrinkContainer { get; protected set; }
        public IContainer ItemContainer { get; protected set; }
        public ICharacter Victim { get; protected set; }
        public string ItemDescription { get; protected set; }
        public string RoomExtraDescription { get; protected set; }
        public ExitDirections? Direction { get; protected set; }

        public Look(ITableValues tableValues)
        {
            TableValues = tableValues;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            // 0: sleeping/blind/dark room
            if (Actor.Position < Positions.Sleeping)
                return "You can't see anything but stars!";
            if (Actor.Position == Positions.Sleeping)
                return "You can't see anything, you're sleeping!";
            if (Actor.CharacterFlags.HasFlag(CharacterFlags.Blind))
                return "You can't see a thing!";

            if (Actor.Room.IsDark)
            {
                IsRoomDark = true;
                return null;
            }

            // 1: room+exits+chars+items
            if (actionInput.Parameters.Length == 0)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(1): room");
                DisplayRoom = true;
                return null;
            }

            // 2: container in room then inventory then equipment
            if (actionInput.Parameters[0].Value == "in" || actionInput.Parameters[0].Value == "on" || actionInput.Parameters[0].Value == "into")
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): container in room, inventory, equipment");

                if (actionInput.Parameters.Length == 1)
                    return "Look in what?";
                // search in room, then in inventory(unequipped), then in equipment
                IItem containerItem = FindHelpers.FindItemHere(Actor, actionInput.Parameters[1]);
                if (containerItem == null)
                    return StringHelpers.ItemNotFound;
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): found in {0}", containerItem.ContainedInto.DebugName);
                DrinkContainer = containerItem as IItemDrinkContainer;
                if (DrinkContainer != null)
                    return null;
                ItemContainer = containerItem as IContainer;
                if (ItemContainer == null)
                    return "This is not a container.";
                return null;
            }

            // 3: character in room
            Victim = FindHelpers.FindByName(Actor.Room.People.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (Victim != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(3): character in room");
                return null;
            }

            // 4: search among inventory/equipment/room.content if an item has extra description or name equals to parameters
            string itemDescription;
            bool itemFound = FindItemByExtraDescriptionOrName(actionInput.Parameters[0], out itemDescription);
            if (itemFound)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(4): item in inventory+equipment+room -> {0}", itemDescription);
                ItemDescription = itemDescription;
                return null;
            }

            // 5: extra description in room
            if (Actor.Room.ExtraDescriptions != null && Actor.Room.ExtraDescriptions.Any())
            {
                // TODO: try to use ElementAtOrDefault
                int count = 0;
                foreach (var extraDescription in Actor.Room.ExtraDescriptions)
                {
                    if (actionInput.Parameters[0].Tokens.All(x => StringCompareHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == actionInput.Parameters[0].Count)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (string desc in extraDescription)
                            sb.Append(desc);
                        RoomExtraDescription = sb.ToString();
                        Log.Default.WriteLine(LogLevels.Debug, "DoLook(5): extra description in room -> {0}", RoomExtraDescription);
                        return null;
                    }
                }
            }

            // 6: direction
            ExitDirections direction;
            if (ExitDirectionsExtensions.TryFindDirection(actionInput.Parameters[0].Value, out direction))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(6): direction");
                Direction = direction;
                return null;
            }
            return StringHelpers.ItemNotFound;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (IsRoomDark)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("It is pitch black ... ");
                foreach (ICharacter victim in Actor.Room.People.Where(x => x != Actor))
                {
                    //  (see act_info.C:714 show_char_to_char)
                    if (Actor.CanSee(victim)) // see act_info.C:375 show_char_to_char_0)
                        victim.AppendInRoom(sb, Actor);
                    else if (Actor.Room.IsDark && victim.CharacterFlags.HasFlag(CharacterFlags.Infrared))
                        sb.AppendLine("You see glowing red eyes watching YOU!");
                }
                Actor.Send(sb);
                return;
            }
            if (DisplayRoom)
            {
                StringBuilder sb = new StringBuilder();
                Actor.Room.Append(sb, Actor);
                Actor.Send(sb);
                return;
            }
            if (DrinkContainer != null)
            {
                if (DrinkContainer.IsEmpty)
                    Actor.Send("It's empty.");
                else
                {
                    string left = DrinkContainer.LiquidLeft < DrinkContainer.MaxLiquid / 4
                        ? "less than half-"
                        : (DrinkContainer.LiquidLeft < (3 * DrinkContainer.MaxLiquid) / 4
                            ? "about half-"
                            : "more than half-");
                    var liquidInfo = TableValues.LiquidInfo(DrinkContainer.LiquidName);
                    Actor.Send("It's {0}filled with a {1} liquid.", left, liquidInfo.color);
                }
                return;
            }
            if (ItemContainer != null)
            {
                if (ItemContainer is ICloseable closeable && closeable.IsClosed)
                {
                    Actor.Send("It's closed.");
                    return;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendFormatLine("{0} holds:", ItemContainer.RelativeDisplayName(Actor));
                ItemsHelpers.AppendItems(sb, ItemContainer.Content.Where(x => Actor.CanSee(x)), Actor, true, true);
                Actor.Send(sb);
                return;
            }
            if (Victim != null)
            {
                if (Actor == Victim)
                    Actor.Act(ActOptions.ToRoom, "{0} looks at {0:m}self.", Actor);
                else
                    Actor.Act(ActOptions.ToRoom, "{0} looks at {1}.", Actor, Victim);
                StringBuilder sb = new StringBuilder();
                Victim.Append(sb, Actor , true); // TODO: always peeking ???
                Actor.Send(sb);
                return;
            }
            if (ItemDescription != null)
            {
                Actor.Send(ItemDescription, false);
                return;
            }

            if (RoomExtraDescription != null)
            {
                Actor.Send(RoomExtraDescription, false);
                return;
            }

            if (Direction.HasValue) // should always be true
            {
                IExit exit = Actor.Room[Direction.Value];

                if (exit?.Destination == null)
                    Actor.Send("Nothing special there.");
                else
                {
                    Actor.Send(exit.Description ?? "Nothing special there.");
                    if (exit.Keywords.Any())
                    {
                        string exitName = exit.Keywords.FirstOrDefault() ?? "door";
                        if (exit.IsClosed)
                            Actor.Send("The {0} is closed.", exitName);
                        else if (exit.IsDoor)
                            Actor.Send("The {0} is open.", exitName);
                    }
                }
                return;
            }
        }

        protected bool FindItemByExtraDescriptionOrName(ICommandParameter parameter, out string description) // Find by extra description then name (search in inventory, then equipment, then in room)
        {
            description = null;
            int count = 0;
            foreach (IItem item in Actor.Inventory.Where(x => Actor.CanSee(x))
                .Concat(Actor.Equipments.Where(x => x.Item != null && Actor.CanSee(x.Item)).Select(x => x.Item))
                .Concat(Actor.Room.Content.Where(x => Actor.CanSee(x))))
            {
                // Search in item extra description keywords
                if (item.ExtraDescriptions != null)
                {
                    foreach (var extraDescription in item.ExtraDescriptions)
                        if (parameter.Tokens.All(x => StringCompareHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameter.Count)
                        {
                            description = extraDescription.FirstOrDefault(); // TODO: really what we want ?
                            return true;
                        }
                }
                // Search in item keywords
                if (StringCompareHelpers.StringListsStartsWith(item.Keywords, parameter.Tokens)
                    && ++count == parameter.Count)
                {
                    StringBuilder sb = new StringBuilder();
                    description = item.Append(sb, Actor, false).ToString();
                    return true;
                }
            }
            return false;
        }
    }
}
