using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Mud.DataStructures.Tests
{
    [TestClass]
    public class ModifiableListTests
    {
        [TestMethod]
        public void Ctor_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            Assert.HasCount(2, list);
        }

        [TestMethod]
        public void Add_NoDuplicate_NoIteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int> {10, 12};


            Assert.HasCount(2, list);
        }

        [TestMethod]
        public void Add_Duplicate_NoIteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int> {10, 12, 10};


            Assert.HasCount(3, list);
        }

        [TestMethod]
        public void Clear_NoIteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            list.Clear();

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void Remove_Existing_NoIteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            list.Remove(10);

            Assert.ContainsSingle(list);
        }

        [TestMethod]
        public void Remove_NonExisting_NoIteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            list.Remove(20);

            Assert.HasCount(2, list);
        }

        [TestMethod]
        public void Add_NoDuplicate_Iteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            foreach (int i in list)
            {
                if (i == 10)
                    list.Add(20);
            }

            Assert.HasCount(3, list);
        }

        [TestMethod]
        public void Add_Duplicate_Iteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            foreach (int i in list)
            {
                if (i == 10)
                    list.Add(10); // this is mean
            }

            Assert.HasCount(3, list);
        }

        [TestMethod]
        public void Remove_Existing_Iteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            foreach (int i in list)
            {
                list.Remove(12);
            }

            Assert.ContainsSingle(list);
        }

        [TestMethod]
        public void Remove_NonExisting_Iteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            foreach (int i in list)
            {
                list.Remove(20);
            }

            Assert.HasCount(2, list);
        }

        [TestMethod]
        public void Clear_Iteration_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12
            };

            foreach(int i in list)
                list.Clear();

            Assert.IsEmpty(list);
        }

        [TestMethod]
        public void Complex_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12,
                14,
                16,
                18
            };

            foreach (int i in list)
            {
                if (i == 10) // 1)
                    list.Add(20);
                if (i == 12) // 2)
                    list.Remove(20);
                if (i == 14) // 3)
                    list.Clear();
                if (i == 16) // 4)
                    list.Remove(20);
                if (i == 18) // 5)
                    list.Add(10);
                if (i == 20) // 6)
                    list.Clear(); // should never been processed because 20 will not be in list while iterating
            }

            // evolution of pending list
            // 1) +20
            // 2) +20, -20
            // 3) +20, -20, clear
            // 4) +20, -20, clear, -20
            // 5) +20, -20, clear, +10
            // 6) +20, -20, clear, +10   same as 5 because 20 will never be in list while iterating

            Assert.ContainsSingle(list);
            Assert.AreEqual(10, list.Single());
        }

        [TestMethod]
        public void Complex_ReverseOrder_Test()
        {
            ModifiableList<int> list = new ModifiableList<int>
            {
                10,
                12,
                14,
                16,
                18
            };

            foreach (int i in list.OrderByDescending(x => x))
            {
                if (i == 10) // 1)
                    list.Add(20);
                if (i == 12) // 2)
                    list.Remove(20);
                if (i == 14) // 3)
                    list.Clear();
                if (i == 16) // 4)
                    list.Remove(20);
                if (i == 18) // 5)
                    list.Add(10);
                if (i == 20) // 6)
                    list.Clear(); // should never been processed because 20 will not be in list while iterating
            }

            // evolution of pending list
            // 6) nop
            // 5) +10
            // 4) +10, -20
            // 3) +10, -20, clear
            // 2) +10, -20, clear, -20
            // 1) +10, -20, clear, -20, +20

            Assert.ContainsSingle(list);
            Assert.AreEqual(20, list.Single());
        }

        // TODO: nested iterations
    }
}
