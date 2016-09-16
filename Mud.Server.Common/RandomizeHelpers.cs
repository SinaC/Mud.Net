using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mud.Server.Common
{
    public class RandomizeHelpers
    {
        public Random Randomizer { get; }

        #region Singleton

        private static readonly Lazy<RandomizeHelpers> Lazy = new Lazy<RandomizeHelpers>(() => new RandomizeHelpers());

        public static RandomizeHelpers Instance => Lazy.Value;

        private RandomizeHelpers()
        {
            Randomizer = new Random();
        }

        #endregion

        public bool Chance(int percentage)
        {
            return 1 + Randomizer.Next(100) <= percentage;
        }

        public int Dice(int count, int value)
        {
            //return Enumerable.Range(0, count).Sum(x => Randomizer.Next(value)+1);
            return Randomizer.Next(count, count*value + 1); // faster :)
        }

        public T Random<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            IList<IOccurancy<T>> list = occurancies as IList<IOccurancy<T>> ?? occurancies.ToList();

            int sum = list.Sum(x => x.Occurancy);
            if (sum <= 0)
                return default(T);

            int random = Randomizer.Next(sum);

            int range = 0;
            foreach (IOccurancy<T> occurancy in list)
            {
                range += occurancy.Occurancy;
                if (random < range)
                    return occurancy.Value;
            }
            Debug.Assert(false, "RangeRandom");
            return default(T);
        }

        public T Random<T>(IEnumerable<IOccurancy<T>> occurancies, IEnumerable<T> history)
        {
            IEnumerable<IOccurancy<T>> list = (occurancies as IList<IOccurancy<T>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value));

            return Random(list);
        }

        public int SumOccurancies<T>(IEnumerable<IOccurancy<T>> occurancies)
        {
            return occurancies.Aggregate(0, (n, i) => n + i.Occurancy);
        }

        public T Random<T, U>(IEnumerable<T> occurancies)
            where T : IOccurancy<U>
        {
            IList<T> list = occurancies as IList<T> ?? occurancies.ToList();

            int sum = list.Sum(x => x.Occurancy);
            if (sum <= 0)
                return default(T);

            int random = Randomizer.Next(sum);

            int range = 0;
            foreach (T occurancy in list)
            {
                range += occurancy.Occurancy;
                if (random < range)
                    return occurancy;
            }
            Debug.Assert(false, "RangeRandom");
            return default(T);
        }
    }
}
