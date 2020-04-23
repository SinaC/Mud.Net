using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {

        [PlayableCharacterCommand("destroy", Category = "Item", Priority = 50, NoShortcut = true)]
        [Syntax("[cmd] <item>")]
        // Destroy item
        protected virtual bool DoDestroy(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Destroy what?");
                return true;
            }
            IItem item = FindHelpers.FindByName(Content.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
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
            World.RemoveItem(item);
            RecomputeAttributes();

            Send($"You destroy {item.RelativeDisplayName(this)}.");
            return true;
        }
    }
}
