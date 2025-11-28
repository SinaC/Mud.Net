using Mud.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Random;

namespace Mud.Server.Common.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, IRandomManager randomManager)
    {
        return source.Shuffle(randomManager.Next);
    }

    public static T? Random<T>(this IEnumerable<T> source, IRandomManager randomManager)
    {
        return randomManager.Random(source);
    }
}
