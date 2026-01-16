using Mud.POC.SafeLinkedList;

namespace Mud.POC.Tests.SafeLinkedList
{
    [TestClass]
    public class SafeLinkedListTests
    {
        [TestMethod]
        public void SafeEnumeration()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            Assert.HasCount(5, list);
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);
        }

        [TestMethod]
        public void Add()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            foreach (var item in list)
            {
                if (item == 7)
                    list.Add(99);
            }

            Assert.HasCount(6, list);
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);
            Assert.Contains(99, list);
        }

        [TestMethod]
        public void RemoveCurrent()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                list.Remove(item);
            }

            Assert.IsEmpty(list);

            Assert.HasCount(5, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(5, items);
            Assert.Contains(7, items);
            Assert.Contains(9, items);
        }

        [TestMethod]
        public void RemoveNext()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 3)
                    list.Remove(5);
            }

            Assert.HasCount(4, list);
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);

            Assert.HasCount(4, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(7, items);
            Assert.Contains(9, items);
        }

        [TestMethod]
        public void RemovePrevious()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 5)
                    list.Remove(3);
            }

            Assert.HasCount(4, list);
            Assert.Contains(1, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);

            Assert.HasCount(5, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(5, items);
            Assert.Contains(7, items);
            Assert.Contains(9, items);
        }

        [TestMethod]
        public void AddWhenIterationIsOnBeforeLast()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 7)
                    list.Add(99);
            }

            Assert.HasCount(6, list);
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);
            Assert.Contains(99, list);

            Assert.HasCount(6, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(5, items);
            Assert.Contains(7, items);
            Assert.Contains(9, items);
            Assert.Contains(99, items);
        }

        [TestMethod]
        public void AddWhenIterationIsOnLast()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 9)
                    list.Add(99);
            }

            Assert.HasCount(6, list);
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);
            Assert.Contains(99, list);

            Assert.HasCount(6, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(5, items);
            Assert.Contains(7, items);
            Assert.Contains(9, items);
            Assert.Contains(99, items);
        }

        [TestMethod]
        public void Clear_DuringEnumeration()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 3)
                    list.Clear();
            }

            Assert.IsEmpty(list);

            Assert.HasCount(2, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
        }

        [TestMethod]
        public void Clear_DuringEnumerationBeforeLast()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 7)
                    list.Clear();
            }

            Assert.IsEmpty(list);

            Assert.HasCount(4, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(5, items);
            Assert.Contains(7, items);
        }

        [TestMethod]
        public void Clear_DuringEnumerationLast()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 9)
                    list.Clear();
            }

            Assert.IsEmpty(list);

            Assert.HasCount(5, items);
            Assert.Contains(1, items);
            Assert.Contains(3, items);
            Assert.Contains(5, items);
            Assert.Contains(7, items);
            Assert.Contains(9, items);
        }

        [TestMethod]
        public void Add_DuringEnumerationOneElementList()
        {
            var list = new SafeLinkedList<int> { 1 };

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                if (item == 1)
                    list.Add(99);
            }

            Assert.HasCount(2, list);
            Assert.Contains(1, list);
            Assert.Contains(99, list);

            Assert.HasCount(2, items);
            Assert.Contains(1, items);
            Assert.Contains(99, items);
        }

        [TestMethod]
        public void Add_DuringEnumerationEmptyList()
        {
            var list = new SafeLinkedList<int>();

            var items = new List<int>();
            foreach (var item in list)
            {
                items.Add(item);
                list.Add(99);
            }

            Assert.IsEmpty(list);
            Assert.IsEmpty(items);
        }

        [TestMethod]
        public void Where_Add()
        {
            var list = new SafeLinkedList<int>(Enumerable.Range(0, 5).Select(x => 1 + 2 * x));

            foreach (var item in list.Where(x => x >= 7))
            {
                if (item == 7)
                    list.Add(99);
            }

            Assert.HasCount(6, list);
            Assert.Contains(1, list);
            Assert.Contains(3, list);
            Assert.Contains(5, list);
            Assert.Contains(7, list);
            Assert.Contains(9, list);
            Assert.Contains(99, list);
        }
    }
}
