using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Common
{
    public static class StringCompareHelpers
    {
        public static readonly Func<string, string, bool> StringEquals = (s, s1) => string.Equals(s, s1, StringComparison.InvariantCultureIgnoreCase);
        public static readonly Func<string, string, bool> StringStartsWith = (s, s1) => s.StartsWith(s1, StringComparison.InvariantCultureIgnoreCase);
        public static readonly Func<IEnumerable<string>, string, bool> StringListEquals = (enumerable, s) => enumerable.Any(x => StringEquals(x, s));
        public static readonly Func<IEnumerable<string>, string, bool> StringListStartsWith = (enumerable, s) => enumerable.Any(x => StringStartsWith(x, s));
        // every item in enumerable1 must be found in enumerable
        public static readonly Func<IEnumerable<string>, IEnumerable<string>, bool> StringListsEquals = (enumerable, enumerable1) => enumerable1.All(x => enumerable.Any(y => StringEquals(y, x)));
        public static readonly Func<IEnumerable<string>, IEnumerable<string>, bool> StringListsStartsWith = (enumerable, enumerable1) => enumerable1.All(x => enumerable.Any(y => StringStartsWith(y, x)));
    }
}
