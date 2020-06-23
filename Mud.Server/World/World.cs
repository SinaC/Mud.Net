using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Item;
using Mud.Server.Random;
using Mud.Server.Room;
using Mud.Settings;

namespace Mud.Server.World
{
    public class World : IWorld, IAreaManager, IRoomManager, ICharacterManager, IItemManager, IAuraManager, IQuestManager
    {
        #region IQuestManager

        private readonly Dictionary<int, QuestBlueprint> _questBlueprints;

        public IReadOnlyCollection<QuestBlueprint> QuestBlueprints => _questBlueprints.Values.ToList().AsReadOnly();

        public QuestBlueprint GetQuestBlueprint(int id) => GetBlueprintById(_questBlueprints, id);

        public void AddQuestBlueprint(QuestBlueprint blueprint)
        {
            if (_questBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, "Quest blueprint duplicate {0}!!!", blueprint.Id);
            else
                _questBlueprints.Add(blueprint.Id, blueprint);
        }

        #endregion

        #region IWorld

        private readonly List<TreasureTable<int>> _treasureTables;

        public IReadOnlyCollection<TreasureTable<int>> TreasureTables => _treasureTables;

        public void AddTreasureTable(TreasureTable<int> table)
        {
            // TODO: check if already exists ?
            _treasureTables.Add(table);
        }

        public void FixWorld()
        {
            FixItems();
            FixResets();
        }

        public void ResetWorld()
        {
            foreach (IArea area in _areas)
            {
                // TODO: handle age + at load time, force age to arbitrary high value to ensure reset are computed
                //if (area.PlayableCharacters.Any())
                {
                    area.ResetArea();
                }
            }
        }

        #endregion

        #region IAreaManager

        private readonly Dictionary<int, AreaBlueprint> _areaBlueprints;
        private readonly List<IArea> _areas;

        public IReadOnlyCollection<AreaBlueprint> AreaBlueprints => _areaBlueprints.Values.ToList().AsReadOnly();
        public AreaBlueprint GetAreaBlueprint(int id) => GetBlueprintById(_areaBlueprints, id);

        public void AddAreaBlueprint(AreaBlueprint blueprint)
        {
            if (_areaBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, "Area blueprint duplicate {0}!!!", blueprint.Id);
            else
                _areaBlueprints.Add(blueprint.Id, blueprint);
        }

        public IEnumerable<IArea> Areas => _areas;

        public IArea AddArea(Guid guid, AreaBlueprint blueprint)
        {
            IArea area = new Area.Area(guid, blueprint);
            _areas.Add(area);
            return area;
        }

        #endregion

        #region IRoomManager
        // Null room is used to avoid setting char.room to null when deleting and is used as container when deleting item
        private IRoom _nullRoom; // save a reference for further use

        private readonly Dictionary<int, RoomBlueprint> _roomBlueprints;
        private readonly List<IRoom> _rooms;
        public IRoom NullRoom => _nullRoom = _nullRoom ?? Rooms.Single(x => x.Blueprint.Id == Settings.NullRoomId);

        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints => _roomBlueprints.Values.ToList().AsReadOnly();

        public RoomBlueprint GetRoomBlueprint(int id) => GetBlueprintById(_roomBlueprints, id);

        public void AddRoomBlueprint(RoomBlueprint blueprint)
        {
            if (_roomBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, "Room blueprint duplicate {0}!!!", blueprint.Id);
            else
                _roomBlueprints.Add(blueprint.Id, blueprint);
        }

        public IEnumerable<IRoom> Rooms => _rooms.Where(x => x.IsValid);

