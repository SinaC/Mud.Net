using Mud.POC.Enumeration;

namespace Mud.POC.Tests.Enumeration
{
    [TestClass]
    public class LinkedListTests
    {
        [TestMethod]
        public void Enumeration_AddFirst()
        {
            var list = new LinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var item in list)
                {
                    if (item == 7)
                        list.AddFirst(99);
                }
            });
        }

        [TestMethod]
        public void SafeEnumeration()
        {
            var list = new LinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            Assert.AreEqual(5, list.SafeEnumeration().Count());
            Assert.Contains(1, list.SafeEnumeration());
            Assert.Contains(3, list.SafeEnumeration());
            Assert.Contains(5, list.SafeEnumeration());
            Assert.Contains(7, list.SafeEnumeration());
            Assert.Contains(9, list.SafeEnumeration());
        }

        [TestMethod]
        public void SafeEnumeration_AddFirst()
        {
            var list = new LinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            foreach (var item in list.SafeEnumeration())
            {
                if (item == 7)
                    list.AddFirst(99);
            }

            Assert.AreEqual(6, list.Count());
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);
            Assert.Contains(99, list);
        }

        [TestMethod]
        public void SafeEnumeration_Remove()
        {
            var list = new LinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            foreach (var item in list.SafeEnumeration())
            {
                list.Remove(item);
            }

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void SafeEnumeration_RemoveNext()
        {
            var list = new LinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list.SafeEnumeration())
            {
                items.Add(item);
                if (item == 3)
                    list.Remove(5); // this will stop enumeration because 5 will be removed and it's next item will be set to null
            }

            Assert.HasCount(4, list);
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);

            // should be 4: 1, 3, 7, 9 but it will be 3: 1, 3, 5
            Assert.HasCount(3, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(5, items);
        }

        [TestMethod]
        public void SafeEnumeration_Where_AddFirst()
        {
            var list = new LinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            foreach (var item in list.SafeEnumeration().Where(x => x >= 7))
            {
                if (item == 7)
                    list.AddFirst(99);
            }

            Assert.AreEqual(6, list.Count());
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);
            Assert.Contains(99, list);
        }
    }
}
