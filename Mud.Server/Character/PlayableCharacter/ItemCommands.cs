using Mud.Domain;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("destroy", "Item", Priority = 50, NoShortcut = true, MinPosition = Positions.Standing)]
        [Syntax("[cmd] <item>")]
        // Destroy item
        protected virtual CommandExecutionResults DoDestroy(string rawParameters, params CommandParameter[] parameters)
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

            World.RemoveItem(item);
            Recompute();

            return CommandExecutionResults.Ok;
        }
    }
}
