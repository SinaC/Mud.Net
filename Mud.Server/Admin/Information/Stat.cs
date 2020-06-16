using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("stat", "Information")]
    public class Stat : AdminGameAction
    {
        private IWorld World { get; }
        private IRoomManager RoomManager { get; }
        private IItemManager ItemManager { get; }
        private IAdminManager AdminManager { get; }
        private IPlayerManager PlayerManager { get; }

        public Stat(IWorld world, IRoomManager roomManager, IItemManager itemManager, IAdminManager adminManager, IPlayerManager playerManager)
        {
            World = world;
            RoomManager = roomManager;
            ItemManager = itemManager;
            AdminManager = adminManager;
            PlayerManager = playerManager;
        }

        public override void Execute(IActionInput actionInput)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine("#Admins: {0}", AdminManager.Admins.Count());
            sb.AppendFormatLine("#Players: {0}", PlayerManager.Players.Count());
            sb.AppendFormatLine("#Areas: {0}", World.Areas.Count());
            sb.AppendLine("Blueprints:");
            sb.AppendFormatLine("   #Areas: {0}", World.AreaBlueprints.Count);
            sb.AppendFormatLine("   #Rooms: {0}", RoomManager.RoomBlueprints.Count);
            sb.AppendFormatLine("   #Characters: {0}", World.CharacterBlueprints.Count);
            sb.AppendFormatLine("   #Items: {0}", ItemManager.ItemBlueprints.Count);
            sb.AppendFormatLine("   #Quests: {0}", World.QuestBlueprints.Count);
            sb.AppendLine("Entities:");
            sb.AppendFormatLine("   #Rooms: {0}", RoomManager.Rooms.Count());
            sb.AppendFormatLine("   #Characters: {0}", World.Characters.Count());
            sb.AppendFormatLine("   #NPC: {0}", World.NonPlayableCharacters.Count());
            sb.AppendFormatLine("   #PC: {0}", World.PlayableCharacters.Count());
            sb.AppendFormatLine("   #Items: {0}", ItemManager.Items.Count());
            Actor.Send(sb);
        }
    }
}
