using System;
using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server
{
    public interface IWorld
    {
        IReadOnlyCollection<RoomBlueprint> GetRoomBlueprints();
        IReadOnlyCollection<CharacterBlueprint> GetCharacterBlueprints();
        IReadOnlyCollection<ItemBlueprintBase> GetItemBlueprints();

        RoomBlueprint GetRoomBlueprint(int id);
        CharacterBlueprint GetCharacterBlueprint(int id);
        ItemBlueprintBase GetItemBlueprint(int id);

        void AddRoomBlueprint(RoomBlueprint blueprint);
        void AddCharacterBlueprint(CharacterBlueprint blueprint);
        void AddItemBlueprint(ItemBlueprintBase blueprint);

        IReadOnlyCollection<IRoom> GetRooms();
        IReadOnlyCollection<ICharacter> GetCharacters();
        IReadOnlyCollection<IItem> GetItems();

        IRoom AddRoom(Guid guid, RoomBlueprint blueprint);

        IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction);

        ICharacter AddCharacter(Guid guid, string name, IClass pcClass, IRace pcRace, Sex pcSex, IRoom room); // Impersonated
        ICharacter AddCharacter(Guid guid, CharacterBlueprint blueprint, IRoom room); // Non-impersonated
        
        IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container);
        IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container);
        IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container);
        IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container);
        IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom container, ICharacter victim);
        IItemShield AddItemShield(Guid guid, ItemShieldBlueprint blueprint, IContainer container);
        IItemFurniture AddItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer container);

        IAura AddAura(ICharacter victim, IAbility ability, ICharacter source, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds, bool visible);
        IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks); // Hot
        IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks); // Dot

        void RemoveCharacter(ICharacter character);
        void RemoveItem(IItem item);
        void RemoveRoom(IRoom room);

        // TODO: remove
        // TEST PURPOSE
        ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false);

        void Update(); // called every pulse
        void Cleanup(); // called once outputs has been processed (before next loop)
    }
}
