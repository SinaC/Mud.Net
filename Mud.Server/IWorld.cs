using System;
using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Input;
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
        
        IItem AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container);
        IItem AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container);
        IItem AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container);
        IItem AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container);

        void RemoveCharacter(ICharacter character, bool pull/*TODO better name*/);

        // TODO: remove
        // TEST PURPOSE
        ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false);

        void Update(); // called every pulse
    }
}
