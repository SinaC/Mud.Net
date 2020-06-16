using System.Linq;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("north", "Movement", Priority = 0, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoNorth(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.North, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("east", "Movement", Priority = 0, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoEast(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.East, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("south", "Movement", Priority = 0, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoSouth(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.South, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("west", "Movement", Priority = 0, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoWest(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.West, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("up", "Movement", Priority = 0, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoUp(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.Up, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("down", "Movement", Priority = 0, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoDown(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.Down, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("northeast", "Movement", Priority = 2, MinPosition = Positions.Standing)]
        [CharacterCommand("ne", "Movement", Priority = 1, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoNorthEast(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.NorthEast, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("northwest", "Movement", Priority = 2, MinPosition = Positions.Standing)]
        [CharacterCommand("nw", "Movement", Priority = 1, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoNorthWest(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.NorthWest, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("southeast", "Movement", Priority = 2, MinPosition = Positions.Standing)]
        [CharacterCommand("se", "Movement", Priority = 1, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoSouthEast(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.SouthEast, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("southwest", "Movement", Priority = 2, MinPosition = Positions.Standing)]
        [CharacterCommand("sw", "Movement", Priority = 1, MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoSouthWest(string rawParameters, params ICommandParameter[] parameters)
        {
            Move(ExitDirections.SouthWest, true);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("open", "Movement", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoOpen(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Open what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Inventory.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemCloseable itemCloseable)
                {
                    if (!itemCloseable.IsCloseable)
                    {
                        Send("You can't do that.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    if (!itemCloseable.IsClosed)
                    {
                        Send("It's already opened.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    if (itemCloseable.IsLocked)
                    {
                        Send("It's locked.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                }
                else
                {
                    Send("You can't do that.");
                    return CommandExecutionResults.InvalidTarget;
                }
                itemCloseable.Open();
                Send("Ok.");
                Act(ActOptions.ToRoom, "{0:N} opens {1}.", this, itemCloseable);
                return CommandExecutionResults.Ok;
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
            Send("Ok.");
            Act(ActOptions.ToRoom, "{0:N} opens the {1}.", this, exit);

            // Open the other side
            IExit otherSideExit = exit.Destination[exitDirection.ReverseDirection()];
            if (otherSideExit != null)
            {
                otherSideExit.Open();
                Act(exit.Destination.People, "The {0} opens.", otherSideExit);
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("close", "Movement", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoClose(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Close what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Inventory.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemCloseable itemCloseable)
                {
                    if (!itemCloseable.IsCloseable)
                    {
                        Send("You can't do that.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    if (itemCloseable.IsClosed)
                    {
                        Send("It's already closed.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                }
                else
                {
                    Send("You can't do that.");
                    return CommandExecutionResults.InvalidTarget;
                }
                itemCloseable.Close();
                Send("Ok.");
                Act(ActOptions.ToRoom, "{0:N} closes {1}.", this, itemCloseable);
                return CommandExecutionResults.Ok;
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
            Send("Ok.");
            Act(ActOptions.ToRoom, "{0:N} closes {1}.", this, exit);

            // Close the other side
            IExit otherSideExit = exit.Destination[exitDirection.ReverseDirection()];
            if (otherSideExit != null)
            {
                otherSideExit.Close();
                Act(exit.Destination.People, "The {0} closes.", otherSideExit);
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("unlock", "Movement", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoUnlock(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Unlock what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Inventory.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemCloseable itemCloseable)
                {
                    if (!itemCloseable.IsCloseable)
                    {
                        Send("You can't do that.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    if (!itemCloseable.IsClosed)
                    {
                        Send("It's not closed.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    if (itemCloseable.KeyId <= 0)
                    {
                        Send("It can't be unlocked.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    bool closeableItemKeyFound = Inventory.OfType<IItemKey>().Any(x => x.Blueprint.Id == itemCloseable.KeyId);
                    if (!closeableItemKeyFound && (this as IPlayableCharacter)?.IsImmortal != true)
                    {
                        Send("You lack the key.");
                        return CommandExecutionResults.NoExecution;
                    }
                    if (!itemCloseable.IsLocked)
                    {
                        Send("It's already unlocked.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                }
                else
                {
                    Send("You can't do that.");
                    return CommandExecutionResults.InvalidTarget;
                }
                itemCloseable.Unlock();
                Send("*Click*");
                Act(ActOptions.ToRoom, "{0:N} unlocks {1}.", this, itemCloseable);
                return CommandExecutionResults.Ok;
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
            bool keyFound = Inventory.OfType<IItemKey>().Any(x => x.Blueprint.Id == exit.Blueprint.Key);
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
            IExit otherSideExit = exit.Destination[exitDirection.ReverseDirection()];
            if (otherSideExit != null)
                otherSideExit.Unlock();
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("lock", "Movement", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
        protected virtual CommandExecutionResults DoLock(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Lock what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindByName(
                Room.Content.Where(CanSee)
                .Concat(Inventory.Where(CanSee))
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item)),
                parameters[0]);
            if (item != null)
            {
                if (item is IItemCloseable itemCloseable)
                {
                    if (!itemCloseable.IsCloseable)
                    {
                        Send("You can't do that.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    if (!itemCloseable.IsClosed)
                    {
                        Send("It's not closed.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    if (!itemCloseable.IsLockable || itemCloseable.KeyId <= 0)
                    {
                        Send("It can't be locked.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                    bool closeableItemKeyFound = Inventory.OfType<IItemKey>().Any(x => x.Blueprint.Id == itemCloseable.KeyId);
                    if (!closeableItemKeyFound && (this as IPlayableCharacter)?.IsImmortal != true)
                    {
                        Send("You lack the key.");
                        return CommandExecutionResults.NoExecution;
                    }
                    if (itemCloseable.IsLocked)
                    {
                        Send("It's already locked.");
                        return CommandExecutionResults.InvalidTarget;
                    }
                }
                else
                {
                    Send("You can't do that.");
                    return CommandExecutionResults.InvalidTarget;
                }
                itemCloseable.Lock();
                Send("*Click*");
                Act(ActOptions.ToRoom, "{0:N} locks {1}.", this, itemCloseable);
                return CommandExecutionResults.Ok;
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
            bool keyFound = Inventory.OfType<IItemKey>().Any(x => x.Blueprint.Id == exit.Blueprint.Key);
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
            IExit otherSideExit = exit.Destination[exitDirection.ReverseDirection()];
            if (otherSideExit != null)
                otherSideExit.Lock();
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("stand", "Movement", MinPosition = Positions.Sleeping)]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoStand(string rawParameters, params ICommandParameter[] parameters)
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

        [CharacterCommand("sit", "Movement", MinPosition = Positions.Sleeping)]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoSit(string rawParameters, params ICommandParameter[] parameters)
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

        [CharacterCommand("rest", "Movement", MinPosition = Positions.Sleeping)]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoRest(string rawParameters, params ICommandParameter[] parameters)
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

        [CharacterCommand("sleep", "Movement", MinPosition = Positions.Sleeping)]
        [Syntax(
            "[cmd]",
            "[cmd] <furniture>")]
        protected virtual CommandExecutionResults DoSleep(string rawParameters, params ICommandParameter[] parameters)
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

        [CharacterCommand("wake", "Movement", MinPosition = Positions.Sleeping)]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        protected virtual CommandExecutionResults DoWake(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return DoStand(rawParameters, parameters);
            if (Position <= Positions.Sleeping)
            {
                Send("You are asleep yourself!");
                return CommandExecutionResults.NoExecution;
            }
            ICharacter victim = FindHelpers.FindByName(Room.People, parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            if (victim.Position > Positions.Sleeping)
            {
                Act(ActOptions.ToCharacter, "{0:N} is already awake.", victim);
                return CommandExecutionResults.InvalidTarget;
            }
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Sleep))
            {
                Act(ActOptions.ToCharacter, "You can't wake {0:m}!", victim);
                return CommandExecutionResults.InvalidTarget;
            }
            victim.Act(ActOptions.ToCharacter, "{0:N} wakes you.", this);
            //TODO victim.DoStand(string.Empty, Enumerable.Empty<ICommandParameter>().ToArray());
            return CommandExecutionResults.NoExecution;
        }

        [CharacterCommand("enter", "Movement", MinPosition = Positions.Standing)]
        [Syntax("[cmd] <portal>")]
        protected virtual CommandExecutionResults DoEnter(string rawParameters, params ICommandParameter[] parameters)
        {
            if (Fighting != null)
                return CommandExecutionResults.NoExecution;
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
            IItemPortal portal = item as IItemPortal;
            if (portal == null)
            {
                Send("You can't seem to find a way in.");
                return CommandExecutionResults.InvalidTarget;
            }

            // Let's go
            bool entered = Enter(portal);

            return entered 
                ? CommandExecutionResults.Ok
                : CommandExecutionResults.NoExecution;
        }

        [CharacterCommand("visible", "Movement", MinPosition = Positions.Sleeping)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoVisible(string rawParameters, params ICommandParameter[] parameter)
        {
            CharacterFlags &= ~CharacterFlags.Invisible;
            CharacterFlags &= ~CharacterFlags.Sneak;
            CharacterFlags &= ~CharacterFlags.Hide;
            RemoveAuras(x => x.AbilityName == "Invisibility"
                             || x.AbilityName == "Sneak"
                             || x.AbilityName == "Hide", true);
            Send("You are now visible");
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("follow", "Group", "Movement")]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        protected virtual CommandExecutionResults DoFollow(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Leader == null)
                {
                    Send("You are not following anyone.");
                    return CommandExecutionResults.NoExecution;
                }
                Act(ActOptions.ToCharacter, "You are following {0:N}.", Leader);
                return CommandExecutionResults.Ok;
            }

            // search target
            ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);
            if (target == null)
            {
                Send("They aren't here.");
                return CommandExecutionResults.TargetNotFound;
            }

            // follow ourself -> cancel follow
            if (target == this)
            {
                if (Leader == null)
                {
                    Send("You already follow yourself.");
                    return CommandExecutionResults.InvalidTarget;
                }

                Leader.RemoveFollower(this);
                return CommandExecutionResults.Ok;
            }

            // check cycle
            ICharacter next = target.Leader;
            while (next != null)
            {
                if (next == this)
                {
                    Act(ActOptions.ToCharacter, "You can't follow {0:N}.", target);
                    return CommandExecutionResults.InvalidTarget; // found a cycle
                }
                next = next.Leader;
            }

            target.Leader?.RemoveFollower(this);
            target.AddFollower(this);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("nofollow", "Group", "Movement")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoNofollow(string rawParameters, params ICommandParameter[] parameters)
        {
            foreach (ICharacter follower in World.Characters.Where(x => x.Leader == this))
                RemoveFollower(follower);

            return CommandExecutionResults.Ok;
        }

        // Helpers
        private IExit VerboseFindDoor(ICommandParameter parameter, out ExitDirections exitDirection)
        {
            bool wasAskingForDirection;
            bool found = FindDoor(parameter, out exitDirection, out wasAskingForDirection);
            if (!found)
            {
                //  if open north -> I see no door north here.
                //  if open black door -> I see no black door here.
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (wasAskingForDirection)
                    Send($"I see no door {parameter.Value} here.");
                else
                    Send($"I see no {parameter.Value} here.");
                return null;
            }
            IExit exit = Room[exitDirection];
            if (exit == null)
                return null;
            if (!exit.IsDoor)
            {
                Send("You can't do that.");
                return null;
            }
            return exit;
        }

        private bool FindDoor(ICommandParameter parameter, out ExitDirections exitDirection, out bool wasAskingForDirection)
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
                IExit exit = Room[direction];
                if (exit?.Destination != null && exit.IsDoor && exit.Keywords.Any(k => StringCompareHelpers.StringStartsWith(k, parameter.Value)))
                {
                    exitDirection = direction;
                    return true;
                }
            }
            return false;
        }
    }
}
