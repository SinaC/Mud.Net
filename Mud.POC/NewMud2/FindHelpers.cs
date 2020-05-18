using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Input;

namespace Mud.POC.NewMud2
{
    public static class FindHelpers
    {
        public static TEntity FindByName<TEntity>(IEnumerable<TEntity> entities, CommandParameter parameter)
            where TEntity : IEntity
            => entities.FirstOrDefault(x => x.Name == parameter.Value); // TODO: real implementation is in Server.Helpers

        public static ICharacter FindByName(ICharacter user, Func<ICharacter, bool> filter, CommandParameter parameter, params CharacterFindLocations[] locations)
        {
            IEnumerable<ICharacter> characters = Enumerable.Empty<ICharacter>();
            foreach (CharacterFindLocations location in locations)
            {
                if (filter != null)
                    characters = characters.Concat(CharactersFromLocation(user, location).Where(filter));
                else
                    characters = characters.Concat(CharactersFromLocation(user, location));
            }

            return FindByName(characters, parameter);
        }

        public static IItem FindByName(ICharacter user, Func<IItem, bool> filter, CommandParameter parameter, params ItemFindLocations[] locations)
        {
            IEnumerable<IItem> items = Enumerable.Empty<IItem>();
            foreach (ItemFindLocations location in locations)
            {
                if (filter != null)
                    items = items.Concat(ItemsFromLocation(user, location).Where(filter));
                else
                    items = items.Concat(ItemsFromLocation(user, location));
            }

            return FindByName(items, parameter);
        }

        private static IEnumerable<ICharacter> CharactersFromLocation(ICharacter user, CharacterFindLocations location)
        {
            switch (location)
            {
                case CharacterFindLocations.CharacterInParty:
                    return user.PartyMembers;
                case CharacterFindLocations.CharacterInRoom:
                    return user.Room.People;
                case CharacterFindLocations.CharacterInArea:
                    return user.Room.Area.Characters;
                case CharacterFindLocations.CharacterInWorld:
                    return user.World.Characters;
                default:
                    // TODO: log
                    return user.Room.People;
            }
        }

        private static IEnumerable<IItem> ItemsFromLocation(ICharacter user, ItemFindLocations location)
        {
            switch (location)
            {
                case ItemFindLocations.ItemInRoom:
                    return user.Room.Content;
                case ItemFindLocations.ItemInInventory:
                    return user.Inventory;
                case ItemFindLocations.ItemInEquipment:
                    return user.Equipments;
                case ItemFindLocations.ItemInArea:
                    return user.Room.Area.Rooms.SelectMany(x => x.Content);
                case ItemFindLocations.ItemInWorld:
                    return user.World.Items;
                default:
                    // TODO: log
                    return user.Inventory;
            }
        }
    }

    public enum ItemFindLocations
    {
        ItemInRoom,
        ItemInInventory,
        ItemInEquipment,
        ItemInArea,
        ItemInWorld
    }

    public enum CharacterFindLocations
    {
        CharacterInParty,
        CharacterInRoom,
        CharacterInArea,
        CharacterInWorld,
    }
}
