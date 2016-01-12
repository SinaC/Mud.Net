using System;

namespace Mud.Server.Helpers
{
    public class RandomizeHelpers
    {
        public Random Randomizer { get; private set; }

        #region Singleton

        private static readonly Lazy<RandomizeHelpers> Lazy = new Lazy<RandomizeHelpers>(() => new RandomizeHelpers());

        public static RandomizeHelpers Instance
        {
            get { return Lazy.Value; }
        }

        private RandomizeHelpers()
        {
            Randomizer = new Random();
        }

        #endregion

        public int Dice(int count, int value)
        {
            //return Enumerable.Range(0, count).Sum(x => Randomizer.Next(value)+1);
            return Randomizer.Next(count, count*value + 1); // faster :)
        }
    }
}
