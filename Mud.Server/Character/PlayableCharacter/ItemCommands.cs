using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Helpers;
using System.Linq;
using Mud.Server.Common;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Quest;
using Mud.Common;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.GameAction;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("destroy", "Item", Priority = 50, NoShortcut = true, MinPosition = Positions.Standing)]
        [Syntax("[cmd] <item>")]
        // Destroy item
        protected virtual CommandExecutionResults DoDestroy(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Destroy what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            // Remove from inventory
            item.ChangeContainer(null);
            // Update quest if needed
            if (item is IItemQuest itemQuest)
            {
                foreach (IQuest quest in Quests)
                    quest.Update(itemQuest, true);
            }
            //
            Log.Default.WriteLine(LogLevels.Debug, "Manually destroying item {0} in {1}", item.DebugName, DebugName);
            Send($"You destroy {item.RelativeDisplayName(this)}.");

            ItemManager.RemoveItem(item);
            Recompute();

            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("tap", "Item", MinPosition = Positions.Standing)]
        [PlayableCharacterCommand("junk", "Item", MinPosition = Positions.Standing)]
        [PlayableCharacterCommand("sacrifice", "Item", MinPosition = Positions.Standing)]
        [Syntax(
            "[cmd] all",
            "[cmd] <item>")]
        protected virtual CommandExecutionResults DoSacrifice(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0 || StringCompareHelpers.StringEquals(Name, parameters[0].Value))
            {
                Act(ActOptions.ToRoom, "{0:N} offers {0:f} to Mota, who graciously declines.", this);
                Send("Mota appreciates your offer and may accept it later.");
                return CommandExecutionResults.NoExecution;
            }

            IEnumerable<IItem> items;
            if (!parameters[0].IsAll)
            {
                IItem item = FindHelpers.FindByName(Room.Content, parameters[0]);
                if (item == null)
                {
                    Send(StringHelpers.CantFindIt);
                    return CommandExecutionResults.TargetNotFound;
                }

                items = item.Yield();
            }
            else
                items = Room.Content.Where(CanSee);

            IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(items.ToList());
            foreach (IItem item in clone)
                SacrificeItem(item);

            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("split", "Item", MinPosition = Positions.Standing, Priority = 600)]
        [Syntax("[cmd] <silver amount> <gold amount>")]
        protected virtual CommandExecutionResults DoSplit(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Split how much?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            long amountSilver = 0, amountGold = 0;
            if (parameters.Length > 0)
                amountSilver = parameters[0].AsLong;
            if (parameters.Length > 1)
                amountGold = parameters[1].AsLong;

            if (amountSilver < 0 || amountGold < 0)
            {
                Send("Your group wouldn't like that.");
                return CommandExecutionResults.InvalidParameter;
            }

            if (amountSilver == 0 && amountGold == 0)
            {
                Send("You hand out zero coins, but no one notices.");
                return CommandExecutionResults.InvalidParameter;
            }

            if (SilverCoins < amountSilver || GoldCoins < amountGold)
            {
                Send("You don't have that much to split.");
                return CommandExecutionResults.InvalidParameter;
            }

            IPlayableCharacter[] members = (Group?.Members ?? this.Yield()).ToArray();
            if (members.Length < 2)
            {
                Send("Just keep it all.");
                return CommandExecutionResults.NoExecution;
            }

            bool split = SplitMoney(amountSilver, amountGold);

            return split
                ? CommandExecutionResults.Ok
                : CommandExecutionResults.NoExecution;
        }

        protected bool SacrificeItem(IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.NoSacrifice) || item.NoTake)
            {
                Act(ActOptions.ToCharacter, "{0} is not an acceptable sacrifice.", item);
                return false;
            }
            if (item is IItemCorpse itemCorpse && itemCorpse.IsPlayableCharacterCorpse && itemCorpse.Content.Any())
            {
                Send("Mota wouldn't like that.");
                return false;
            }
            if (item is IItemFurniture itemFurniture)
            {
                ICharacter user = itemFurniture.People.FirstOrDefault();
                if (user != null)
                {
                    Act(ActOptions.ToCharacter, "{0:N} appears to be using {1}.", user, itemFurniture);
                    return false;
                }
            }

            Act(ActOptions.ToAll, "{0:N} sacrifices {1:v} to Mota.", this, item);
            Wiznet.Wiznet($"{DebugName} sacrifices {item.DebugName} as a burnt offering.", WiznetFlags.Saccing);
            ItemManager.RemoveItem(item);
            //
            long silver = Math.Max(1, item.Level * 3);
            if (!(item is IItemCorpse))
                silver = Math.Min(silver, item.Cost);
            if (silver <= 0)
            {
                Send("Mota doesn't give you anything for your sacrifice.");
                Wiznet.Wiznet($"DoSacrifice: {item.DebugName} gives zero or negative money {silver}!", WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            else if (silver == 1)
                Send("Mota gives you one silver coin for your sacrifice.");
            else
                Send("Mota gives you {0} silver coins for your sacrifice.", silver);
            if (silver > 0)
                SilverCoins += silver;
            // autosplit
            if (silver > 0 && AutoFlags.HasFlag(AutoFlags.Split))
                SplitMoney(silver, 0);

            return true;
        }

        protected bool SplitMoney(long amountSilver, long amountGold)
        {
            IPlayableCharacter[] members = (Group?.Members ?? this.Yield()).ToArray();
            if (members.Length < 2)
                return false;
            long extraSilver = Math.DivRem(amountSilver, members.Length, out long shareSilver);
            long extraGold = Math.DivRem(amountGold, members.Length, out long shareGold);
            if (shareSilver == 0 || shareGold == 0)
            {
                Send("Don't even bother, cheapstake.");
                return false;
            }
            // Remove money from ours, extra money excluded
            if (shareSilver > 0)
                Send("You split {0} silver coins. Your share is {1} silver.", amountSilver, shareSilver+extraSilver);
            if (shareGold > 0)
                Send("You split {0} gold coins. Your share is {1} gold.", amountGold, shareGold + extraGold);
            UpdateMoney(-amountSilver+extraSilver, -amountGold+extraGold);
            UpdateMoney(extraSilver, extraGold);
            // Give share money to group member (including ourself)
            foreach (IPlayableCharacter member in members)
            {
                if (member != this)
                {
                    if (shareGold == 0)
                        Act(ActOptions.ToCharacter, "{0:N} splits {1} silver coins. Your share is {2} silver.", this, amountSilver, shareSilver);
                    else if (shareSilver == 0)
                        Act(ActOptions.ToCharacter, "{0:N} splits {1} gold coins. Your share is {2} gold.", this, amountGold, shareGold);
                    else
                        Act(ActOptions.ToCharacter, "{0:N} splits {1} silver and {2} gold coins, giving you {3} silver and {4} gold.", this, amountSilver, amountGold, shareSilver, shareGold);
                }
                UpdateMoney(shareSilver, shareGold);
            }

            return true;
        }
    }
}
