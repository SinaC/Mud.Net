using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Container;
using Mud.Server.Input;

namespace Mud.Server.Helpers
{
    public static class FindHelpers
    {
        private static IWorld World => DependencyContainer.Current.GetInstance<IWorld>();

        public static readonly Func<string, string, bool> StringEquals = (s, s1) => string.Equals(s, s1, StringComparison.InvariantCultureIgnoreCase);
        public static readonly Func<string, string, bool> StringStartsWith = (s, s1) => s.StartsWith(s1, StringComparison.InvariantCultureIgnoreCase);
        public static readonly Func<IEnumerable<string>, string, bool> StringListEquals = (enumerable, s) => enumerable.Any(x => StringEquals(x, s));
        public static readonly Func<IEnumerable<string>, string, bool> StringListStartsWith = (enumerable, s) => enumerable.Any(x => StringStartsWith(x, s));
        // every item in enumerable1 must be found in enumerable
        public static readonly Func<IEnumerable<string>, IEnumerable<string>, bool> StringListsEquals = (enumerable, enumerable1) => enumerable1.All(x => enumerable.Any(y => StringEquals(y, x)));
        public static readonly Func<IEnumerable<string>, IEnumerable<string>, bool> StringListsStartsWith = (enumerable, enumerable1) => enumerable1.All(x => enumerable.Any(y => StringStartsWith(y, x)));

        //// Search in room content, then in inventory, then in equipment
        //public static IItem FindItemHere(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
        //{
        //    return FindByName(character.Room.Content.Where(character.CanSee), parameter, perfectMatch)
        //           ?? FindByName(character.Content.Where(character.CanSee), parameter, perfectMatch)
        //           ?? (FindByName(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)), x => x.Item, parameter, perfectMatch) ?? EquipedItem.NullObject).Item;
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
        public static IItem FindItemHere(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
        {
            return FindByName(
                character.Room.Content.Where(character.CanSee)
                    .Concat(character.Inventory.Where(character.CanSee))
                    .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item)),
                parameter, perfectMatch);
        }

        // Players/Admin
        public static IPlayer FindByName(IEnumerable<IPlayer> list, string name, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.FirstOrDefault(x => StringEquals(x.Name, name))
                : list.FirstOrDefault(x => StringStartsWith(x.Name, name));
        }

        public static IPlayer FindByName(IEnumerable<IPlayer> list, CommandParameter parameter, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public static IAdmin FindByName(IEnumerable<IAdmin> list, string name, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.FirstOrDefault(x => StringEquals(x.Name, name))
                : list.FirstOrDefault(x => StringStartsWith(x.Name, name));
        }

        public static IAdmin FindByName(IEnumerable<IAdmin> list, CommandParameter parameter, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        // Entity
        public static T FindByName<T>(IEnumerable<T> list, CommandParameter parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringListsEquals(x.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringListsStartsWith(x.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1);
        }

        public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, CommandParameter parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringListsEquals(x.Keywords, parameter.Tokens))
                : list.Where(x => StringListsStartsWith(x.Keywords, parameter.Tokens));
        }

        public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, string parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringListEquals(x.Keywords, parameter))
                : list.Where(x => StringListStartsWith(x.Keywords, parameter));
        }

        //public static IEnumerable<IPlayer> FindAllByName(IEnumerable<IPlayer> list, CommandParameter parameter, bool perfectMatch = false)
        //{
        //    return perfectMatch
        //        ? list.Where(x => StringEquals(x.Name, parameter.Value))
        //        : list.Where(x => StringStartsWith(x.Name, parameter.Value));
        //}

        public static T FindByName<T, TEntity>(IEnumerable<T> collection, Func<T, TEntity> getItemFunc, CommandParameter parameter, bool perfectMatch = false)
            where TEntity : IEntity
        {
            return perfectMatch
                ? collection.Where(x => StringListsEquals(getItemFunc(x).Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1)
                : collection.Where(x => StringListsStartsWith(getItemFunc(x).Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1);
        }

        // FindLocation
        public static IRoom FindLocation(CommandParameter parameter)
        {
            if (parameter.IsNumber)
            {
                int id = parameter.AsNumber;
                return World.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            }

            ICharacter victim = FindByName(World.Characters, parameter);
            if (victim != null)
                return victim.Room;

            IItem item = FindByName(World.Items, parameter);
            return item?.ContainedInto as IRoom;
        }

        public static IRoom FindLocation(ICharacter asker, CommandParameter parameter)
        {
            if (parameter.IsNumber)
            {
                int id = parameter.AsNumber;
                return World.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            }

            ICharacter victim = FindChararacterInWorld(asker, parameter);
            if (victim != null)
                return victim.Room;

            IItem item = FindItemInWorld(asker, parameter);
            return item?.ContainedInto as IRoom;
        }

        // FindCharacter
        public static ICharacter FindChararacterInWorld(ICharacter asker, CommandParameter parameter) // equivalent to get_char_world in handler.C:3511
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

        public static INonPlayableCharacter FindNonPlayableChararacterInWorld(ICharacter asker, CommandParameter parameter) // equivalent to get_char_world in handler.C:3511
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

        public static IPlayableCharacter FindPlayableChararacterInWorld(ICharacter asker, CommandParameter parameter) // equivalent to get_char_world in handler.C:3511
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
        public static IItem FindItemInWorld(ICharacter asker, CommandParameter parameter) // equivalent to get_obj_world in handler.C:3702
        {
            IItem hereItem = FindItemHere(asker, parameter);
            if (hereItem != null)
                return hereItem;

            IItem inWorldItem = FindByName(World.Items.Where(asker.CanSee), parameter);
            return inWorldItem;
        }
    }
}
