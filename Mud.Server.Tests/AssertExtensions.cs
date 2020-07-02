using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.DataStructures.Flags;

namespace Mud.Server.Tests
{
    public static class AssertExtensions
    {
        public static void Contains(this Assert @this, string expectedContains, string actual)
        {
            Assert.IsTrue(actual.Contains(expectedContains), "{0} was expected to contain {1}", actual, expectedContains);
        }

        public static void AreEqual<TFlagValues>(this Assert @this, IFlags<string, TFlagValues> expected, IFlags<string, TFlagValues> actual)
            where TFlagValues : IFlagValues<string>
        {
            Assert.IsTrue(expected.Items.ToHashSet().SetEquals(actual.Items));
        }
    }
}
