using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Mud.DataStructures.Tests
{
    [TestClass]
    public class SafeListTests
    {
        [TestMethod]
        public void Ctor_Count_Test()
        {
            SafeList<int> list = new SafeList<int>();

            Assert.ContainsSingle(list);
        }

        [TestMethod]
        public void Add_Count_Test()
        {
            SafeList<int> list = new SafeList<int>();

            list.Add(5);

            Assert.ContainsSingle(list);
        }

        [TestMethod]
        public void AddMultiple_Count_Test()
        {
            SafeList<int> list = new SafeList<int>();

            list.Add(5);
            list.Add(3);

            Assert.HasCount(2, list);
        }

        [TestMethod]
        public void RemoveExisting_Count_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                5,
                3
            };

            list.Remove(5);

            Assert.ContainsSingle(list);
        }

        [TestMethod]
        public void RemoveInexistant_Count_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                5,
                3
            };

            list.Remove(8);

            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void RemoveAll_ReverseOrder_Count_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                5,
                3,
                7
            };

            list.Remove(7);
            list.Remove(3);
            list.Remove(5);

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void RemoveAll_SameOrder_Count_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                5,
                3
            };

            list.Remove(5);
            list.Remove(3);

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void Clear_Count_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                5,
                3
            };

            list.Clear();

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void Remove_EmptyList_Test()
        {
            SafeList<int> list = new SafeList<int>();

            list.Remove(5);

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void Clear_EmptyList_Test()
        {
            SafeList<int> list = new SafeList<int>();

            list.Clear();

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void Enumeration_EmptyList_Test()
        {
            SafeList<int> list = new SafeList<int>();

            int count = 0;
            foreach (int i in list)
            {
                count++;
            }

            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void Enumeration_Count_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                1,
                2,
                3,
                4
            };

            int count = 0;
            foreach (int i in list)
            {
                count++;
            }

            Assert.HasCount(count, list);
        }

        [TestMethod]
        public void Enumeration_Content_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                1,
                2,
                3,
                4
            }; // head insertion by default -> reverse order

            List<int> copy = new List<int>();
            foreach (int i in list)
            {
                copy.Add(i);
            }

            Assert.HasCount(list.Count, copy);
            Assert.AreEqual(4, copy[0]);
            Assert.AreEqual(3, copy[1]);
            Assert.AreEqual(2, copy[2]);
            Assert.AreEqual(1, copy[3]);
        }

        [TestMethod]
        public void Enumeration_Remove_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                1,
                2,
                3,
                4
            }; // head insertion by default -> reverse order

            List<int> copy = new List<int>();
            foreach (int i in list)
            {
                list.Remove(i);
                copy.Add(i);
            }

            Assert.IsEmpty(list);
            Assert.HasCount(4, copy);
            Assert.AreEqual(4, copy[0]);
            Assert.AreEqual(3, copy[1]);
            Assert.AreEqual(2, copy[2]);
            Assert.AreEqual(1, copy[3]);
        }

        [TestMethod]
        public void Enumeration_Clear_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                1,
                2,
                3,
                4
            }; // head insertion by default -> reverse order

            List<int> copy = new List<int>();
            foreach (int i in list)
            {
                list.Clear();
                copy.Add(i);
            }

            Assert.IsEmpty(list);
            Assert.HasCount(2, copy); // head and head.next will be available when list is clear while iterating
            Assert.AreEqual(4, copy[0]);
            Assert.AreEqual(3, copy[1]);
        }

        [TestMethod]
        public void Enumeration_Add_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                1,
                2,
                3,
                4
            }; // head insertion by default -> reverse order

            int add = 5;
            List<int> copy = new List<int>();
            foreach (int i in list)
            {
                list.Add(add);
                add++;
                copy.Add(i);
            }

            Assert.HasCount(8, list);
            Assert.HasCount(4, list.Where(x => x >= 5));
            Assert.AreEqual(4, copy[0]);
            Assert.AreEqual(3, copy[1]);
            Assert.AreEqual(2, copy[2]);
            Assert.AreEqual(1, copy[3]);
        }

        [TestMethod]
        public void Enumeration_Remove_OrderBy_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                1,
                2,
                3,
                4
            }; // head insertion by default -> reverse order

            List<int> copy = new List<int>();
            foreach (int i in list.OrderBy(x => x))
            {
                list.Remove(i);
                copy.Add(i);
            }

            Assert.IsEmpty(list);
            Assert.HasCount(4, copy);
            Assert.AreEqual(1, copy[0]);
            Assert.AreEqual(2, copy[1]);
            Assert.AreEqual(3, copy[2]);
            Assert.AreEqual(4, copy[3]);
        }

        [TestMethod]
        public void Enumeration_AddAndRemove_Test()
        {
            SafeList<int> list = new SafeList<int>
            {
                1,
                2,
                3,
                4
            }; // head insertion by default -> reverse order

            int add = 5;
            List<int> copy = new List<int>();
            foreach (int i in list)
            {
                copy.Add(i);
                list.Add(add);
                list.Remove(add);
                add++;
            }

            Assert.HasCount(4, list);
            Assert.IsEmpty(list.Where(x => x >= 5)); // nothing added
            Assert.AreEqual(4, copy[0]);
            Assert.AreEqual(3, copy[1]);
            Assert.AreEqual(2, copy[2]);
            Assert.AreEqual(1, copy[3]);
        }
    }
}
