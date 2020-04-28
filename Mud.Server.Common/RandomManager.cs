using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mud.Server.Common
{
    public class RandomManager : IRandomManager
    {
        private Random _randomizer { get; }

        public RandomManager()
        {
            _randomizer = new Random();
        }

        public RandomManager(int seed)
        {
            _randomizer = new Random(seed);
        }

        public int Next(int maxExcluded)
        {
            return _randomizer.Next(maxExcluded);
        }

        public int Next(int minIncluded, int maxExcluded)
        {
            return _randomizer.Next(minIncluded, maxExcluded);
        }

        public bool Chance(int percentage)
        {
            return 1 + _randomizer.Next(100) < percentage;
        }

        public int Dice(int count, int value)
        {
            //return Enumerable.Range(0, count).Sum(x => _randomizer.Next(value)+1);
            return _randomizer.Next(count, count*value + 1); // faster :)
        }

        public int Range(int min, int max)
        {
            return _randomizer.Next(min, max+1);
        }

        public int Fuzzy(int number)
        {
            switch (_randomizer.Next(4))
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
                if (_randomizer.Next(count) == 0)
                    current = element;
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }

        public T RandomOccurancy<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            IList<IOccurancy<T>> list = occurancies as IList<IOccurancy<T>> ?? occurancies.ToList();

            int sum = list.Sum(x => x.Occurancy);
            if (sum <= 0)
                return default;

            int random = _randomizer.Next(sum);

            int range = 0;
            foreach (IOccurancy<T> occurancy in list)
            {
                range += occurancy.Occurancy;
                if (random < range)
                    return occurancy.Value;
            }
            Debug.Assert(false, "Random");
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

            int random = _randomizer.Next(sum);

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
