using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Helpers
{
    public static class EnumHelpers
    {
         // cannot constraint on enum so using struct to limit a little bit

        public static IEnumerable<T> GetValues<T>()
            where T:struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static bool TryFindByName<T>(string name, out T value)
            where T : struct
        {
            return Enum.TryParse(name, true, out value);
        }

        public static bool TryFindByPrefix<T>(string prefix, out T value)
            where T : struct
        {
            string name = Enum.GetNames(typeof (T)).FirstOrDefault(x => FindHelpers.StringStartWith(x, prefix));
            if (!String.IsNullOrWhiteSpace(name))
                return Enum.TryParse(name, true, out value);
            value = default(T);
            return false;
        }

        public static int GetCount<T>()
            where T : struct
        {
            return Enum.GetValues(typeof(T)).Length;
        }
    }
}
