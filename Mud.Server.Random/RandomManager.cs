using Mud.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mud.Server.Random
{
    public class RandomManager : IRandomManager
    {
        private readonly System.Random _random;

        public RandomManager()
        {
            _random = new System.Random();
        }

        public RandomManager(int seed)
        {
            _random = new System.Random(seed);
        }

        public int Next(int maxExcluded)
        {
            return _random.Next(maxExcluded);
        }

        public int Next(int minIncluded, int maxExcluded)
        {
            return _random.Next(minIncluded, maxExcluded);
        }

        public bool Chance(int percentage)
        {
            return 1 + _random.Next(100) < percentage;
        }

        public int Dice(int count, int value)
        {
            //return Enumerable.Range(0, count).Sum(x => _randomizer.Next(value)+1);
            return _random.Next(count, count*value + 1); // faster :)
        }

        public int Range(int min, int max)
        {
            return _random.Next(min, max+1);
        }

        // https://stackoverflow.com/questions/6651554/random-number-in-long-range-is-this-the-way
        public long Range(long min, long max)
        {
            if (min == max)
                return 0L;
            if (min > max)
                return max;
            //Working with ulong so that modulo works correctly with values > long.MaxValue
            ulong uRange = (ulong)(max - min);

            //Prevent a modulo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                _random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        public int Fuzzy(int number)
        {
            switch (_random.Next(4))
            {
                case 0: number -= 1; break;
                case 3: number += 1; break;
            }
            return Math.Max(1, number);
        }

        public T Random<T>()
            where T : Enum
        {
            return Random<T>(EnumHelpers.GetValues<T>());
        }

        public T Random<T>(IEnumerable<T> values) // https://stackoverflow.com/questions/648196/random-row-from-linq-to-sql/648240#648240
        {
            T current = default;
            int count = 0;
            foreach (T element in values)
            {
                count++;
                if (_random.Next(count) == 0)
                    current = element;
            }
            if (count == 0)
                return default;
            return current;
        }

        public T RandomOccurancy<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            IList<IOccurancy<T>> list = occurancies as IList<IOccurancy<T>> ?? occurancies.ToList();

            int sum = list.Sum(x => x.Occurancy);
            if (sum <= 0)
                return default;

            int random = _random.Next(sum);

            int range = 0;
            foreach (IOccurancy<T> occurancy in list)
            {
                range += occurancy.Occurancy;
                if (random < range)
                    return occurancy.Value;
            }
            Debug.Assert(false, "RandomOccurancy");
            return default;
        }

        public T RandomOccurancy<T>(IEnumerable<IOccurancy<T>> occurancies, IEnumerable<T> history)
        {
            IEnumerable<IOccurancy<T>> list = (occurancies as IList<IOccurancy<T>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value));

            return RandomOccurancy(list);
        }

        public int SumOccurancies<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            return occurancies.Aggregate(0, (n, i) => n + i.Occurancy);
        }

        public T RandomOccurancy<T, U>(IEnumerable<T> occurancies)
            where T : IOccurancy<U>
        {
            IList<T> list = occurancies as IList<T> ?? occurancies.ToList();

            int sum = list.Sum(x => x.Occurancy);
            if (sum <= 0)
                return default;

            int random = _random.Next(sum);

            int range = 0;
            foreach (T occurancy in list)
            {
                range += occurancy.Occurancy;
                if (random < range)
                    return occurancy;
            }
            Debug.Assert(false, "RangeRandom");
            return default;
        }
    }
}
