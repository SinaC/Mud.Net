using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Item;
using Mud.Server.Room;
using Mud.Settings;

namespace Mud.Server.World
{
    public class World : IWorld
    {
        private readonly List<TreasureTable<int>> _treasureTables;
        private readonly Dictionary<int, QuestBlueprint> _questBlueprints;
        private readonly Dictionary<int, RoomBlueprint> _roomBlueprints;
        private readonly Dictionary<int, CharacterBlueprintBase> _characterBlueprints;
        private readonly Dictionary<int, ItemBlueprintBase> _itemBlueprints;
        private readonly List<IArea> _areas;
        private readonly List<IRoom> _rooms;
        private readonly List<ICharacter> _characters;
        private readonly List<IItem> _items;

        private IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();
        private IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();
        private ISettings Settings => DependencyContainer.Current.GetInstance<ISettings>();

        public World()
        {
            _treasureTables = new List<TreasureTable<int>>();
            _questBlueprints = new Dictionary<int, QuestBlueprint>();
            _roomBlueprints = new Dictionary<int, RoomBlueprint>();
            _characterBlueprints = new Dictionary<int, CharacterBlueprintBase>();
            _itemBlueprints = new Dictionary<int, ItemBlueprintBase>();
            _areas = new List<IArea>();
            _rooms = new List<IRoom>();
            _characters = new List<ICharacter>();
            _items = new List<IItem>();
        }

        #region IWorld

        // Treasure tables
        public IReadOnlyCollection<TreasureTable<int>> TreasureTables => _treasureTables;

        public void AddTreasureTable(TreasureTable<int> table)
        {
            // TODO: check if already exists ?
            _treasureTables.Add(table);
        }

        // Blueprints
        public IReadOnlyCollection<QuestBlueprint> QuestBlueprints => _questBlueprints.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints => _roomBlueprints.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints => _characterBlueprints.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints => _itemBlueprints.Values.ToList().AsReadOnly();

        public QuestBlueprint GetQuestBlueprint(int id) => GetBlueprintById(_questBlueprints, id);

        public RoomBlueprint GetRoomBlueprint(int id) => GetBlueprintById(_roomBlueprints, id);

        public CharacterBlueprintBase GetCharacterBlueprint(int id) => GetBlueprintById(_characterBlueprints, id);

        public ItemBlueprintBase GetItemBlueprint(int id) => GetBlueprintById(_itemBlueprints, id);

        public TBlueprint GetCharacterBlueprint<TBlueprint>(int id)
            where TBlueprint : CharacterBlueprintBase => GetCharacterBlueprint(id) as TBlueprint;

        public TBlueprint GetItemBlueprint<TBlueprint>(int id)
            where TBlueprint : ItemBlueprintBase => GetItemBlueprint(id) as TBlueprint;

