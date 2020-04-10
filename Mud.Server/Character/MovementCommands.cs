using System.Linq;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("north", Category = "Movement", Priority = 0)]
        protected virtual bool DoNorth(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.North, true);
        }

        [Command("east", Category = "Movement", Priority = 0)]
        protected virtual bool DoEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.East, true);
        }

        [Command("south", Category = "Movement", Priority = 0)]
        protected virtual bool DoSouth(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.South, true);
        }

        [Command("west", Category = "Movement", Priority = 0)]
        protected virtual bool DoWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.West, true);
        }

        [Command("up", Category = "Movement", Priority = 0)]
        protected virtual bool DoUp(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.Up, true);
        }

        [Command("down", Category = "Movement", Priority = 0)]
        protected virtual bool DoDown(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.Down, true);
        }

        [Command("northeast", Category = "Movement", Priority = 1)]
        [Command("ne", Category = "Movement", Priority = 0)]
        protected virtual bool DoNorthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.NorthEast, true);
        }

        [Command("northwest", Category = "Movement", Priority = 1)]
        [Command("nw", Category = "Movement", Priority = 0)]
        protected virtual bool DoNorthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.NorthWest, true);
        }

        [Command("southeast", Category = "Movement", Priority = 1)]
        [Command("se", Category = "Movement", Priority = 0)]
        protected virtual bool DoSouthEast(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.SouthEast, true);
        }

        [Command("southwest", Category = "Movement", Priority = 1)]
        [Command("sw", Category = "Movement", Priority = 0)]
        protected virtual bool DoSouthWest(string rawParameters, params CommandParameter[] parameters)
        {
            return Move(ExitDirections.SouthWest, true);
        }

        [Command("open", Category = "Movement")]
        protected virtual bool DoOpen(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Open what?");
                return true;
            }
            // TODO: search item

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return true;
            if (!exit.IsClosed)
            {
                Send("It's already open.");
                return true;
            }
            if (exit.IsLocked)
            {
                Send("It's locked.");
                return true;
            }

            // Open this side side
            exit.Open();
            Act(ActOptions.ToRoom, "{0:N} opens the {1}.", this, exit);
            Send("Ok.");

            // Open the other side
            IExit otherSideExit = exit.Destination.Exit(ExitHelpers.ReverseDirection(exitDirection));
            if (otherSideExit != null)
            {
                otherSideExit.Open();
                Act(exit.Destination.People, "The {0} opens.", otherSideExit);
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");
            return true;
        }

        [Command("close", Category = "Movement")]
        protected virtual bool DoClose(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Close what?");
                return true;
            }
            // TODO: search item

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return true;
            if (exit.IsClosed)
            {
                Send("It's already closed.");
                return true;
            }

            // Close this side
            exit.Close();
            Act(ActOptions.ToRoom, "{0:N} closes {1}.", this, exit);
            Send("Ok.");

            // Close the other side
            IExit otherSideExit = exit.Destination.Exit(ExitHelpers.ReverseDirection(exitDirection));
            if (otherSideExit != null)
            {
                otherSideExit.Close();
                Act(exit.Destination.People, "The {0} closes.", otherSideExit);
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");
            return true;
        }

        [Command("unlock", Category = "Movement")]
        protected virtual bool DoUnlock(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Unlock what?");
                return true;
            }

            // TODO: search item

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return true;
            if (!exit.IsClosed)
            {
                Send("It's not closed.");
                return true;
            }
            if (exit.Blueprint.Key <= 0)
            {
                Send("It can't be unlocked.");
                return true;
            }
            // search key
            bool keyFound = Content.OfType<IItemKey>().Any(x => x.Blueprint.Id == exit.Blueprint.Key);
            if (!keyFound)
            {
                Send("You lack the key.");
                return true;
            }
            if (!exit.IsLocked)
            {
                Send("It's already unlocked.");
                return true;
            }
            // Unlock this side
            exit.Unlock();
            Send("*Click*");
            Act(ActOptions.ToRoom, "{0:N} unlocks the {1}.", this, exit);

            // Unlock other side
            IExit otherSideExit = exit.Destination.Exit(ExitHelpers.ReverseDirection(exitDirection));
            if (otherSideExit != null)
                otherSideExit.Unlock();
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");

            return true;
        }

        [Command("lock", Category = "Movement")]
        protected virtual bool DoLock(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Lock what?");
                return true;
            }

            // TODO: search item

            // No item found, search door
            ExitDirections exitDirection;
            IExit exit = VerboseFindDoor(parameters[0], out exitDirection);
            if (exit == null)
                return true;
            if (!exit.IsClosed)
            {
                Send("It's not closed.");
                return true;
            }
            if (exit.Blueprint.Key <= 0)
            {
                Send("It can't be locked.");
                return true;
            }
            // search key
            bool keyFound = Content.OfType<IItemKey>().Any(x => x.Blueprint.Id == exit.Blueprint.Key);
            if (!keyFound)
            {
                Send("You lack the key.");
                return true;
            }
            if (exit.IsLocked)
            {
                Send("It's already locked.");
                return true;
            }
            // Unlock this side
            exit.Lock();
            Send("*Click*");
            Act(ActOptions.ToRoom, "{0:N} locks the {1}.", this, exit);

            // Unlock other side
            IExit otherSideExit = exit.Destination.Exit(ExitHelpers.ReverseDirection(exitDirection));
            if (otherSideExit != null)
                otherSideExit.Lock();
            else
                Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Room.Blueprint.Id} direction {exitDirection}");

            return true;
        }

        [Command("stand", Category = "Movement")]
        protected virtual bool DoStand(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return true;
            }
            if (Position == Positions.Standing)
            {
                Send("You are already standing.");
                return true;
            }

            // Search valid furniture if any
            IItemFurniture furniture = null;
            if (parameters.Length > 0)
            {
                furniture = FindHelpers.FindByName(Room.Content.OfType<IItemFurniture>().Where(CanSee), parameters[0]); // TODO: search for IItem than cast it to ItemFurniture?
                if (furniture == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return true;
                }
                if ((furniture.FurnitureActions & FurnitureActions.Stand) != FurnitureActions.Stand)
                {
                    Send("You can't seem to find a place to stand.");
                    return true;
                }

                if (1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room to stand on {0}.", furniture);
                    return true;
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
                if (ImpersonatedBy != null || IncarnatedBy != null)
                    DisplayRoom();
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

            return true;
        }

        [Command("sit", Category = "Movement")]
        protected virtual bool DoSit(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return true;
            }
            if (Position == Positions.Sitting)
            {
                Send("You are already sitting.");
                return true;
            }

            // Search valid furniture if any
            IItemFurniture furniture = null;
            if (parameters.Length > 0)
            {
                furniture = FindHelpers.FindByName(Room.Content.OfType<IItemFurniture>().Where(CanSee), parameters[0]); // TODO: search for IItem than cast it to ItemFurniture?
                if (furniture == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return true;
                }
                if ((furniture.FurnitureActions & FurnitureActions.Sit) != FurnitureActions.Sit)
                {
                    Send("You can't sit on that.");
                    return true;
                }

                if (1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room on {0}.", furniture);
                    return true;
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

            return true;
        }

        [Command("rest", Category = "Movement")]
        protected virtual bool DoRest(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return true;
            }
            if (Position == Positions.Resting)
            {
                Send("You are already resting.");
                return true;
            }

            // Search valid furniture if any
            IItemFurniture furniture = null;
            if (parameters.Length > 0)
            {
                furniture = FindHelpers.FindByName(Room.Content.OfType<IItemFurniture>().Where(CanSee), parameters[0]); // TODO: search for IItem than cast it to ItemFurniture?
                if (furniture == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return true;
                }
                if ((furniture.FurnitureActions & FurnitureActions.Rest) != FurnitureActions.Rest)
                {
                    Send("You can't rest on that.");
                    return true;
                }

                if (1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room on {0}.", furniture);
                    return true;
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

            return true;
        }

        [Command("sleep", Category = "Movement")]
        protected virtual bool DoSleep(string rawParameters, params CommandParameter[] parameters)
        {
            if (Position == Positions.Fighting)
            {
                Send("Maybe you should finish fighting first?");
                return true;
            }
            if (Position == Positions.Sleeping)
            {
                Send("You are already sleeping.");
                return true;
            }

            // If already on a furniture and no parameter specified, use that furniture
            // Search valid furniture if any
            IItemFurniture furniture;
            if (parameters.Length != 0)
            {
                furniture = FindHelpers.FindByName(Room.Content.OfType<IItemFurniture>().Where(CanSee), parameters[0]); // TODO: search for IItem than cast it to ItemFurniture?
                if (furniture == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return true;
                }
            }
            else
                furniture = Furniture;

            // Check furniture validity
            if (furniture != null)
            {
                if ((furniture.FurnitureActions & FurnitureActions.Sleep) != FurnitureActions.Sleep)
                {
                    Send("You can't sleep on that.");
                    return true;
                }

                // If already on furniture, don't count
                if (furniture != Furniture && 1 + furniture.People.Count() > furniture.MaxPeople)
                {
                    Act(ActOptions.ToCharacter, "There is no more room on {0} for you.", furniture);
                    return true;
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

            return true;
        }

        [Command("enter", Category = "Movement")]
        protected virtual bool DoEnter(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Nope, can't do it.");
                return true;
            }

            IItem item = FindHelpers.FindItemHere(this, parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemNotFound);
                return true;
            }
            IItemPortal portal = item as IItemPortal;
            if (portal == null)
            {
                Send("You can't seem to find a way in.");
                return true;
            }

            return Enter(portal);
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
            if (ExitHelpers.FindDirection(parameter.Value, out exitDirection))
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
