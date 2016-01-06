﻿using System;
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

        public static int GetCount<T>()
            where T : struct
        {
            return Enum.GetValues(typeof(T)).Length;
        }
    }
}
