using System;
using System.Collections.Generic;
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
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Character
{
    public partial class CharacterBase
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
        protected virtual CommandExecutionResults DoLook(string rawParameters, params ICommandParameter[] parameters)
        {
            // 0: sleeping/blind/dark room
            if (Position < Positions.Sleeping)
            {
                Send("You can't see anything but stars!");
                return CommandExecutionResults.NoExecution;
            }
            if (Position == Positions.Sleeping)
            {
                Send("You can't see anything, you're sleeping!");
                return CommandExecutionResults.NoExecution;
            }
            if (CharacterFlags.HasFlag(CharacterFlags.Blind))
            {
                Send("You can't see a thing!");
                return CommandExecutionResults.NoExecution;
            }
            if (Room.IsDark)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("It is pitch black ... ");
                AppendCharacters(sb, Room);
                Send(sb);
                return CommandExecutionResults.Ok;
            }

            // 1: room+exits+chars+items
            if (string.IsNullOrWhiteSpace(rawParameters))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(1): room");
                StringBuilder sb = new StringBuilder();
                AppendRoom(sb, Room);
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // 2: container in room then inventory then equipment
            if (parameters[0].Value == "in" || parameters[0].Value == "on" || parameters[0].Value == "into")
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): container in room, inventory, equipment");
                // look in container
                if (parameters.Length == 1)
                {
                    Send("Look in what?");
                    return CommandExecutionResults.SyntaxErrorNoDisplay;
                }
                // search in room, then in inventory(unequipped), then in equipment
                IItem containerItem = FindHelpers.FindItemHere(this, parameters[1]);
                if (containerItem == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): found in {0}", containerItem.ContainedInto.DebugName);

                // drink container
                if (containerItem is IItemDrinkContainer itemDrinkContainer)
                {
                    if (itemDrinkContainer.IsEmpty)
                        Send("It's empty.");
                    else
                    {
                        string left = itemDrinkContainer.LiquidLeft < itemDrinkContainer.MaxLiquid / 4
                            ? "less than half-"
                            : (itemDrinkContainer.LiquidLeft < (3 * itemDrinkContainer.MaxLiquid) / 4
                                ? "about half-"
                                : "more than half-");
                        var liquidInfo = TableValues.LiquidInfo(itemDrinkContainer.LiquidName);
                        Send("It's {0}filled with a {1} liquid.", left, liquidInfo.color);
                    }
                    return CommandExecutionResults.Ok;
                }
                // other container
                IContainer container = containerItem as IContainer;
                if (container == null)
                {
                    Send("This is not a container.");
                    return CommandExecutionResults.InvalidTarget;
                }
                // closed ?
                if (containerItem is ICloseable closeable && closeable.IsClosed)
                {
                    Send("It's closed.");
                    return CommandExecutionResults.Ok;
                }
                StringBuilder sb = new StringBuilder();
                AppendContainerContent(sb, container);
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // 3: character in room
            ICharacter victim = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
            if (victim != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(3): character in room");
                if (this == victim)
                    Act(ActOptions.ToRoom, "{0} looks at {0:m}self.", this);
                else
                    Act(ActOptions.ToRoom, "{0} looks at {1}.", this, victim);
                StringBuilder sb = new StringBuilder();
                AppendCharacter(sb, victim, true); // TODO: always peeking ???
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // 4: search among inventory/equipment/room.content if an item has extra description or name equals to parameters
            string itemDescription;
            bool itemFound = FindItemByExtraDescriptionOrName(parameters[0], out itemDescription);
            if (itemFound)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(4): item in inventory+equipment+room -> {0}", itemDescription);
                Send(itemDescription, false);
                return CommandExecutionResults.Ok;
            }
            // 5: extra description in room
            if (Room.ExtraDescriptions != null && Room.ExtraDescriptions.Any())
            {
                // TODO: try to use ElementAtOrDefault
                int count = 0;
                foreach (var extraDescription in Room.ExtraDescriptions)
                {
                    if (parameters[0].Tokens.All(x => StringCompareHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameters[0].Count)
                    {
                        foreach(string desc in extraDescription)
                            Send(desc, false);
                        return CommandExecutionResults.Ok;
                    }
                }
            }
            // 6: direction
            ExitDirections direction;
            if (ExitDirectionsExtensions.TryFindDirection(parameters[0].Value, out direction))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(6): direction");
                IExit exit = Room[direction];
                if (exit?.Destination == null)
                    Send("Nothing special there.");
                else
                {
                    Send(exit.Description ?? "Nothing special there.");
                    if (exit.Keywords.Any())
                    {
                        string exitName = exit.Keywords.FirstOrDefault() ?? "door";
                        if (exit.IsClosed)
                            Send("The {0} is closed.", exitName);
                        else if (exit.IsDoor)
                            Send("The {0} is open.", exitName);
                    }
                }
                return CommandExecutionResults.Ok;
            }
            //
            Send(StringHelpers.ItemNotFound);
            return CommandExecutionResults.TargetNotFound;
        }
    }
}
