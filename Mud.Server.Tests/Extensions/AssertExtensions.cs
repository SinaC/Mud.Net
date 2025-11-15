using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.DataStructures.Flags;
using System.Linq;

namespace Mud.Server.Tests.Extensions
{
    public static class AssertExtensions
    {
        public static void AreEqual<TFlagValues>(this Assert @this, IFlags<string, TFlagValues> expected, IFlags<string, TFlagValues> actual)
            where TFlagValues : IFlagValues<string>
        {
            Assert.IsTrue(expected.Items.ToHashSet().SetEquals(actual.Items));
        }
    }
}
