using System.Collections.Generic;

namespace Mud.Server.Common
{
    public static class DictionaryExtensions
    {
        public static void Increment(this Dictionary<int, int> dictionary, int key)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key]++;
            else
                dictionary.Add(key, 1);
        }
    }
}
