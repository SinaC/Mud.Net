using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Input;

namespace Mud.Server.Helpers
{
    public static class FindHelpers
    {
        private readonly static Func<string, string, bool> StringEquals = (s, s1) => String.Equals(s, s1, StringComparison.InvariantCultureIgnoreCase);
        private readonly static Func<string, string, bool> StringStartWith = (s, s1) => s.StartsWith(s1, StringComparison.InvariantCultureIgnoreCase);

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
    }
}
