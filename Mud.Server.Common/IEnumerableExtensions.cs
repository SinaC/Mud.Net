using Mud.Common;
using Mud.Server.Random;
using System.Collections.Generic;

namespace Mud.Server.Common
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, IRandomManager randomManager)
        {
            return source.Shuffle(randomManager.Next);
        }

        public static T Random<T>(this IEnumerable<T> source, IRandomManager randomManager)
        {
            return randomManager.Random(source);
        }
    }
}
