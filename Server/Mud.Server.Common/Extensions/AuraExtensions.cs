using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;

namespace Mud.Server.Common.Extensions
{
    public static class AuraExtensions
    {
        public static bool HasAffect<TAffect>(this IEnumerable<IAura> auras)
            where TAffect: IAffect
            => auras.Any(x => x.Affects.OfType<TAffect>().Any());

        public static bool HasAffect<TAffect>(this IEnumerable<IAura> auras, Func<TAffect, bool> predicate)
            where TAffect : IAffect
            => auras.Any(x => x.Affects.OfType<TAffect>().Any(predicate));
    }
}
