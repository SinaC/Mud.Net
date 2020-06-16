using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Common;
using Mud.Container;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;

namespace Mud.Server.Common
{
    // TODO: don't use GetInstance
    public static class FindHelpers
    {
        private static IWorld World => DependencyContainer.Current.GetInstance<IWorld>();
        private static IRoomManager RoomManager => DependencyContainer.Current.GetInstance<IRoomManager>();
        private static IItemManager ItemManager => DependencyContainer.Current.GetInstance<IItemManager>();

        //// Search in room content, then in inventory, then in equipment
        //public static IItem FindItemHere(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
        //{
        //    return FindByName(character.Room.Content.Where(character.CanSee), parameter, perfectMatch)
        //           ?? FindByName(character.Content.Where(character.CanSee), parameter, perfectMatch)
        //           ?? (FindByName(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)), x => x.Item, parameter, perfectMatch) ?? EquippedItem.NullObject).Item;
        //}

        //// Concat room content, inventory and equipment, then search
        //public static IItem FindCharacterItemByNameInRoomAndInventoryAndEquipment(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // should replace FindItemHere
        //{
        //    return FindByName(
        //        character.Room.Content.Where(character.CanSee)
        //            .Concat(character.Content.Where(character.CanSee))
        //            .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item)),
        //        parameter, perfectMatch);
        //}

        // Concat room content, inventory and equipment, then search
        public static IItem FindItemHere(ICharacter character, ICommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
        {
            return FindByName(
                character.Room.Content.Where(character.CanSee)
                    .Concat(character.Inventory.Where(character.CanSee))
                    .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item)),
                parameter, perfectMatch);
        }
        public static T FindItemHere<T>(ICharacter character, ICommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
            where T:IItem
        {
            return FindByName(
                character.Room.Content.Where(character.CanSee).OfType<T>()
                    .Concat(character.Inventory.Where(character.CanSee).OfType<T>())
                    .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item).OfType<T>()),
                parameter, perfectMatch);
        }

        // Players/Admin
        public static IPlayer FindByName(IEnumerable<IPlayer> list, string name, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name))
                : list.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
        }

        public static IPlayer FindByName(IEnumerable<IPlayer> list, ICommandParameter parameter, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.Where(x => StringCompareHelpers.StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public static IAdmin FindByName(IEnumerable<IAdmin> list, string name, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name))
                : list.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
        }

        public static IAdmin FindByName(IEnumerable<IAdmin> list, ICommandParameter parameter, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.Where(x => StringCompareHelpers.StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        // Entity
        public static T FindByName<T>(IEnumerable<T> list, ICommandParameter parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringCompareHelpers.StringListsEquals(x.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringCompareHelpers.StringListsStartsWith(x.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1);
        }

        public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, ICommandParameter parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringCompareHelpers.StringListsEquals(x.Keywords, parameter.Tokens))
                : list.Where(x => StringCompareHelpers.StringListsStartsWith(x.Keywords, parameter.Tokens));
        }

        public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, string parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringCompareHelpers.StringListEquals(x.Keywords, parameter))
                : list.Where(x => StringCompareHelpers.StringListStartsWith(x.Keywords, parameter));
        }

        //public static IEnumerable<IPlayer> FindAllByName(IEnumerable<IPlayer> list, CommandParameter parameter, bool perfectMatch = false)
        //{
        //    return perfectMatch
        //        ? list.Where(x => StringEquals(x.Name, parameter.Value))
        //        : list.Where(x => StringStartsWith(x.Name, parameter.Value));
        //}

        public static T FindByName<T, TEntity>(IEnumerable<T> collection, Func<T, TEntity> getItemFunc, ICommandParameter parameter, bool perfectMatch = false)
            where TEntity : IEntity
        {
            return perfectMatch
                ? collection.Where(x => StringCompareHelpers.StringListsEquals(getItemFunc(x).Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1)
                : collection.Where(x => StringCompareHelpers.StringListsStartsWith(getItemFunc(x).Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1);
        }

        // FindLocation
        public static IRoom FindLocation(ICommandParameter parameter)
        {
            if (parameter.IsNumber)
            {
                int id = parameter.AsNumber;
                return RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            }

            ICharacter victim = FindByName(World.Characters, parameter);
            if (victim != null)
                return victim.Room;

            IItem item = FindByName(ItemManager.Items, parameter);
            return item?.ContainedInto as IRoom;
        }

        public static IRoom FindLocation(ICharacter asker, ICommandParameter parameter)
        {
            if (parameter.IsNumber)
            {
                int id = parameter.AsNumber;
                return RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            }

            ICharacter victim = FindChararacterInWorld(asker, parameter);
            if (victim != null)
                return victim.Room;

            IItem item = FindItemInWorld(asker, parameter);
            return item?.ContainedInto as IRoom;
        }

        // FindCharacter
        public static ICharacter FindChararacterInWorld(ICharacter asker, ICommandParameter parameter) // equivalent to get_char_world in handler.C:3511
        {
            // In room
            ICharacter inRoom = FindByName(asker.Room.People.Where(asker.CanSee), parameter);
            if (inRoom != null)
                return inRoom;

            // In area
            //  players
            IPlayableCharacter inAreaPlayer = FindByName(asker.Room.Area.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
            if (inAreaPlayer != null)
                return inAreaPlayer;
            //  characters
            INonPlayableCharacter inAreaCharacter = FindByName(asker.Room.Area.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
            if (inAreaCharacter != null)
                return inAreaCharacter;

            // In world
            //  players
            IPlayableCharacter inWorldPlayer = FindByName(World.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
            if (inWorldPlayer != null)
                return inWorldPlayer;
            //  characters
            INonPlayableCharacter inWorldCharacter = FindByName(World.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
            return inWorldCharacter;
        }

        public static INonPlayableCharacter FindNonPlayableChararacterInWorld(ICharacter asker, ICommandParameter parameter) // equivalent to get_char_world in handler.C:3511
        {
            // In room
            INonPlayableCharacter inRoom = FindByName(asker.Room.NonPlayableCharacters.Where(asker.CanSee), parameter);
            if (inRoom != null)
                return inRoom;

            // In area
            INonPlayableCharacter inAreaCharacter = FindByName(asker.Room.Area.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
            if (inAreaCharacter != null)
                return inAreaCharacter;

            // In world
            INonPlayableCharacter inWorldCharacter = FindByName(World.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
            return inWorldCharacter;
        }

        public static IPlayableCharacter FindPlayableChararacterInWorld(ICharacter asker, ICommandParameter parameter) // equivalent to get_char_world in handler.C:3511
        {
            // In room
            IPlayableCharacter inRoom = FindByName(asker.Room.PlayableCharacters.Where(asker.CanSee), parameter);
            if (inRoom != null)
                return inRoom;

            // In area
            IPlayableCharacter inAreaPlayer = FindByName(asker.Room.Area.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
            if (inAreaPlayer != null)
                return inAreaPlayer;

            // In world
            IPlayableCharacter inWorldPlayer = FindByName(World.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
            return inWorldPlayer;
        }

        // FindItem
        public static IItem FindItemInWorld(ICharacter asker, ICommandParameter parameter) // equivalent to get_obj_world in handler.C:3702
        {
            IItem hereItem = FindItemHere(asker, parameter);
            if (hereItem != null)
                return hereItem;

            IItem inWorldItem = FindByName(ItemManager.Items.Where(asker.CanSee), parameter);
            return inWorldItem;
        }
    }
}
