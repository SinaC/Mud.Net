using System.Linq;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [Command("north", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoNorth(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.North, true);
            return CommandExecutionResults.Ok;
        }

        [Command("east", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoEast(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.East, true);
            return CommandExecutionResults.Ok;
        }

        [Command("south", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoSouth(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.South, true);
            return CommandExecutionResults.Ok;
        }

        [Command("west", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoWest(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.West, true);
            return CommandExecutionResults.Ok;
        }

        [Command("up", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoUp(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.Up, true);
            return CommandExecutionResults.Ok;
        }

        [Command("down", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoDown(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.Down, true);
            return CommandExecutionResults.Ok;
        }

        [Command("northeast", Category = "Movement", Priority = 1)]
        [Command("ne", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoNorthEast(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.NorthEast, true);
            return CommandExecutionResults.Ok;
        }

        [Command("northwest", Category = "Movement", Priority = 1)]
        [Command("nw", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoNorthWest(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.NorthWest, true);
            return CommandExecutionResults.Ok;
        }

        [Command("southeast", Category = "Movement", Priority = 1)]
        [Command("se", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoSouthEast(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.SouthEast, true);
            return CommandExecutionResults.Ok;
        }

        [Command("southwest", Category = "Movement", Priority = 1)]
        [Command("sw", Category = "Movement", Priority = 0)]
        protected virtual CommandExecutionResults DoSouthWest(string rawParameters, params CommandParameter[] parameters)
        {
            Move(ExitDirections.SouthWest, true);
            return CommandExecutionResults.Ok;
        }

        [Command("open", Category = "Movement")]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoOpen(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Open what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Content.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemPortal itemPortal)
                {
                    // TODO: no open/close/lock/unlock on portal for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                if (item is IItemContainer itemContainer)
                {
                    // TODO: no open/close/lock/unlock on container for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return CommandExecutionResults.TargetNotFound;
            if (!exit.IsClosed)
            {
                Send("It's already open.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (exit.IsLocked)
            {
                Send("It's locked.");
                return CommandExecutionResults.InvalidTarget;
            }

            // Open this side side
            exit.Open();
            Act(ActOptions.ToRoom, "{0:N} opens the {1}.", this, exit);
            Send("Ok.");

            // Open the other side
            IExit otherSideExit = exit.Destination.Exit(exitDirection.ReverseDirection());
            if (otherSideExit != null)
            {
                otherSideExit.Open();
                Act(exit.Destination.People, "The {0} opens.", otherSideExit);
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");
            return CommandExecutionResults.Ok;
        }

        [Command("close", Category = "Movement")]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoClose(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Close what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Content.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemPortal itemPortal)
                {
                    // TODO: no open/close/lock/unlock on portal for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                if (item is IItemContainer itemContainer)
                {
                    // TODO: no open/close/lock/unlock on container for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return CommandExecutionResults.TargetNotFound;
            if (exit.IsClosed)
            {
                Send("It's already closed.");
                return CommandExecutionResults.InvalidTarget;
            }

            // Close this side
            exit.Close();
            Act(ActOptions.ToRoom, "{0:N} closes {1}.", this, exit);
            Send("Ok.");

            // Close the other side
            IExit otherSideExit = exit.Destination.Exit(exitDirection.ReverseDirection());
            if (otherSideExit != null)
            {
                otherSideExit.Close();
                Act(exit.Destination.People, "The {0} closes.", otherSideExit);
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");
            return CommandExecutionResults.Ok;
        }

        [Command("unlock", Category = "Movement")]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoUnlock(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Unlock what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Content.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemPortal itemPortal)
                {
                    // TODO: no open/close/lock/unlock on portal for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                if (item is IItemContainer itemContainer)
                {
                    // TODO: no open/close/lock/unlock on container for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return CommandExecutionResults.TargetNotFound;
            if (!exit.IsClosed)
            {
                Send("It's not closed.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (exit.Blueprint.Key <= 0)
            {
                Send("It can't be unlocked.");
                return CommandExecutionResults.InvalidTarget;
            }
            // search key
            bool keyFound = Content.OfType<IItemKey>().Any(x => x.Blueprint.Id == exit.Blueprint.Key);
            if (!keyFound)
            {
                Send("You lack the key.");
                return CommandExecutionResults.NoExecution;
            }
            if (!exit.IsLocked)
            {
                Send("It's already unlocked.");
                return CommandExecutionResults.NoExecution;
            }
            // Unlock this side
            exit.Unlock();
            Send("*Click*");
            Act(ActOptions.ToRoom, "{0:N} unlocks the {1}.", this, exit);

            // Unlock other side
            IExit otherSideExit = exit.Destination.Exit(exitDirection.ReverseDirection());
            if (otherSideExit != null)
                otherSideExit.Unlock();
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");

            return CommandExecutionResults.Ok;
        }

        [Command("lock", Category = "Movement")]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoLock(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Lock what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Content.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemPortal itemPortal)
                {
                    // TODO: no open/close/lock/unlock on portal for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                if (item is IItemContainer itemContainer)
                {
                    // TODO: no open/close/lock/unlock on container for now
                    Send(StringHelpers.NotYetImplemented);
                    return CommandExecutionResults.Error;
                }
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return CommandExecutionResults.TargetNotFound;
            if (!exit.IsClosed)
            {
                Send("It's not closed.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (exit.Blueprint.Key <= 0)
            {
                Send("It can't be locked.");
                return CommandExecutionResults.InvalidTarget;
            }
            // search key
            bool keyFound = Content.OfType<IItemKey>().Any(x => x.Blueprint.Id == exit.Blueprint.Key);
            if (!keyFound)
            {
                Send("You lack the key.");
                return CommandExecutionResults.NoExecution;
            }
            if (exit.IsLocked)
            {
                Send("It's already locked.");
                return CommandExecutionResults.NoExecution;
            }
            // Unlock this side
            exit.Lock();
            Send("*Click*");
            Act(ActOptions.ToRoom, "{0:N} locks the {1}.", this, exit);

            // Unlock other side
            IExit otherSideExit = exit.Destination.Exit(exitDirection.ReverseDirection());
            if (otherSideExit != null)
                otherSideExit.Lock();
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");

            return CommandExecutionResults.Ok;
        }

        [Command("stand", Category = "Movement")]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoStand(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return CommandExecutionResults.NoExecution;
            }
            if (Position == Positions.Standing)
            {
                Send("You are already standing.");
                return CommandExecutionResults.NoExecution;
            }

            // Search valid furniture if any
            IItemFurniture furniture = null;
            if (parameters.Length > 0)
            {
                IItem item = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[0]);
                if (item == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                furniture = item as IItemFurniture;
                if (furniture == null || !furniture.CanStand)
                {
                    Send("You can't seem to find a place to stand.");
                    return CommandExecutionResults.InvalidTarget;
                }

                if (1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room to stand on {0}.", furniture);
                    return CommandExecutionResults.InvalidTarget;
                }

                //
                ChangeFurniture(furniture);
            }
            else
                ChangeFurniture(null);

            // Change position
            if (Position == Positions.Sleeping)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} up and stand{0:v} at {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} up and stand{0:v} on {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} up and stand{0:v} in {1}.", this, furniture);
                else
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} up and stand{0:v} up.", this);
                // Autolook if impersonated/incarnated
                AutoLook();
            }
            else if (Position == Positions.Resting
                     || Position == Positions.Sitting)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} stand{0:v} at {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} stand{0:v} on {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} stand{0:v} in {1}.", this, furniture);
                else
                    Act(ActOptions.ToAll, "{0:N} stand{0:v} up.", this);
            }
            ChangePosition(Positions.Standing);

            return CommandExecutionResults.Ok;
        }

        [Command("sit", Category = "Movement")]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoSit(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return CommandExecutionResults.NoExecution;
            }
            if (Position == Positions.Sitting)
            {
                Send("You are already sitting.");
                return CommandExecutionResults.NoExecution;
            }

            // Search valid furniture if any
            IItemFurniture furniture = null;
            if (parameters.Length > 0)
            {
                IItem item = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[0]);
                if (item == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                furniture = item as IItemFurniture;
                if (furniture == null || !furniture.CanSit)
                {
                    Send("You can't sit on that.");
                    return CommandExecutionResults.InvalidTarget;
                }

                if (1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room on {0}.", furniture);
                    return CommandExecutionResults.InvalidTarget;
                }
                //
                ChangeFurniture(furniture);
            }
            else
                ChangeFurniture(null);

            // Change position
            if (Position == Positions.Sleeping)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and sit{0:v} at {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and sit{0:v} on {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and sit{0:v} in {1}.", this, furniture);
                else
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and sit{0:v} up.", this);
            }
            else if (Position == Positions.Resting)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} at {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} on {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} in {1}.", this, furniture);
                else
                    Send("You stop resting.");
            }
            else if (Position == Positions.Standing)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} down at {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} on {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} down in {1}.", this, furniture);
                else
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} down.", this);
            }
            ChangePosition(Positions.Sitting);

            return CommandExecutionResults.Ok;
        }

        [Command("rest", Category = "Movement")]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoRest(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return CommandExecutionResults.NoExecution;
            }
            if (Position == Positions.Resting)
            {
                Send("You are already resting.");
                return CommandExecutionResults.NoExecution;
            }

            // Search valid furniture if any
            IItemFurniture furniture = null;
            if (parameters.Length > 0)
            {
                IItem item = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[0]);
                if (item == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                furniture = item as IItemFurniture;
                if (furniture == null || !furniture.CanRest)
                {
                    Send("You can't rest on that.");
                    return CommandExecutionResults.InvalidTarget;
                }

                if (1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room on {0}.", furniture);
                    return CommandExecutionResults.InvalidTarget;
                }
                //
                ChangeFurniture(furniture);
            }
            else
                ChangeFurniture(null);

            // Change position
            if (Position == Positions.Sleeping)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and rest{0:v} at {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and rest{0:v} on {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and rest{0:v} in {1}.", this, furniture);
                else
                    Act(ActOptions.ToAll, "{0:N} wake{0:v} and start{0:v} resting.", this);
            }
            else if (Position == Positions.Sitting)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} rest{0:v} at {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} rest{0:v} on {1}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} rest{0:v} in {1}.", this, furniture);
                else
                    Act(ActOptions.ToRoom, "{0;N} rest{0:v}.", this);
            }
            else if (Position == Positions.Standing)
            {
                if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} down at {1} and rest{0:v}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} on {1} and rest{0:v}.", this, furniture);
                else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    Act(ActOptions.ToAll, "{0:N} rest{0:v} in {1}.", this, furniture);
                else
                    Act(ActOptions.ToAll, "{0:N} sit{0:v} down and rest{0:v}.", this);
            }
            ChangePosition(Positions.Resting);

            return CommandExecutionResults.Ok;
        }

        [Command("sleep", Category = "Movement")]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoSleep(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return CommandExecutionResults.NoExecution;
            }
            if (Position == Positions.Sleeping)
            {
                Send("You are already sleeping.");
                return CommandExecutionResults.NoExecution;
            }

            // If already on a furniture and no parameter specified, use that furniture
            // Search valid furniture if any
            IItemFurniture furniture;
            if (parameters.Length != 0)
            {
                IItem item = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[0]);
                if (item == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                furniture = item as IItemFurniture;
            }
            else
                furniture = Furniture;

            // Check furniture validity
            if (furniture != null)
            {
                if (!furniture.CanSleep)
                {
                    Send("You can't sleep on that.");
                    return CommandExecutionResults.InvalidTarget;
                }

                // If already on furniture, don't count
                if (furniture != Furniture && 1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room on {0} for you.", furniture);
                    return CommandExecutionResults.InvalidTarget;
                }
                ChangeFurniture(furniture);
            }
            else
                ChangeFurniture(null);

            // ChangeFurniture(furniture)
            if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                Act(ActOptions.ToAll, "{0:N} go{0:v} sleep at {1}.", this, furniture);
            else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                Act(ActOptions.ToAll, "{0:N} go{0:v} sleep on {1}.", this, furniture);
            else if (furniture?.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                Act(ActOptions.ToAll, "{0:N} go{0:v} sleep in {1}.", this, furniture);
            else
                Act(ActOptions.ToAll, "{0:N} go{0:v} to sleep.", this);
            ChangePosition(Positions.Sleeping);

            return CommandExecutionResults.Ok;
        }

        [Command("enter", Category = "Movement")]
        [Syntax("[cmd] <portal>")]
        protected virtual CommandExecutionResults DoEnter(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Nope, can't do it.");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            IItem item = FindHelpers.FindItemHere(this, parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (!(item is IItemPortal portal))
            {
                Send("You can't seem to find a way in.");
                return CommandExecutionResults.InvalidTarget;
            }

            Enter(portal);

            return CommandExecutionResults.Ok;
        }

        // Helpers
        private IExit VerboseFindDoor(CommandParameter parameter, out ExitDirections exitDirection)
        {
            bool wasAskingForDirection;
            bool found = FindDoor(parameter, out exitDirection, out wasAskingForDirection);
            if (!found)
            {
                //  if open north -> I see no door north here.
                //  if open black door -> I see no black door here.
                if (wasAskingForDirection)
                    Send($"I see no door {parameter.Value} here.");
                else
                    Send($"I see no {parameter.Value} here.");
                return null;
            }
            IExit exit = Room.Exit(exitDirection);
            if (exit == null)
                return null;
            if (!exit.IsDoor)
            {
                Send("You can't do that.");
                return null;
            }
            return exit;
        }

        private bool FindDoor(CommandParameter parameter, out ExitDirections exitDirection, out bool wasAskingForDirection)
        {
            if (ExitDirectionsExtensions.TryFindDirection(parameter.Value, out exitDirection))
            {
                wasAskingForDirection = true;
                return true;
            }
            wasAskingForDirection = false;
            //exit = Room.Exits.FirstOrDefault(x => x?.Destination != null && x.IsDoor && x.Keywords.Any(k => FindHelpers.StringStartsWith(k, parameter.Value)));
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IExit exit = Room.Exit(direction);
                if (exit?.Destination != null && exit.IsDoor && exit.Keywords.Any(k => FindHelpers.StringStartsWith(k, parameter.Value)))
                {
                    exitDirection = direction;
                    return true;
                }
            }
            return false;
        }
    }
}
