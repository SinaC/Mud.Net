using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Input;

namespace Mud.Server
{
    public static class FindHelpers
    {
        private readonly static Func<string, string, bool> StringEquals = (s, s1) => String.Equals(s, s1, StringComparison.InvariantCultureIgnoreCase);

        public static IPlayer FindByName(IEnumerable<IPlayer> list, string name)
        {
            return list.FirstOrDefault(x => StringEquals(x.Name, name));
        }

        public static IPlayer FindByName(IEnumerable<IPlayer> list, CommandParameter parameter)
        {
            return list.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public static T FindByName<T>(IEnumerable<T> list, string name)
            where T : IEntity
        {
            return list.FirstOrDefault(x => StringEquals(x.Name, name));
        }

        public static T FindByName<T>(IEnumerable<T> list, CommandParameter parameter)
            where T:IEntity
        {
            return list.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }
    }
}
