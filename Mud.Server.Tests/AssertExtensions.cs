using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mud.Server.Tests
{
    public static class AssertExtensions
    {
        public static void Contains(this Assert @this, string expectedContains, string actual)
        {
            Assert.IsTrue(actual.Contains(expectedContains), "{0} was expected to contain {1}", actual, expectedContains);
        }
    }
}
