using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("stat", "Information")]
    public class Stat : AdminGameAction
    {
        private IAreaManager AreaManager { get; }
        private IRoomManager RoomManager { get; }
        private ICharacterManager CharacterManager { get; }
        private IItemManager ItemManager { get; }
        private IQuestManager QuestManager { get; }
        private IAdminManager AdminManager { get; }
        private IPlayerManager PlayerManager { get; }

        public Stat(IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IQuestManager questManager, IAdminManager adminManager, IPlayerManager playerManager)
        {
            AreaManager = areaManager;
            RoomManager = roomManager;
            CharacterManager = characterManager;
            ItemManager = itemManager;
            QuestManager = questManager;
            AdminManager = adminManager;
            PlayerManager = playerManager;
        }

        public override void Execute(IActionInput actionInput)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine("#Admins: {0}", AdminManager.Admins.Count());
            sb.AppendFormatLine("#Players: {0}", PlayerManager.Players.Count());
            sb.AppendFormatLine("#Areas: {0}", AreaManager.Areas.Count());
            sb.AppendLine("Blueprints:");
            sb.AppendFormatLine("   #Areas: {0}", AreaManager.AreaBlueprints.Count);
            sb.AppendFormatLine("   #Rooms: {0}", RoomManager.RoomBlueprints.Count);
            sb.AppendFormatLine("   #Characters: {0}", CharacterManager.CharacterBlueprints.Count);
            sb.AppendFormatLine("   #Items: {0}", ItemManager.ItemBlueprints.Count);
            sb.AppendFormatLine("   #Quests: {0}", QuestManager.QuestBlueprints.Count);
            sb.AppendLine("Entities:");
            sb.AppendFormatLine("   #Rooms: {0}", RoomManager.Rooms.Count());
            sb.AppendFormatLine("   #Characters: {0}", CharacterManager.Characters.Count());
            sb.AppendFormatLine("   #NPC: {0}", CharacterManager.NonPlayableCharacters.Count());
            sb.AppendFormatLine("   #PC: {0}", CharacterManager.PlayableCharacters.Count());
            sb.AppendFormatLine("   #Items: {0}", ItemManager.Items.Count());
            Actor.Send(sb);
        }
    }
}