        public void AddQuestBlueprint(QuestBlueprint blueprint)
        {
            if (_questBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Quest blueprint duplicate {blueprint.Id}!!!");
            else
                _questBlueprints.Add(blueprint.Id, blueprint);
        }

        public void AddRoomBlueprint(RoomBlueprint blueprint)
        {
            if (_roomBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Room blueprint duplicate {blueprint.Id}!!!");
            else
                _roomBlueprints.Add(blueprint.Id, blueprint);
        }

        public void AddCharacterBlueprint(CharacterBlueprintBase blueprint)
        {
            if (_characterBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Character blueprint duplicate {blueprint.Id}!!!");
            else
                _characterBlueprints.Add(blueprint.Id, blueprint);
        }

        public void AddItemBlueprint(ItemBlueprintBase blueprint)
        {
            if (_itemBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Item blueprint duplicate {blueprint.Id}!!!");
            else
                _itemBlueprints.Add(blueprint.Id, blueprint);
        }

        //
        public IEnumerable<IArea> Areas => _areas;

        public IEnumerable<IRoom> Rooms => _rooms.Where(x => x.IsValid);

        public IEnumerable<ICharacter> Characters => _characters.Where(x => x.IsValid);

        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => Characters.OfType<INonPlayableCharacter>();

        public IEnumerable<IPlayableCharacter> PlayableCharacters => Characters.OfType<IPlayableCharacter>();

        public IEnumerable<IItem> Items => _items.Where(x => x.IsValid);

        public IRoom GetRandomRoom(ICharacter character) 
        {
            INonPlayableCharacter nonPlayableCharacter = character as INonPlayableCharacter;
            return RandomManager.Random(Rooms.Where(x =>
                character.CanSee(x)
                && !x.RoomFlags.HasFlag(RoomFlags.Safe)
                && !x.RoomFlags.HasFlag(RoomFlags.Private)
                && !x.RoomFlags.HasFlag(RoomFlags.Solitary)
                && (nonPlayableCharacter == null || nonPlayableCharacter.ActFlags.HasFlag(ActFlags.Aggressive) || !x.RoomFlags.HasFlag(RoomFlags.Law))));
        }

        public IRoom GetDefaultRecallRoom() 
        {
            return _rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRecallRoomId);
        }

        public IArea AddArea(Guid guid, string displayName, int minLevel, int maxLevel, string builders, string credits)
        {
            IArea area = new Area.Area(displayName, minLevel, maxLevel, builders, credits);
            _areas.Add(area);
            return area;
        }

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IRoom room = new Room.Room(Guid.NewGuid(), blueprint, area);
            room.Recompute();
            _rooms.Add(room);
            return room;
        }

        public IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            Exit from2To = new Exit(blueprint, to);
            from.Exits[(int)direction] = from2To;
            return from2To;
        }

        public IPlayableCharacter AddPlayableCharacter(Guid guid, CharacterData characterData, IPlayer player, IRoom room) // PC
        {
            IPlayableCharacter character = new Character.PlayableCharacter.PlayableCharacter(guid, characterData, player, room);
            character.Recompute();
            _characters.Add(character);
            return character;
        }

        public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room) // NPC
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            INonPlayableCharacter character = new Character.NonPlayableCharacter.NonPlayableCharacter(guid, blueprint, room);
            _characters.Add(character);
            character.Recompute();
            return character;
        }

