using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Input;

namespace Mud.Server.Helpers
{
    public static class FindHelpers // TODO: check if a keyword contains parameter instead of name == parameter  ???
    {
        private static readonly Func<string, string, bool> StringEquals = (s, s1) => String.Equals(s, s1, StringComparison.InvariantCultureIgnoreCase);
        private static readonly Func<string, string, bool> StringStartWith = (s, s1) => s.StartsWith(s1, StringComparison.InvariantCultureIgnoreCase);

        // Search in room content, then in inventory, then in equipment
        public static IItem FindItemByName2(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // equivalent do get_obj_here in handler.C:3680
        {
            return FindByName(character.Room.Content.Where(character.CanSee), parameter, perfectMatch)
                   ?? FindByName(character.Content.Where(character.CanSee), parameter, perfectMatch)
                   ?? (FindByName(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)), x => x.Item, parameter, perfectMatch) ?? EquipmentSlot.NullObject).Item;
        }

        // Concat room content, inventory and equipment, then search
        public static IItem FindItemByName(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // equivalent do get_obj_here in handler.C:3680
        {
            return FindByName(
                character.Room.Content.Where(character.CanSee)
                    .Concat(character.Content.Where(character.CanSee))
                    .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item)),
                parameter, perfectMatch);
        }

        // Players/Admin
        public static IPlayer FindByName(IEnumerable<IPlayer> list, string name, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.FirstOrDefault(x => StringEquals(x.Name, name))
                : list.FirstOrDefault(x => StringStartWith(x.Name, name));
        }

        public static IPlayer FindByName(IEnumerable<IPlayer> list, CommandParameter parameter, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringStartWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public static IAdmin FindByName(IEnumerable<IAdmin> list, string name, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.FirstOrDefault(x => StringEquals(x.Name, name))
                : list.FirstOrDefault(x => StringStartWith(x.Name, name));
        }

        public static IAdmin FindByName(IEnumerable<IAdmin> list, CommandParameter parameter, bool perfectMatch = false)
        {
            return perfectMatch
                ? list.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringStartWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        // Entity
        public static T FindByName<T>(IEnumerable<T> list, string name, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.FirstOrDefault(x => StringEquals(x.Name, name))
                : list.FirstOrDefault(x => StringStartWith(x.Name, name));
        }

        public static T FindByName<T>(IEnumerable<T> list, CommandParameter parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : list.Where(x => StringStartWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, string name, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringEquals(x.Name, name))
                : list.Where(x => StringStartWith(x.Name, name));
        }

        public static T FindByName<T, TEntity>(IEnumerable<T> collection, Func<T, TEntity> getItemFunc, CommandParameter parameter, bool perfectMatch = false)
            where TEntity : IEntity
        {
            return perfectMatch
                ? collection.Where(x => StringEquals(getItemFunc(x).Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
                : collection.Where(x => StringStartWith(getItemFunc(x).Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }
    }
}
