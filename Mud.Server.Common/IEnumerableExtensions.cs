using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Common
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
                yield return default;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(RandomizeHelpers.Instance.Randomizer);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
