﻿using Mud.Common;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("resets", "Information", Priority = 50)]
    [Syntax(
    "[cmd] <id>",
    "[cmd] (if impersonated)")]
    public class Resets : AdminGameAction
    {
        private IRoomManager RoomManager { get; }
        private ICharacterManager CharacterManager { get; }
        private IItemManager ItemManager { get; }

        public IRoom Room { get; protected set; }

        public Resets(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
        {
            RoomManager = roomManager;
            CharacterManager = characterManager;
            ItemManager = itemManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0 && Impersonating == null)
                return BuildCommandSyntax();

            if (actionInput.Parameters.Length >= 1 && !actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();

            IRoom room;
            if (Impersonating != null)
                room = Impersonating.Room;
            else
            {
                int id = actionInput.Parameters[0].AsNumber;
                room = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            }
            if (room == null)
                return "It doesn't exist.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Room.Blueprint.Resets == null || !Room.Blueprint.Resets.Any())
            {
                Actor.Send("No resets.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" No.  Loads    Description       Location         Vnum    Max   Description");
            sb.AppendLine("==== ======== ============= =================== ======== [R  W] ===========");
            int resetId = 0;
            CharacterBlueprintBase previousCharacter = null;
            ItemBlueprintBase previousItem = null;
            foreach (ResetBase reset in Room.Blueprint.Resets)
            {
                sb.AppendFormat("[{0,2}] ", resetId);
                switch (reset)
                {
                    case CharacterReset characterReset: // 'M'
                        {
                            CharacterBlueprintBase characterBlueprint = CharacterManager.GetCharacterBlueprint(characterReset.CharacterId);
                            if (characterBlueprint == null)
                            {
                                sb.AppendFormatLine("Load Char - Bad Character {0}", characterReset.CharacterId);
                                continue;
                            }

                            RoomBlueprint roomBlueprint = RoomManager.GetRoomBlueprint(characterReset.RoomId);
                            if (roomBlueprint == null)
                            {
                                sb.AppendFormatLine("Load Char - Bad Room {0}", characterReset.RoomId);
                                continue;
                            }

                            previousCharacter = characterBlueprint;
                            sb.AppendFormatLine("M[{0,5}] {1,-13} in room             R[{2,5}] [{3,-2}{4,2}] {5,-15}", characterReset.CharacterId, characterBlueprint.ShortDescription.MaxLength(13), characterReset.RoomId, characterReset.LocalLimit, characterReset.GlobalLimit, roomBlueprint.Name.MaxLength(15));
                            break;
                        }

                    case ItemInRoomReset itemInRoomReset: // 'O'
                        {
                            ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInRoomReset.ItemId);
                            if (itemBlueprint == null)
                            {
                                sb.AppendFormatLine("Load Item - Bad Item {0}", itemInRoomReset.ItemId);
                                continue;
                            }

                            previousItem = itemBlueprint;

                            RoomBlueprint roomBlueprint = RoomManager.GetRoomBlueprint(itemInRoomReset.RoomId);
                            if (roomBlueprint == null)
                            {
                                sb.AppendFormatLine("Load Item - Bad Room {0}", itemInRoomReset.RoomId);
                                continue;
                            }

                            sb.AppendFormatLine("O[{0,5}] {1,-13} in room             R[{2,5}] [{3,-2}{4,2}] {5,-15}", itemInRoomReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), itemInRoomReset.RoomId, itemInRoomReset.LocalLimit, itemInRoomReset.GlobalLimit, roomBlueprint.Name.MaxLength(15));
                            break;
                        }

                    case ItemInItemReset itemInItemReset: // 'P'
                        {
                            ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInItemReset.ItemId);
                            if (itemBlueprint == null)
                            {
                                sb.AppendFormatLine("Put Item - Bad Item {0}", itemInItemReset.ItemId);
                                continue;
                            }

                            previousItem = itemBlueprint;

                            ItemBlueprintBase containerBlueprint = ItemManager.GetItemBlueprint(itemInItemReset.ContainerId);
                            if (containerBlueprint == null)
                            {
                                sb.AppendFormatLine("Put Item - Bad To Item {0}", itemInItemReset.ContainerId);
                                continue;
                            }

                            sb.AppendFormatLine("O[{0,5}] {1,-13} inside              O[{2,5}]       {3,-15}", itemInItemReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), itemInItemReset.ContainerId, containerBlueprint.ShortDescription.MaxLength(15));
                            break;
                        }

                    case ItemInCharacterReset itemInCharacterReset: // 'G'
                        {
                            ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInCharacterReset.ItemId);
                            if (itemBlueprint == null)
                            {
                                sb.AppendFormatLine("Give Item - Bad Item {0}", itemInCharacterReset.ItemId);
                                continue;
                            }

                            previousItem = itemBlueprint;

                            if (previousCharacter == null)
                            {
                                sb.AppendFormatLine("Give Item - No Previous Character");
                                continue;
                            }

                            sb.AppendFormatLine("O[{0,5}] {1,-13} {2,-19} M[{3,5}]       {4,-15}", itemInCharacterReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), "in the inventory", previousCharacter.Id, previousCharacter.ShortDescription.MaxLength(15));
                            break;
                        }

                    case ItemInEquipmentReset itemInEquipmentReset: // 'E'
                        {
                            ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInEquipmentReset.ItemId);
                            if (itemBlueprint == null)
                            {
                                sb.AppendFormatLine("Equip Item - Bad Item {0}", itemInEquipmentReset.ItemId);
                                continue;
                            }

                            previousItem = itemBlueprint;

                            if (previousCharacter == null)
                            {
                                sb.AppendFormatLine("Equip Item - No Previous Character");
                                continue;
                            }

                            sb.AppendFormatLine("O[{0,5}] {1,-13} {2,-19} M[{3,5}]       {4,-15}", itemInEquipmentReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), itemInEquipmentReset.EquipmentSlot, previousCharacter.Id, previousCharacter.ShortDescription.MaxLength(15));
                            break;
                        }

                    default:
                        sb.AppendFormatLine("Bad reset command: {0}.", reset.GetType());
                        break;
                }
                resetId++;
            }
            Actor.Send(sb);
        }
    }
}