        public IRoom DefaultRecallRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultRecallRoomId);
        public IRoom DefaultDeathRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.DefaultDeathRoomId);
        public IRoom MudSchoolRoom => _rooms.FirstOrDefault(x => x.Blueprint.Id == Settings.MudSchoolRoomId);

        public IRoom GetRandomRoom(ICharacter character)
        {
            INonPlayableCharacter nonPlayableCharacter = character as INonPlayableCharacter;
            return RandomManager.Random(Rooms.Where(x =>
                character.CanSee(x)
                && !x.IsPrivate
                && !x.RoomFlags.HasFlag(RoomFlags.Safe)
                && !x.RoomFlags.HasFlag(RoomFlags.Private)
                && !x.RoomFlags.HasFlag(RoomFlags.Solitary)
                && (nonPlayableCharacter == null || nonPlayableCharacter.ActFlags.HasFlag(ActFlags.Aggressive) || !x.RoomFlags.HasFlag(RoomFlags.Law))));
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

        #endregion

        #region ICharacterManager

        private readonly Dictionary<int, CharacterBlueprintBase> _characterBlueprints;
        private readonly List<ICharacter> _characters;

        public IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints => _characterBlueprints.Values.ToList().AsReadOnly();

        public CharacterBlueprintBase GetCharacterBlueprint(int id) => GetBlueprintById(_characterBlueprints, id);
        public TBlueprint GetCharacterBlueprint<TBlueprint>(int id)
            where TBlueprint : CharacterBlueprintBase => GetCharacterBlueprint(id) as TBlueprint;

        public void AddCharacterBlueprint(CharacterBlueprintBase blueprint)
        {
            if (_characterBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, "Character blueprint duplicate {0}!!!", blueprint.Id);
            else
                _characterBlueprints.Add(blueprint.Id, blueprint);
        }

        public IEnumerable<ICharacter> Characters => _characters.Where(x => x.IsValid);

        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => Characters.OfType<INonPlayableCharacter>();

        public IEnumerable<IPlayableCharacter> PlayableCharacters => Characters.OfType<IPlayableCharacter>();

        public IPlayableCharacter AddPlayableCharacter(Guid guid, PlayableCharacterData playableCharacterData, IPlayer player, IRoom room) // PC
        {
            IPlayableCharacter character = new Character.PlayableCharacter.PlayableCharacter(guid, playableCharacterData, player, room);
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

        public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room) // pet
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            INonPlayableCharacter character = new Character.NonPlayableCharacter.NonPlayableCharacter(guid, blueprint, petData, room);
            _characters.Add(character);
            character.Recompute();
            return character;
        }

        public void RemoveCharacter(ICharacter character)
        {
            character.StopFighting(true);

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
                    IReadOnlyCollection<IItem> equipment = new ReadOnlyCollection<IItem>(character.Equipments.Where(x => x.Item != null).Select(x => x.Item).ToList()); // clone
                    foreach (IItem item in equipment)
                        RemoveItem(item);
                }
            }
            // Move to NullRoom
            character.ChangeRoom(NullRoom);
            //
            character.OnRemoved();
            //_characters.Remove(character); will be removed in cleanup step
        }

        #endregion

        #region IItemManager

        private readonly Dictionary<int, ItemBlueprintBase> _itemBlueprints;
        private readonly List<IItem> _items;

        public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints => _itemBlueprints.Values.ToList().AsReadOnly();

        public ItemBlueprintBase GetItemBlueprint(int id) => GetBlueprintById(_itemBlueprints, id);

        public TBlueprint GetItemBlueprint<TBlueprint>(int id)
            where TBlueprint : ItemBlueprintBase => GetItemBlueprint(id) as TBlueprint;

        public void AddItemBlueprint(ItemBlueprintBase blueprint)
        {
            if (_itemBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, "Item blueprint duplicate {0}!!!", blueprint.Id);
            else
                _itemBlueprints.Add(blueprint.Id, blueprint);
        }

        public IEnumerable<IItem> Items => _items.Where(x => x.IsValid);

        public IItemCorpse AddItemCorpse(Guid guid, IRoom room, ICharacter victim)
        {
            ItemCorpseBlueprint blueprint = GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId);
            if (blueprint == null)
                throw new Exception($"Corpse blueprint {Settings.CorpseBlueprintId} not found");
            IItemCorpse item = new ItemCorpse(guid, blueprint, room, victim);
            _items.Add(item);
            item.Recompute();
            return item;
        }

        public IItemCorpse AddItemCorpse(Guid guid, IRoom room, ICharacter victim, ICharacter killer)
        {
            ItemCorpseBlueprint blueprint = GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId);
            if (blueprint == null)
                throw new Exception($"Corpse blueprint {Settings.CorpseBlueprintId} not found");
            IItemCorpse item = new ItemCorpse(guid, blueprint, room, victim, killer);
            _items.Add(item);
            item.Recompute();
            return item;
        }

        public IItemMoney AddItemMoney(Guid guid, long silverCoins, long goldCoins, IContainer container)
        {
            silverCoins = Math.Max(0, silverCoins);
            goldCoins = Math.Max(0, goldCoins);
            if (silverCoins == 0 && goldCoins == 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "World.AddItemMoney: 0 silver and 0 gold.");
                return null;
            }
            int blueprintId = Settings.CoinsBlueprintId;
            ItemMoneyBlueprint blueprint = GetItemBlueprint<ItemMoneyBlueprint>(blueprintId);
            if (blueprint == null)
                throw new Exception($"Money blueprint {blueprintId} not found");
            IItemMoney money = new ItemMoney(guid, blueprint, silverCoins, goldCoins, container);
            _items.Add(money);
            return money;
        }

        public IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            IItem item = null;

            switch (blueprint)
            {
                case ItemArmorBlueprint armorBlueprint:
                    item = new ItemArmor(guid, armorBlueprint, container);
                    break;
                case ItemBoatBlueprint boatBlueprint:
                    item = new ItemBoat(guid, boatBlueprint, container);
                    break;
                case ItemClothingBlueprint clothingBlueprint:
                    item = new ItemClothing(guid, clothingBlueprint, container);
                    break;
                case ItemContainerBlueprint containerBlueprint:
                    item = new ItemContainer(guid, containerBlueprint, container);
                    break;
                case ItemCorpseBlueprint corpseBlueprint:
                    item = new ItemCorpse(guid, corpseBlueprint, container);
                    break;
                case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                    item = new ItemDrinkContainer(guid, drinkContainerBlueprint, container);
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
                case ItemGemBlueprint gemBlueprint:
                    item = new ItemGem(guid, gemBlueprint, container);
                    break;
                case ItemJewelryBlueprint jewelryBlueprint:
                    item = new ItemJewelry(guid, jewelryBlueprint, container);
                    break;
                case ItemJukeboxBlueprint jukeboxBlueprint:
                    item = new ItemJukebox(guid, jukeboxBlueprint, container);
                    break;
                case ItemKeyBlueprint keyBlueprint:
                    item = new ItemKey(guid, keyBlueprint, container);
                    break;
                case ItemLightBlueprint lightBlueprint:
                    item = new ItemLight(guid, lightBlueprint, container);
                    break;
                case ItemMapBlueprint mapBlueprint:
                    item = new ItemMap(guid, mapBlueprint, container);
                    break;
                case ItemMoneyBlueprint moneyBlueprint:
                    item = new ItemMoney(guid, moneyBlueprint, container);
                    break;
                case ItemPillBlueprint pillBlueprint:
                    item = new ItemPill(guid, pillBlueprint, container);
                    break;
                case ItemPotionBlueprint potionBlueprint:
                    item = new ItemPotion(guid, potionBlueprint, container);
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
                case ItemScrollBlueprint scrollBlueprint:
                    item = new ItemScroll(guid, scrollBlueprint, container);
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    item = new ItemShield(guid, shieldBlueprint, container);
                    break;
                case ItemStaffBlueprint staffBlueprint:
                    item = new ItemStaff(guid, staffBlueprint, container);
                    break;
                case ItemTrashBlueprint trashBlueprint:
                    item = new ItemTrash(guid, trashBlueprint, container);
                    break;
                case ItemTreasureBlueprint treasureBlueprint:
                    item = new ItemTreasure(guid, treasureBlueprint, container);
                    break;
                case ItemWandBlueprint wandBlueprint:
                    item = new ItemWand(guid, wandBlueprint, container);
                    break;
                case ItemWarpStoneBlueprint warpstoneBlueprint:
                    item = new ItemWarpstone(guid, warpstoneBlueprint, container);
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    item = new ItemWeapon(guid, weaponBlueprint, container);
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "World.AddItem: unknown Item blueprint type {0}.", blueprint.GetType());
                    break;
            }
            if (item != null)
            {
                item.Recompute();
                _items.Add(item);
                return item;
            }

            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: unknown Item blueprint type {0}.", blueprint.GetType());
            return null;
        }

        public IItem AddItem(Guid guid, ItemData itemData, IContainer container)
        {
            ItemBlueprintBase blueprint = GetItemBlueprint(itemData.ItemId);
            if (blueprint == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Item blueprint Id {0} doesn't exist anymore.", itemData.ItemId);
                return null;
            }

            IItem item = null;
            switch (blueprint)
            {
                case ItemArmorBlueprint armorBlueprint:
                    item = new ItemArmor(guid, armorBlueprint, itemData, container); // no specific ItemData
                    break;
                case ItemBoatBlueprint boatBlueprint:
                    item = new ItemBoat(guid, boatBlueprint, itemData, container);
                    break;
                case ItemClothingBlueprint clothingBlueprint:
                    item = new ItemClothing(guid, clothingBlueprint, itemData, container);
                    break;
                case ItemContainerBlueprint containerBlueprint:
                    item = new ItemContainer(guid, containerBlueprint, itemData as ItemContainerData, container);
                    break;
                case ItemCorpseBlueprint corpseBlueprint:
                    item = new ItemCorpse(guid, corpseBlueprint, itemData as ItemCorpseData, container);
                    break;
                case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                    item = new ItemDrinkContainer(guid, drinkContainerBlueprint, itemData as ItemDrinkContainerData, container);
                    break;
                case ItemFoodBlueprint foodBlueprint:
                    item = new ItemFood(guid, foodBlueprint, itemData as ItemFoodData, container);
                    break;
                case ItemFurnitureBlueprint furnitureBlueprint:
                    item = new ItemFurniture(guid, furnitureBlueprint, itemData, container);
                    break;
                case ItemFountainBlueprint fountainBlueprint:
                    item = new ItemFountain(guid, fountainBlueprint, itemData, container);
                    break;
                case ItemGemBlueprint gemBlueprint:
                    item = new ItemGem(guid, gemBlueprint, itemData, container);
                    break;
                case ItemJewelryBlueprint jewelryBlueprint:
                    item = new ItemJewelry(guid, jewelryBlueprint, itemData, container);
                    break;
                case ItemJukeboxBlueprint jukeboxBlueprint:
                    item = new ItemJukebox(guid, jukeboxBlueprint, itemData, container);
                    break;
                case ItemKeyBlueprint keyBlueprint:
                    item = new ItemKey(guid, keyBlueprint, itemData, container);
                    break;
                case ItemLightBlueprint lightBlueprint:
                    item = new ItemLight(guid, lightBlueprint, itemData as ItemLightData, container);
                    break;
                case ItemMapBlueprint mapBlueprint:
                    item = new ItemMap(guid, mapBlueprint, itemData, container);
                    break;
                case ItemMoneyBlueprint moneyBlueprint:
                    item = new ItemMoney(guid, moneyBlueprint, itemData, container);
                    break;
                case ItemPillBlueprint pillBlueprint:
                    item = new ItemPill(guid, pillBlueprint, itemData, container);
                    break;
                case ItemPotionBlueprint potionBlueprint:
                    item = new ItemPotion(guid, potionBlueprint, itemData, container);
                    break;
                case ItemPortalBlueprint portalBlueprint:
                    {
                        ItemPortalData itemPortalData = itemData as ItemPortalData;
                        IRoom destination = null;
                        if (itemPortalData != null && itemPortalData.DestinationRoomId != ItemPortal.NoDestinationRoomId)
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
                case ItemScrollBlueprint scrollBlueprint:
                    item = new ItemScroll(guid, scrollBlueprint, itemData, container);
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    item = new ItemShield(guid, shieldBlueprint, itemData, container);
                    break;
                case ItemStaffBlueprint staffBlueprint:
                    item = new ItemStaff(guid, staffBlueprint, itemData as ItemStaffData, container);
                    break;
                case ItemTrashBlueprint trashBlueprint:
                    item = new ItemTrash(guid, trashBlueprint, itemData, container);
                    break;
                case ItemTreasureBlueprint treasureBlueprint:
                    item = new ItemTreasure(guid, treasureBlueprint, itemData, container);
                    break;
                case ItemWandBlueprint wandBlueprint:
                    item = new ItemWand(guid, wandBlueprint, itemData as ItemWandData, container);
                    break;
                case ItemWarpStoneBlueprint warpstoneBlueprint:
                    item = new ItemWarpstone(guid, warpstoneBlueprint, itemData, container);
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    item = new ItemWeapon(guid, weaponBlueprint, itemData as ItemWeaponData, container);
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Unknown Item blueprint type {0}", blueprint.GetType());
                    break;
            }

            if (item != null)
            {
                item.Recompute();
                _items.Add(item);
                return item;
            }
            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Invalid blueprint type  {0}", blueprint.GetType());
            return null;
        }

        public IItem AddItem(Guid guid, int blueprintId, IContainer container)
        {
            ItemBlueprintBase blueprint = GetItemBlueprint(blueprintId);
            if (blueprint == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Item blueprint Id {0} doesn't exist anymore.", blueprintId);
                return null;
            }
            IItem item = AddItem(guid, blueprint, container);
            if (item == null)
                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Unknown blueprint id {0} or type {1}.", blueprintId, blueprint.GetType().FullName);
            return item;
        }

        public void RemoveItem(IItem item)
        {
            item.ChangeContainer(NullRoom); // move to NullRoom
            item.ChangeEquippedBy(null, false);
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

        #endregion

        #region IAuraManager

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

        public IAura AddAura(IEntity target, string abilityName, IEntity source, int level, TimeSpan duration, AuraFlags auraFlags, bool recompute, params IAffect[] affects)
        {
            IAura aura = new Aura.Aura(abilityName, source, auraFlags, level, duration, affects);
            target.AddAura(aura, recompute);
            return aura;
        }

        public IAura AddAura(IEntity target, AuraData auraData, bool recompute)
        {
            IAura aura = new Aura.Aura(auraData);
            target.AddAura(aura, recompute);
            return aura;
        }

        #endregion

        private IRandomManager RandomManager { get; }
        private ISettings Settings { get; }

        public World(IRandomManager randomManager, ISettings settings)
        {
            RandomManager = randomManager;
            Settings = settings;

            _treasureTables = new List<TreasureTable<int>>();
            _questBlueprints = new Dictionary<int, QuestBlueprint>();
            _areaBlueprints = new Dictionary<int, AreaBlueprint>();
            _roomBlueprints = new Dictionary<int, RoomBlueprint>();
            _characterBlueprints = new Dictionary<int, CharacterBlueprintBase>();
            _itemBlueprints = new Dictionary<int, ItemBlueprintBase>();
            _areas = new List<IArea>();
            _rooms = new List<IRoom>();
            _characters = new List<ICharacter>();
            _items = new List<IItem>();
        }

        public void Cleanup() // remove invalid entities
        {
            // TODO: room ?
            if (_items.Any(x => !x.IsValid))
                Log.Default.WriteLine(LogLevels.Debug, $"Cleaning {_items.Count(x => !x.IsValid)} item(s)");
            if (_characters.Any(x => !x.IsValid))
                Log.Default.WriteLine(LogLevels.Debug, $"Cleaning {_characters.Count(x => !x.IsValid)} character(s)");

            IItem[] itemsToRemove = _items.Where(x => !x.IsValid).ToArray();
            foreach (IItem item in itemsToRemove)
                item.OnCleaned(); // definitive remove
            _items.RemoveAll(x => !x.IsValid);

            ICharacter[] charactersToRemove = _characters.Where(x => !x.IsValid).ToArray();
            foreach (ICharacter character in charactersToRemove)
                character.OnCleaned(); // definitive remove
            _characters.RemoveAll(x => !x.IsValid);
            // TODO: room
        }

        private T GetBlueprintById<T>(IDictionary<int, T> blueprints, int id)
        {
            blueprints.TryGetValue(id, out var blueprint);
            return blueprint;
        }

        private void FixItems()
        {
            Log.Default.WriteLine(LogLevels.Info, "Fixing items");
            foreach (ItemBlueprintBase itemBlueprint in ItemBlueprints.OrderBy(x => x.Id))
            {
                switch (itemBlueprint)
                {
                    case ItemLightBlueprint _:
                        if (itemBlueprint.WearLocation != WearLocations.Light)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Light {0} has wear location {1} -> set wear location to Light", itemBlueprint.Id, itemBlueprint.WearLocation);
                            itemBlueprint.WearLocation = WearLocations.Light;
                        }

                        break;
                    case ItemWeaponBlueprint weaponBlueprint:
                        if (itemBlueprint.WearLocation != WearLocations.Wield && itemBlueprint.WearLocation != WearLocations.Wield2H)
                        {
                            WearLocations newWearLocation = WearLocations.Wield;
                            if (weaponBlueprint.Flags.HasFlag(WeaponFlags.TwoHands))
                                newWearLocation = WearLocations.Wield2H;
                            Log.Default.WriteLine(LogLevels.Error, "Weapon {0} has wear location {1} -> set wear location to {2}", itemBlueprint.Id, itemBlueprint.WearLocation, newWearLocation);
                            itemBlueprint.WearLocation = newWearLocation;
                        }

                        break;
                    case ItemShieldBlueprint _:
                        if (itemBlueprint.WearLocation != WearLocations.Shield)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Shield {0} has wear location {1} -> set wear location to Shield", itemBlueprint.Id, itemBlueprint.WearLocation);
                            itemBlueprint.WearLocation = WearLocations.Shield;
                        }

                        break;
                    case ItemStaffBlueprint _:
                    case ItemWandBlueprint _:
                        if (itemBlueprint.WearLocation != WearLocations.Hold)
                        {
                            Log.Default.WriteLine(LogLevels.Error, "{0} {1} has wear location {2} -> set wear location to Hold", itemBlueprint.ItemType(), itemBlueprint.Id, itemBlueprint.WearLocation);
                            itemBlueprint.WearLocation = WearLocations.Hold;
                        }

                        break;
                }
            }

            Log.Default.WriteLine(LogLevels.Info, "items fixed");
        }

        private void FixResets()
        {
            Log.Default.WriteLine(LogLevels.Info, "Fixing resets");

            // Global count is used to check global limit to 0
            Dictionary<int, int> characterResetGlobalCountById = new Dictionary<int, int>();
            Dictionary<int, int> itemResetCountGlobalById = new Dictionary<int, int>();

            foreach (IRoom room in _rooms.Where(x => x.Blueprint.Resets?.Any() == true).OrderBy(x => x.Blueprint.Id))
            {
                Dictionary<int, int> characterResetCountById = new Dictionary<int, int>();
                Dictionary<int, int> itemResetCountById = new Dictionary<int, int>();

                // Count to check local limit  TODO: local limit is relative to container   example in dwarven.are Room 6534: item 6506 found with reset O and reset E -> 2 different containers
                foreach (ResetBase reset in room.Blueprint.Resets)
                {
                    switch (reset)
                    {
                        case CharacterReset characterReset:
                            characterResetCountById.Increment(characterReset.CharacterId);
                            characterResetGlobalCountById.Increment(characterReset.CharacterId);
                            break;
                        case ItemInRoomReset itemInRoomReset:
                            itemResetCountById.Increment(itemInRoomReset.ItemId);
                            itemResetCountGlobalById.Increment(itemInRoomReset.ItemId);
                            break;
                        case ItemInItemReset itemInItemReset:
                            itemResetCountById.Increment(itemInItemReset.ItemId);
                            itemResetCountGlobalById.Increment(itemInItemReset.ItemId);
                            break;
                        case ItemInCharacterReset itemInCharacterReset:
                            itemResetCountById.Increment(itemInCharacterReset.ItemId);
                            itemResetCountGlobalById.Increment(itemInCharacterReset.ItemId);
                            break;
                        case ItemInEquipmentReset itemInEquipmentReset:
                            itemResetCountById.Increment(itemInEquipmentReset.ItemId);
                            itemResetCountGlobalById.Increment(itemInEquipmentReset.ItemId);
                            break;
                    }
                }

                // Check local limit + wear location
                foreach (ResetBase reset in room.Blueprint.Resets)
                {
                    switch (reset)
                    {
                        case CharacterReset characterReset:
                        {
                            int localCount = characterResetCountById[characterReset.CharacterId];
                            int localLimit = characterReset.LocalLimit;
                            if (localCount > localLimit)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Room {0}: M: character {1} found {2} times in room but local limit is {3} -> modifying local limit to {4}", room.Blueprint.Id, characterReset.CharacterId, localCount, localLimit, localCount);
                                characterReset.LocalLimit = localCount;
                            }

                            break;
                        }

                        case ItemInRoomReset itemInRoomReset:
                        {
                            int localCount = itemResetCountById[itemInRoomReset.ItemId];
                            int localLimit = itemInRoomReset.LocalLimit;
                            if (localCount > localLimit)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Room {0}: O: item {1} found {2} times in room but local limit is {3} -> modifying local limit to {4}", room.Blueprint.Id, itemInRoomReset.ItemId, localCount, localLimit, localCount);
                                itemInRoomReset.LocalLimit = localCount;
                            }

                            break;
                        }

                        case ItemInItemReset itemInItemReset:
                        {
                            int localCount = itemResetCountById[itemInItemReset.ItemId];
                            int localLimit = itemInItemReset.LocalLimit;
                            if (localCount > localLimit)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Room {0}: O: item {1} found {2} times in room but local limit is {3} -> modifying local limit to {4}", room.Blueprint.Id, itemInItemReset.ItemId, localCount, localLimit, localCount);
                                itemInItemReset.LocalLimit = localCount;
                            }

                            break;
                        }

                        case ItemInCharacterReset _: // no local limit check
                            break;
                        case ItemInEquipmentReset itemInEquipmentReset: // no local limit check but wear local check
                        {
                            // check wear location
                            ItemBlueprintBase blueprint = GetItemBlueprint(itemInEquipmentReset.ItemId);
                            if (blueprint != null)
                            {
                                if (blueprint.WearLocation == WearLocations.None)
                                {
                                    WearLocations[] wearLocations = itemInEquipmentReset.EquipmentSlot.ToWearLocations().ToArray();
                                    WearLocations newWearLocation = wearLocations.FirstOrDefault(); // TODO: which one to choose from ?
                                    Log.Default.WriteLine(LogLevels.Error, "Room {0}: E: item {1} has no wear location but reset equipment slot {2} -> modifying item wear location to {3}", room.Blueprint.Id, itemInEquipmentReset.ItemId, itemInEquipmentReset.EquipmentSlot, newWearLocation);
                                    blueprint.WearLocation = newWearLocation;
                                }
                                else
                                {
                                    EquipmentSlots[] equipmentSlots = blueprint.WearLocation.ToEquipmentSlots().ToArray();
                                    if (equipmentSlots.All(x => x != itemInEquipmentReset.EquipmentSlot))
                                    {
                                        EquipmentSlots newEquipmentSlot = equipmentSlots.First();
                                        Log.Default.WriteLine(LogLevels.Error, "Room {0}: E: item {1} reset equipment slot {2} incompatible with wear location {3} -> modifying reset equipment slot to {4}", room.Blueprint.Id, itemInEquipmentReset.ItemId, itemInEquipmentReset.EquipmentSlot, blueprint.WearLocation, newEquipmentSlot);
                                        itemInEquipmentReset.EquipmentSlot = newEquipmentSlot;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }

            // Check global = 0 but found in reset
            foreach (IRoom room in _rooms.Where(x => x.Blueprint.Resets?.Any() == true).OrderBy(x => x.Blueprint.Id))
            {
                foreach (ResetBase reset in room.Blueprint.Resets)
                {
                    switch (reset)
                    {
                        case CharacterReset characterReset:
                        {
                            int globalCount = characterResetGlobalCountById[characterReset.CharacterId];
                            if (characterReset.GlobalLimit == 0)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Room {0}: M: character {1} found {2} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, characterReset.CharacterId, globalCount);
                                characterReset.GlobalLimit = 1;
                            }
                            else if (characterReset.GlobalLimit != -1 && characterReset.GlobalLimit < globalCount)
                                Log.Default.WriteLine(LogLevels.Warning, "Room {0}: M: character {1} found {2} times in world but global limit is {3}", room.Blueprint.Id, characterReset.CharacterId, globalCount, characterReset.GlobalLimit);

                            break;
                        }

                        case ItemInRoomReset _: // no global count check
                            break;
                        case ItemInItemReset _: // no global count check
                            break;
                        case ItemInCharacterReset itemInCharacterReset:
                        {
                            int globalCount = itemResetCountGlobalById[itemInCharacterReset.ItemId];
                            if (itemInCharacterReset.GlobalLimit == 0)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Room {0}: G: item {1} found {2} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, itemInCharacterReset.ItemId, globalCount);
                                itemInCharacterReset.GlobalLimit = 1;
                            }
                            else if (itemInCharacterReset.GlobalLimit != -1 && itemInCharacterReset.GlobalLimit < globalCount)
                                Log.Default.WriteLine(LogLevels.Warning, "Room {0}: G: item {1} found {2} times in world but global limit is {3}", room.Blueprint.Id, itemInCharacterReset.ItemId, globalCount, itemInCharacterReset.GlobalLimit);

                            break;
                        }
                        case ItemInEquipmentReset itemInEquipmentReset:
                        {
                            int globalCount = itemResetCountGlobalById[itemInEquipmentReset.ItemId];
                            if (itemInEquipmentReset.GlobalLimit == 0)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Room {0}: E: item {1} found {2} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, itemInEquipmentReset.ItemId, globalCount);
                                itemInEquipmentReset.GlobalLimit = 1;
                            }
                            else if (itemInEquipmentReset.GlobalLimit != -1 && itemInEquipmentReset.GlobalLimit < globalCount)
                                Log.Default.WriteLine(LogLevels.Warning, "Room {0}: E: item {1} found {2} times in world but global limit is {3}", room.Blueprint.Id, itemInEquipmentReset.ItemId, globalCount, itemInEquipmentReset.GlobalLimit);

                            break;
                        }
                    }
                }
            }

            Log.Default.WriteLine(LogLevels.Info, "Resets fixed");
        }

    }
}
