using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mud.Common
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> source, Func<T, object> propertySelector) => source.GroupBy(propertySelector).Select(x => x.First());

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

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Func<int, int, int> randomNextFunc)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (randomNextFunc == null) throw new ArgumentNullException(nameof(randomNextFunc));

            return source.ShuffleIterator(randomNextFunc);
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Func<int, int, int> randomNextFunc)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = randomNextFunc(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        //public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> collection, int batchSize)
        //{
        //    List<T> nextbatch = new List<T>(batchSize);
        //    foreach (T item in collection)
        //    {
        //        nextbatch.Add(item);
        //        if (nextbatch.Count == batchSize)
        //        {
        //            yield return nextbatch;
        //            nextbatch = new List<T>(batchSize);
        //        }
        //    }
        //    if (nextbatch.Count > 0)
        //        yield return nextbatch;
        //}
        //https://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source,
            int chunkSize)
        {
            // Validate parameters.
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize),
                "The chunkSize parameter must be a positive value.");

            // Call the internal implementation.
            return source.ChunkInternal(chunkSize);
        }

        private static IEnumerable<IEnumerable<T>> ChunkInternal<T>(
            this IEnumerable<T> source, int chunkSize)
        {
            // Validate parameters.
            Debug.Assert(source != null);
            Debug.Assert(chunkSize > 0);

            // Get the enumerator.  Dispose of when done.
            using (IEnumerator<T> enumerator = source.GetEnumerator())
                do
                {
                    // Move to the next element.  If there's nothing left
                    // then get out.
                    if (!enumerator.MoveNext()) yield break;

                    // Return the chunked sequence.
                    yield return ChunkSequence(enumerator, chunkSize);
                } while (true);
        }

        private static IEnumerable<T> ChunkSequence<T>(IEnumerator<T> enumerator,
            int chunkSize)
        {
            // Validate parameters.
            Debug.Assert(enumerator != null);
            Debug.Assert(chunkSize > 0);

            // The count.
            int count = 0;

            // There is at least one item.  Yield and then continue.
            do
            {
                // Yield the item.
                yield return enumerator.Current;
            } while (++count < chunkSize && enumerator.MoveNext());
        }
    }
}
