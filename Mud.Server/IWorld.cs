using System;
using System.Collections.Generic;
using Mud.Server.Blueprints;
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
        
        IExit AddExit(IRoom from, IRoom to, ServerOptions.ExitDirections direction, bool bidirectional);
        
        ICharacter AddCharacter(Guid guid, string name, IRoom room); // Impersonated
        ICharacter AddCharacter(Guid guid, CharacterBlueprint blueprint, IRoom room); // Non-impersonated
        
        IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container);
        IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container);
        IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container);
        IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container);
        IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom container, ICharacter victim);

        void RemoveCharacter(ICharacter character);
        void RemoveItem(IItem item);
        void RemoveRoom(IRoom room);

        // TODO: remove
        // TEST PURPOSE
        ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false);

        void Update(); // called every pulse
    }
}
