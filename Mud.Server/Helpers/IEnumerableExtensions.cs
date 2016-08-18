using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Helpers
{
    public static class IEnumerableExtensions
    {
        //http://stackoverflow.com/questions/22152160/linq-fill-function
        public static IEnumerable<T> Fill<T>(this IEnumerable<T> source, int length)
        {
            int i = 0;
            // use "Take" in case "length" is smaller than the source's length.
            foreach (var item in source.Take(length))
            {
                yield return item;
                i++;
            }
            for (; i < length; i++)
                yield return default(T);
        }
    }
}
