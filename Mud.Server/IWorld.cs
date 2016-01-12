using System;
using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Server.Server;

namespace Mud.Server
{
    public interface IWorld
    {
        IReadOnlyCollection<IRoom> GetRooms();
        IReadOnlyCollection<ICharacter> GetCharacters();
        IReadOnlyCollection<IItem> GetItems();

        IRoom AddRoom(Guid guid, RoomBlueprint blueprint);

        IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ServerOptions.ExitDirections direction);
        
        ICharacter AddCharacter(Guid guid, string name, IRoom room); // Impersonated
        ICharacter AddCharacter(Guid guid, CharacterBlueprint blueprint, IRoom room); // Non-impersonated
        
        IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container);
        IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container);
        IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container);
        IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container);
        IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom container, ICharacter victim);

        IAura AddAura(ICharacter victim, string name, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds, bool visible);
        IPeriodicAura AddPeriodicAura(ICharacter victim, string name, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks); // Hot
        IPeriodicAura AddPeriodicAura(ICharacter victim, string name, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks); // Dot

        void RemoveCharacter(ICharacter character);
        void RemoveItem(IItem item);
        void RemoveRoom(IRoom room);

        // TODO: remove
        // TEST PURPOSE
        ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false);

        void Update(); // called every pulse
    }
}
