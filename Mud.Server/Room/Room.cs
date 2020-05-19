using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Aura;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Entity;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Room
{
    public class Room : EntityBase, IRoom
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> RoomCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(GetCommands<Room>);

        private ITimeManager TimeManager => DependencyContainer.Current.GetInstance<ITimeManager>();

        private readonly List<ICharacter> _people;
        private readonly List<IItem> _content;

        public Room(Guid guid, RoomBlueprint blueprint, IArea area)
            : base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;
            _people = new List<ICharacter>();
            _content = new List<IItem>();
            Exits = new IExit[EnumHelpers.GetCount<ExitDirections>()];

            BaseRoomFlags = blueprint.RoomFlags;
            SectorType = blueprint.SectorType;
            HealRate = blueprint.HealRate;
            ResourceRate = blueprint.ResourceRate;
            MaxSize = blueprint.MaxSize;

            Area = area;
            Area.AddRoom(this);
        }

        #region IRoom

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => RoomCommands.Value;

        #endregion

        public override string DisplayName => Name.UpperFirstLetter();

        public override string DebugName => $"{DisplayName}[{Blueprint.Id}]";

        // Recompute
        public override void Recompute()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Room.Recompute: {0}", DebugName);

            // 0) Reset
            ResetAttributes();

            // 1) Apply own auras
            ApplyAuras(this);

            // 2) Apply people auras
            foreach (ICharacter character in People)
                ApplyAuras(character);

            // 3) Apply content auras
            foreach (IItem item in Content)
                ApplyAuras(item);
        }

        //
        public override void OnRemoved()
        {
            base.OnRemoved();
            Blueprint = null;
            _people.Clear();
            for (int i = 0; i < Exits.Length; i++)
                Exits[i] = null;
            _content.Clear();
        }

        #endregion

        #region IContainer

        public IEnumerable<IItem> Content => _content.Where(x => x.IsValid);

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container
            _content.Add(obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            bool removed = _content.Remove(obj);
            return removed;
        }

        #endregion

        public RoomBlueprint Blueprint { get; private set; }

        public ILookup<string, string> ExtraDescriptions => Blueprint.ExtraDescriptions;

        public RoomFlags BaseRoomFlags { get; protected set; }
        public RoomFlags RoomFlags { get; protected set; }

        public IArea Area { get; }

        public IEnumerable<ICharacter> People => _people.Where(x => x.IsValid);

        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => People.OfType<INonPlayableCharacter>();

        public IEnumerable<IPlayableCharacter> PlayableCharacters => People.OfType<IPlayableCharacter>();

        public IEnumerable<(INonPlayableCharacter character, TBlueprint blueprint)> GetNonPlayableCharacters<TBlueprint>()
            where TBlueprint : CharacterBlueprintBase
        {
            foreach (var character in NonPlayableCharacters.Where(x => x.Blueprint is TBlueprint))
                yield return (character, character.Blueprint as TBlueprint);
        }

        public Sizes? MaxSize { get; }

        public int HealRate { get; }

        public int ResourceRate { get; }

        public int Light { get; protected set; }

        public SectorTypes SectorType { get; }

        public bool IsPrivate
        {
            get
            {
                // TODO: ownership
                int count = People.Count();
                if (RoomFlags.HasFlag(RoomFlags.Private) && count >= 2)
                    return true;
                if (RoomFlags.HasFlag(RoomFlags.Solitary) && count >= 1)
                    return true;
                if (RoomFlags.HasFlag(RoomFlags.ImpOnly))
                    return true;
                return false;
            }
        }

        public bool IsDark
        {
            get
            {
                if (Light > 0)
                    return false;
                if (RoomFlags.HasFlag(RoomFlags.Dark))
                    return true;
                if (SectorType == SectorTypes.Inside
                    || SectorType == SectorTypes.City
                    || RoomFlags.HasFlag(RoomFlags.Indoors))
                    return false;
                if (TimeManager.SunPhase == SunPhases.Set
                    || TimeManager.SunPhase == SunPhases.Dark)
                    return true;
                return false;
            }
        }

        public IExit[] Exits { get; }

        public IExit this[ExitDirections direction] => Exits[(int) direction];

        public IRoom GetRoom(ExitDirections direction)
        {
            IExit exit = this[direction];
            return exit?.Destination;
        }

        public bool Enter(ICharacter character)
        {
            if (_people.Contains(character))
                Wiznet.Wiznet($"IRoom.Enter: Character {character.DebugName} is already in Room {character.Room.DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);
            else
                _people.Add(character);
            // Update light
            IItemLight light = character.GetEquipment<IItemLight>(EquipmentSlots.Light);
            if (light != null
                && light.IsLighten)
                Light++;
            // Update location quest
            if (character is IPlayableCharacter playableCharacter)
            {
                foreach(IQuest quest in playableCharacter.Quests)
                    quest.Update(this);
            }
            return true;
        }

        public bool Leave(ICharacter character)
        {
            // Update light
            IItemLight light = character.GetEquipment<IItemLight>(EquipmentSlots.Light);
            if (light != null
                && light.IsLighten
                && Light > 0)
                Light--;

            // TODO: check if in room
            bool removed = _people.Remove(character);
            return removed;
        }

        public void IncreaseLight()
        {
            Light++;
        }

        public void DecreaseLight()
        {
            Light = Math.Max(0, Light - 1);
        }

        public void ResetRoom()
        {
            INonPlayableCharacter lastCharacter = null;
            bool isLastCharacter = false;
            foreach (ResetBase reset in Blueprint.Resets)
            {
                switch (reset)
                {
                    case CharacterReset characterReset: // 'M'
                    {
                        CharacterBlueprintBase blueprint = World.GetCharacterBlueprint(characterReset.CharacterId);
                        if (blueprint != null)
                        {
                            int globalCount = characterReset.LocalLimit == -1 ? int.MinValue : World.NonPlayableCharacters.Count(x => x.Blueprint.Id == characterReset.CharacterId);
                            if (globalCount < characterReset.GlobalLimit)
                            {
                                int localCount = characterReset.LocalLimit == -1 ? int.MinValue : NonPlayableCharacters.Count(x => x.Blueprint.Id == characterReset.CharacterId);
                                if (localCount >= characterReset.LocalLimit)
                                    isLastCharacter = false;
                                if (localCount < characterReset.LocalLimit)
                                {
                                    lastCharacter = World.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, this);
                                    Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: M: Mob {characterReset.CharacterId} added");
                                    isLastCharacter = true;
                                }
                            }
                        }
                        else
                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: M: Mob {characterReset.CharacterId} not found");

                        break;
                    }

                    case ItemInRoomReset itemInRoomReset: // 'O'
                    {
                        ItemBlueprintBase blueprint = World.GetItemBlueprint(itemInRoomReset.ItemId);
                        if (blueprint != null)
                        {
                            // Global limit is not used in stock rom2.4
                            //int globalCount = itemInRoomReset.GlobalLimit == -1 ? int.MinValue : World.Items.Count(x => x.Blueprint.Id == itemInRoomReset.ItemId);
                            //if (globalCount < itemInRoomReset.GlobalLimit)
                            {
                                int localCount = itemInRoomReset.LocalLimit == -1 ? int.MinValue : Content.Count(x => x.Blueprint.Id == itemInRoomReset.ItemId);
                                if (localCount < itemInRoomReset.LocalLimit)
                                {
                                    IItem item = World.AddItem(Guid.NewGuid(), blueprint.Id, this);
                                    if (item != null)
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: O: Obj {itemInRoomReset.ItemId} added room");
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: O: Obj {itemInRoomReset.ItemId} not created");
                                }
                            }
                        }
                        else
                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: O: Obj {itemInRoomReset.ItemId} not found");

                        break;
                    }

                    case ItemInItemReset itemInItemReset: // 'P'
                    {
                        ItemBlueprintBase blueprint = World.GetItemBlueprint(itemInItemReset.ItemId);
                        if (blueprint != null)
                        {
                            // Global limit is not used in stock rom2.4
                            //int globalCount = itemInItemReset.GlobalLimit == -1 ? int.MinValue : World.Items.Count(x => x.Blueprint.Id == itemInItemReset.ItemId);
                            //if (globalCount < itemInItemReset.GlobalLimit)
                            {
                                ItemBlueprintBase containerBlueprint = World.GetItemBlueprint(itemInItemReset.ContainerId);
                                if (containerBlueprint != null)
                                {
                                    if (containerBlueprint is ItemContainerBlueprint)
                                    {
                                        IItemContainer container = Content.OfType<IItemContainer>().LastOrDefault(x => x.Blueprint.Id == containerBlueprint.Id); // search container in room in stock rom 2.4 it was search in the world)
                                        if (container != null)
                                        {
                                            int localLimit = itemInItemReset.LocalLimit == -1 ? int.MinValue : container.Content.Count(x => x.Blueprint.Id == itemInItemReset.ItemId);
                                            if (localLimit < itemInItemReset.LocalLimit)
                                            {
                                                World.AddItem(Guid.NewGuid(), blueprint.Id, container);
                                                Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: P: Obj {itemInItemReset.ItemId} added in {container.Blueprint.Id}");
                                            }
                                        }
                                        else
                                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: P: Container Obj {itemInItemReset.ContainerId} not found in room");
                                    }
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: P: Container Obj {itemInItemReset.ContainerId} is not a container");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: P: Container Obj {itemInItemReset.ContainerId} not found");
                            }
                        }
                        else
                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: P: Obj {itemInItemReset.ItemId} not found");

                        break;
                    }

                    case ItemInCharacterReset itemInCharacterReset: // 'G'
                    {
                        ItemBlueprintBase blueprint = World.GetItemBlueprint(itemInCharacterReset.ItemId);
                        if (blueprint != null)
                        {
                            if (isLastCharacter)
                            {
                                int globalCount = itemInCharacterReset.GlobalLimit == -1 ? int.MinValue : World.Items.Count(x => x.Blueprint.Id == itemInCharacterReset.ItemId);
                                if (globalCount < itemInCharacterReset.GlobalLimit)
                                {
                                    if (lastCharacter != null)
                                    {
                                        World.AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: G: Obj {itemInCharacterReset.ItemId} added on {lastCharacter.Blueprint.Id}");
                                    }
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: G: No last character");
                                }
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: G: not last character");
                        }
                        else
                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: G: Obj {itemInCharacterReset.ItemId} not found");

                        break;
                    }

                    case ItemInEquipmentReset itemInEquipmentReset: // 'E'
                    {
                        ItemBlueprintBase blueprint = World.GetItemBlueprint(itemInEquipmentReset.ItemId);
                        if (blueprint != null)
                        {
                            if (isLastCharacter)
                            {
                                int globalCount = itemInEquipmentReset.GlobalLimit == -1 ? int.MinValue : World.Items.Count(x => x.Blueprint.Id == itemInEquipmentReset.ItemId);
                                if (globalCount < itemInEquipmentReset.GlobalLimit)
                                {
                                    if (lastCharacter != null)
                                    {
                                        IItem item = World.AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} added on {lastCharacter.Blueprint.Id}");
                                        // try to equip
                                        if (item.WearLocation != WearLocations.None)
                                        {
                                            EquippedItem equippedItem = lastCharacter.SearchEquipmentSlot(item, false);
                                            if (equippedItem != null)
                                            {
                                                equippedItem.Item = item;
                                                item.ChangeContainer(null); // remove from inventory
                                                item.ChangeEquippedBy(lastCharacter, true); // set as equipped by lastCharacter
                                            }
                                            else
                                                Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: E: Item {itemInEquipmentReset.ItemId} wear location {item.WearLocation} doesn't exist on last character");
                                        }
                                        else
                                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: E: Item {itemInEquipmentReset.ItemId} cannot be equipped");
                                    }
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: E: Last character doesn't exist");
                                }
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: E: not last character");
                        }
                        else
                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: E: Obj {itemInEquipmentReset.ItemId} not found");

                        break;
                    }

                    case DoorReset doorReset: // 'D'
                    {
                        IExit exit = Exits[(int) doorReset.ExitDirection];
                        if (exit != null)
                        {
                            switch (doorReset.Operation)
                            {
                                case 0:
                                    Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: D: Open/Unlock {doorReset.ExitDirection}");
                                    exit.Open();
                                    exit.Unlock();
                                    break;
                                case 1:
                                    Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: D: Close/Unlock {doorReset.ExitDirection}");
                                        exit.Close();
                                    exit.Unlock();
                                    break;
                                case 2:
                                    Log.Default.WriteLine(LogLevels.Debug, $"Room {Blueprint.Id}: D: Close/Lock {doorReset.ExitDirection}");
                                        exit.Close();
                                    exit.Lock();
                                    break;
                                default:
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: D: Invalid operation {doorReset.Operation} for exit {doorReset.ExitDirection}");
                                    break;
                            }
                        }
                        else
                            Log.Default.WriteLine(LogLevels.Warning, $"Room {Blueprint.Id}: D: Invalid exit {doorReset.ExitDirection}");

                        break;
                    }

                    // TODO: R: randomize room exits
                }
            }
        }

        public void ApplyAffect(RoomFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    RoomFlags |= affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    RoomFlags = affect.Modifier;
                    break;
                case AffectOperators.Nor:
                    RoomFlags &= ~affect.Modifier;
                    break;
            }
        }

        #endregion

        protected virtual void ResetAttributes()
        {
            RoomFlags = BaseRoomFlags;
        }

        protected void ApplyAuras(IEntity entity)
        {
            if (!entity.IsValid)
                return;
            foreach (IAura aura in entity.Auras.Where(x => x.IsValid))
            {
                foreach (IRoomAffect affect in aura.Affects.OfType<IRoomAffect>())
                {
                    affect.Apply(this);
                }
            }
        }

        [Command("test", "!!Test!!")]
        // ReSharper disable once UnusedMember.Global
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Room: DoTest");
            return true;
        }

        [Command("look", "Information")]
        // ReSharper disable once UnusedMember.Global
        protected virtual CommandExecutionResults DoLook(string rawParameters, params CommandParameter[] parameters)
        {
            //TODO: better 'UI'
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("People:");
            foreach (ICharacter character in _people)
                sb.AppendFormatLine($"{character.DisplayName}");
            sb.AppendLine("Items:");
            foreach (IItem item in _content)
                sb.AppendFormatLine($"{item.DisplayName}");
            //
            Send(sb);
            return CommandExecutionResults.Ok;
        }
    }
}
