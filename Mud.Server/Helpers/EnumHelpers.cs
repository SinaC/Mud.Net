using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Helpers
{
    public static class EnumHelpers
    {
        public static IEnumerable<T> GetValues<T>()
            where T:struct // cannot constraint on enum so using struct to limit a little bit
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