        public IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemCorpse item = new ItemCorpse(guid, blueprint, room, victim);
            _items.Add(item);
            item.Recompute();
            return item;
        }

        public IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, ICharacter killer)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemCorpse item = new ItemCorpse(guid, blueprint, room, victim, killer);
            _items.Add(item);
            item.Recompute();
            return item;
        }

        public IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            IItem item = null;

            switch (blueprint)
            {
                case ItemArmorBlueprint armorBlueprint:
                    item = new ItemArmor(guid, armorBlueprint, container);
                    break;
                case ItemContainerBlueprint containerBlueprint:
                    item = new ItemContainer(guid, containerBlueprint, container);
                    break;
                case ItemCorpseBlueprint _:
                    Log.Default.WriteLine(LogLevels.Error, "World.AddItem: CorpseBlueprint cannot be added with this API.");
                    break;
                case ItemFoodBlueprint foodBlueprint:
                    item = new ItemFood(guid, foodBlueprint, container);
                    break;
                case ItemFurnitureBlueprint furnitureBlueprint:
                    item = new ItemFurniture(guid, furnitureBlueprint, container);
                    break;
                case ItemFountainBlueprint fountainBlueprint:
                    item = new ItemFountain(guid, fountainBlueprint, container);
                    break;
                case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                    item = new ItemDrinkContainer(guid, drinkContainerBlueprint, container);
                    break;
                case ItemJewelryBlueprint jewelryBlueprint:
                    item = new ItemJewelry(guid, jewelryBlueprint, container);
                    break;
                case ItemKeyBlueprint keyBlueprint:
                    item = new ItemKey(guid, keyBlueprint, container);
                    break;
                case ItemLightBlueprint lightBlueprint:
                    item = new ItemLight(guid, lightBlueprint, container);
                    break;
                case ItemPortalBlueprint portalBlueprint:
                {
                    IRoom destination = null;
                    if (portalBlueprint.Destination != ItemPortal.NoDestinationRoomId)
                    {
                        destination = Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
                        if (destination == null)
                        {
                            destination = Rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRecallRoomId);
                            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: PortalBlueprint {0} unknown destination {1} setting to recall {2}", blueprint.Id, portalBlueprint.Destination, Settings.DefaultRecallRoomId);
                        }
                    }
                    item = new ItemPortal(guid, portalBlueprint, destination, container);
                    break;
                    }
                case ItemQuestBlueprint questBlueprint:
                    item = new ItemQuest(guid, questBlueprint, container);
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    item = new ItemShield(guid, shieldBlueprint, container);
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    item = new ItemWeapon(guid, weaponBlueprint, container);
                    break;
                case ItemWarpstoneBlueprint warpstoneBlueprint:
                    item = new ItemWarpstone(guid, warpstoneBlueprint, container);
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unknown Item blueprint type {0}", blueprint.GetType());
                    break;
            }
            if (item != null)
            {
                item.Recompute();
                _items.Add(item);
                return item;
            }

            // TODO: other blueprint
            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Invalid blueprint type {0}", blueprint.GetType().FullName);
            return null;
        }

        public IItem AddItem(Guid guid, ItemData itemData, IContainer container)
        {
            ItemBlueprintBase blueprint = GetItemBlueprint(itemData.ItemId);
            if (blueprint == null)
            {
                string msg = $"Item blueprint Id {itemData.ItemId} doesn't exist anymore";
                Log.Default.WriteLine(LogLevels.Error, msg);
                Wiznet.Wiznet(msg, WiznetFlags.Bugs);

                return null;
            }

            IItem item = null;
            switch (blueprint)
            {
                case ItemArmorBlueprint armorBlueprint:
                    item = new ItemArmor(guid, armorBlueprint, itemData, container); // no specific ItemData
                    break;
                case ItemContainerBlueprint containerBlueprint:
                    item = new ItemContainer(guid, containerBlueprint, itemData as ItemContainerData, container);
                    break;
                case ItemCorpseBlueprint corpseBlueprint:
                    item = new ItemCorpse(guid, corpseBlueprint, itemData as ItemCorpseData, container);
                    break;
                case ItemFoodBlueprint foodBlueprint:
                    item = new ItemFood(guid, foodBlueprint, container);
                    break;
                case ItemFurnitureBlueprint furnitureBlueprint:
                    item = new ItemFurniture(guid, furnitureBlueprint, itemData, container);
                    break;
                case ItemFountainBlueprint fountainBlueprint:
                    item = new ItemFountain(guid, fountainBlueprint, itemData, container);
                    break;
                case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                    item = new ItemDrinkContainer(guid, drinkContainerBlueprint, itemData as ItemDrinkContainerData, container);
                    break;
                case ItemJewelryBlueprint jewelryBlueprint:
                    item = new ItemJewelry (guid, jewelryBlueprint, itemData, container);
                    break;
                case ItemKeyBlueprint keyBlueprint:
                    item = new ItemKey(guid, keyBlueprint, itemData, container);
                    break;
                case ItemLightBlueprint lightBlueprint:
                    item = new ItemLight(guid, lightBlueprint, itemData, container);
                    break;
                case ItemPortalBlueprint portalBlueprint:
                    {
                        ItemPortalData itemPortalData = itemData as ItemPortalData;
                        IRoom destination = null;
                        if (itemPortalData.DestinationRoomId != ItemPortal.NoDestinationRoomId)
                        {
                            destination = Rooms.FirstOrDefault(x => x.Blueprint?.Id == itemPortalData.DestinationRoomId);
                            if (destination == null)
                            {
                                destination = Rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRecallRoomId);
                                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: ItemPortalData unknown destination {0} setting to recall {1}", itemPortalData.DestinationRoomId, Settings.DefaultRecallRoomId);
                            }
                        }
                        item = new ItemPortal(guid, portalBlueprint, itemPortalData, destination, container);
                    }
                    break;
                case ItemQuestBlueprint questBlueprint:
                    item = new ItemQuest(guid, questBlueprint, itemData, container);
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    item = new ItemShield(guid, shieldBlueprint, itemData, container);
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    item = new ItemWeapon(guid, weaponBlueprint, itemData as ItemWeaponData, container);
                    break;
                case ItemWarpstoneBlueprint warpstoneBlueprint:
                    item = new ItemWarpstone(guid, warpstoneBlueprint, itemData, container);
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unknown Item blueprint type {0}", blueprint.GetType());
                    break;
            }

            if (item != null)
            {
                item.Recompute();
                _items.Add(item);
                return item;
            }
            // TODO: other blueprint
            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Invalid blueprint type {0}", blueprint.GetType().FullName);
            return null;
        }

        public IItem AddItem(Guid guid, int blueprintId, IContainer container)
        {
            ItemBlueprintBase blueprint = GetItemBlueprint(blueprintId);
            if (blueprint == null)
            {
                string msg = $"Item blueprint Id {blueprintId} doesn't exist anymore";
                Log.Default.WriteLine(LogLevels.Error, msg);
                Wiznet.Wiznet(msg, WiznetFlags.Bugs);

                return null;
            }
            IItem item = AddItem(guid, blueprint, container);
            item?.Recompute();
            if (item == null)
                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Unknown blueprint id {0} or type {1}", blueprintId, blueprint.GetType().FullName);
            return item;
        }

        public IAura AddAura(IEntity target, IAbility ability, IEntity source, int level, TimeSpan duration, AuraFlags auraFlags, bool recompute, params IAffect[] affects)
        {
            IAura aura = new Aura.Aura(ability, source, auraFlags, level, duration, affects);
            target.AddAura(aura, recompute);
            return aura;
        }

        //public IPeriodicAura AddPeriodicAura(IEntity target, IAbility ability, IEntity source, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
        //{
        //    IPeriodicAura periodicAura = new PeriodicAura(ability, PeriodicAuraTypes.Heal, source, amount, amountOperator, level, tickVisible, tickDelay, totalTicks);
        //    target.AddPeriodicAura(periodicAura);
        //    return periodicAura;
        //}

        //public IPeriodicAura AddPeriodicAura(IEntity target, IAbility ability, IEntity source, SchoolTypes school, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
        //{
        //    IPeriodicAura periodicAura = new PeriodicAura(ability, PeriodicAuraTypes.Damage, source, school, amount, amountOperator, level, tickVisible, tickDelay, totalTicks);
        //    target.AddPeriodicAura(periodicAura);
        //    return periodicAura;
        //}

        public void RemoveCharacter(ICharacter character)
        {
            character.StopFighting(true);

            // Remove from group if in a group + stop following
            if (character is IPlayableCharacter playableCharacter && playableCharacter.Leader != null)
            {
                IPlayableCharacter leader = playableCharacter.Leader;
                leader.StopFollower(playableCharacter);
                if (leader.GroupMembers.Any(x => x == character))
                    leader.RemoveGroupMember(playableCharacter, false);
            }
            // TODO: if leader of a group

            // Search IPeriodicAura with character as Source and nullify Source
            IReadOnlyCollection<ICharacter> charactersWithPeriodicAuras = new ReadOnlyCollection<ICharacter>(_characters.Where(x => x.PeriodicAuras.Any(pa => pa.Source == character)).ToList()); // clone
            foreach (ICharacter characterWithPeriodicAuras in charactersWithPeriodicAuras)
            {
                foreach (IPeriodicAura pa in characterWithPeriodicAuras.PeriodicAuras.Where(x => x.Source == character))
                    pa.ResetSource();
            }

            //// Search IAura with character as Source and nullify Source
            //IReadOnlyCollection<ICharacter> charactersWithAuras = new ReadOnlyCollection<ICharacter>(_characters.Where(x => x.Auras.Any(a => a.Source == character)).ToList()); // clone
            //foreach (ICharacter characterWithAuras in charactersWithAuras)
            //{
            //    foreach (IAura aura in characterWithAuras.Auras.Where(x => x.Source == character))
            //        aura.ResetSource();
            //}

            // Remove periodic auras on character
            IReadOnlyCollection<IPeriodicAura> periodicAuras = new ReadOnlyCollection<IPeriodicAura>(character.PeriodicAuras.ToList()); // clone
            foreach (IPeriodicAura pa in periodicAuras)
            {
                pa.ResetSource();
                character.RemovePeriodicAura(pa);
            }

            // Remove auras
            IReadOnlyCollection<IAura> auras = new ReadOnlyCollection<IAura>(character.Auras.ToList()); // clone
            foreach (IAura aura in auras)
            {
                aura.OnRemoved();
                character.RemoveAura(aura, false);
            }
            // no need to recompute

            // Remove content
            if (character.Inventory.Any())
            {
                IReadOnlyCollection<IItem> clonedInventory = new ReadOnlyCollection<IItem>(character.Inventory.ToList()); // clone because GetFromContainer change Content collection
                foreach (IItem item in clonedInventory)
                    RemoveItem(item);
                // Remove equipments
                if (character.Equipments.Any(x => x.Item != null))
                {
                    IReadOnlyCollection<IEquipableItem> equipment = new ReadOnlyCollection<IEquipableItem>(character.Equipments.Where(x => x.Item != null).Select(x => x.Item).ToList()); // clone
                    foreach (IEquipableItem item in equipment)
                        RemoveItem(item);
                }
            }
            // Remove from room
            character.ChangeRoom(null);
            //
            character.OnRemoved();
            //_characters.Remove(character); will be removed in cleanup step
        }

        public void RemoveItem(IItem item)
        {
            //item.ContainedInto?.GetFromContainer(item);
            item.ChangeContainer(null);
            IEquipableItem equipable = item as IEquipableItem;
            equipable?.ChangeEquipedBy(null);
            // If container, remove content
            if (item is IContainer container)
            {
                IReadOnlyCollection<IItem> content = new ReadOnlyCollection<IItem>(container.Content.ToList()); // clone to be sure
                foreach (IItem itemInContainer in content)
                    RemoveItem(itemInContainer);
            }
            // Remove auras
            IReadOnlyCollection<IAura> auras = new ReadOnlyCollection<IAura>(item.Auras.ToList()); // clone
            foreach (IAura aura in auras)
            {
                aura.OnRemoved();
                item.RemoveAura(aura, false);
            }
            // no need to recompute
            //
            item.OnRemoved();
            //_items.Remove(item); will be removed in cleanup step
        }

        public void RemoveRoom(IRoom room)
        {
            //// Remove auras
            //IReadOnlyCollection<IAura> auras = new ReadOnlyCollection<IAura>(room.Auras.ToList()); // clone
            //foreach (IAura aura in auras)
            //{
            //    aura.OnRemoved();
            //    room.RemoveAura(aura, false);
            //}
            //// no need to recompute
            ////
            //room.OnRemoved();
            throw new NotImplementedException();
        }

        public void Update() // called every pulse (every 1/4 seconds)
        {
            // TODO: see update.C:2332
        }

        public void Cleanup() // remove invalid entities
        {
            // TODO: room ?
            if (_items.Any(x => !x.IsValid))
                Log.Default.WriteLine(LogLevels.Debug, $"Cleaning {_items.Count(x => !x.IsValid)} item(s)");
            if (_characters.Any(x => !x.IsValid))
                Log.Default.WriteLine(LogLevels.Debug, $"Cleaning {_characters.Count(x => !x.IsValid)} character(s)");

            _items.RemoveAll(x => !x.IsValid);
            _characters.RemoveAll(x => !x.IsValid);
            // TODO: room
        }

        #endregion

        private T GetBlueprintById<T>(IDictionary<int, T> blueprints, int id)
        {
            blueprints.TryGetValue(id, out var blueprint);
            return blueprint;
        }


        private IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemContainer item = new ItemContainer(guid, blueprint, container);
            _items.Add(item);
            return item;
        }
    }
}
