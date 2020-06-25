using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mud.DataStructures.Tests
{
    [TestClass]
    public class FlagsTests
    {
        [TestMethod]
        public void Ctor()
        {
            Flags.Flags flags = new Flags.Flags();

            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Ctor_MultipleValues()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            Assert.AreEqual(3, flags.Count);
            Assert.IsTrue(flags.IsSet("flag1"));
            Assert.IsTrue(flags.IsSet("flag2"));
            Assert.IsTrue(flags.IsSet("flag3"));
        }

        #region Set

        [TestMethod]
        public void Set_OneFlag_Count()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void Set_OneFlag_IsSet()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");

            Assert.IsTrue(flags.IsSet("flag1"));
        }

        [TestMethod]
        public void Set_OneFlag_IsSetMixedCase()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");

            Assert.IsTrue(flags.IsSet("FlAg1"));
        }

        [TestMethod]
        public void Set_OneFlag_CheckAnotherFlag()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");

            Assert.IsFalse(flags.IsSet("flag2"));
        }

        [TestMethod]
        public void Set_TwoFlags_Count()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");
            flags.Set("flag2");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void Set_TwoFlags_Set()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");
            flags.Set("flag2");

            Assert.IsTrue(flags.IsSet("flag1"));
            Assert.IsTrue(flags.IsSet("flag2"));
        }

        [TestMethod]
        public void Set_MultipleMixedCaseFlags_Set()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");
            flags.Set("fLag1");
            flags.Set("FLag1");
            flags.Set("FLAG1");

            Assert.IsTrue(flags.IsSet("flag1"));
        }

        [TestMethod]
        public void Set_MultipleMixedCaseFlags_Count()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1");
            flags.Set("fLag1");
            flags.Set("FLag1");
            flags.Set("FLAG1");

            Assert.AreEqual(1, flags.Count);
        }

        #endregion

        #region Set params

        [TestMethod]
        public void SetParams_Multiple_Count()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1", "flag2");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void SetParams_Multiple_IsSet()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1", "flag2");

            Assert.IsTrue(flags.IsSet("flag1"));
            Assert.IsTrue(flags.IsSet("flag2"));
        }

        [TestMethod]
        public void SetParams_MultipleMixedCase_Count()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1", "FlAG1");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void SetParams_MultipleMixedCase_IsSet()
        {
            Flags.Flags flags = new Flags.Flags();

            flags.Set("flag1", "flag2");

            Assert.IsTrue(flags.IsSet("flag1"));
        }

        #endregion

        #region Set flags

        [TestMethod]
        public void SetFlags_DifferentFlags_Count()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag4", "flag5", "flag6");

            flags.Set(flags2);

            Assert.AreEqual(6, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        [TestMethod]
        public void SetFlags_IdenticalFlags_Count()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag1", "flag2", "flag3");

            flags.Set(flags2);

            Assert.AreEqual(3, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        [TestMethod]
        public void SetFlags_IdenticalAndDifferentFlags_Count()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag3", "flag4", "flag5");

            flags.Set(flags2);

            Assert.AreEqual(5, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        #endregion

        #region Unset

        [TestMethod]
        public void Unset_ExistingFlag_Count()
        {
            Flags.Flags flags = new Flags.Flags("flag1");

            flags.Unset("flag1");

            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Unset_ExistingFlag_IsSet()
        {
            Flags.Flags flags = new Flags.Flags("flag1");

            flags.Unset("flag1");

            Assert.IsFalse(flags.IsSet("flag1"));
        }

        [TestMethod]
        public void Unset_InexistingFlag_Count()
        {
            Flags.Flags flags = new Flags.Flags("flag1");

            flags.Unset("flag2");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void Unset_InexistingFlag_IsSet()
        {
            Flags.Flags flags = new Flags.Flags("flag1");

            flags.Unset("flag2");

            Assert.IsTrue(flags.IsSet("flag1"));
            Assert.IsFalse(flags.IsSet("flag2"));
        }

        #endregion

        #region Unset params

        [TestMethod]
        public void UnsetParams_MultipleExisting_Count()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            flags.Unset("flag1", "flag3");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void UnsetParams_MultipleExisting_IsSet()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            flags.Unset("flag1", "flag3");

            Assert.IsFalse(flags.IsSet("flag1"));
            Assert.IsTrue(flags.IsSet("flag2"));
            Assert.IsFalse(flags.IsSet("flag3"));
        }

        [TestMethod]
        public void UnsetParams_MultipleExistingAndInexisting_Count()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            flags.Unset("flag1", "flag4");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void UnsetParams_MultipleExistingAndInexisting_IsSet()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            flags.Unset("flag1", "flag4");

            Assert.IsFalse(flags.IsSet("flag1"));
            Assert.IsTrue(flags.IsSet("flag2"));
            Assert.IsTrue(flags.IsSet("flag3"));
        }

        #endregion

        #region HasAny

        [TestMethod]
        public void HasAny_Existing()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            bool hasAny = flags.HasAny("flag1", "flag3");

            Assert.IsTrue(hasAny);
        }

        [TestMethod]
        public void HasAny_Inexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            bool hasAny = flags.HasAny("flag5", "flag4");

            Assert.IsFalse(hasAny);
        }

        [TestMethod]
        public void HasAny_MultipleExistingAndInexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            bool hasAny = flags.HasAny("flag1", "flag4");

            Assert.IsTrue(hasAny);
        }

        #endregion

        #region HasAny Flag

        [TestMethod]
        public void HasAnyFlag_Existing()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag1", "flag3");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        [TestMethod]
        public void HasAnyFlag_Inexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag5", "flag4");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsFalse(hasAny);
        }

        [TestMethod]
        public void HasAnyFlag_MultipleExistingAndInexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag1", "flag4");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        #endregion

        #region HasAll

        [TestMethod]
        public void HasAll_Existing()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            bool hasAll = flags.HasAll("flag1", "flag3");

            Assert.IsTrue(hasAll);
        }

        [TestMethod]
        public void HasAll_Inexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            bool hasAll = flags.HasAll("flag5", "flag4");

            Assert.IsFalse(hasAll);
        }

        [TestMethod]
        public void HasAll_MultipleExistingAndInexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            bool hasAll = flags.HasAll("flag1", "flag4");

            Assert.IsFalse(hasAll);
        }

        #endregion

        #region HasAll Flag

        [TestMethod]
        public void HasAllFlag_Existing()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag1", "flag3");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsTrue(hasAll);
        }

        [TestMethod]
        public void HasAllFlag_Inexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag5", "flag4");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }

        [TestMethod]
        public void HasAllFlag_MultipleExistingAndInexisting()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");
            Flags.Flags flags2 = new Flags.Flags("flag1", "flag4");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }

        #endregion

        #region Items

        [TestMethod]
        public void Items_NoFlag()
        {
            Flags.Flags flags = new Flags.Flags();

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
        }

        [TestMethod]
        public void Items_OneFlag()
        {
            Flags.Flags flags = new Flags.Flags("flag1");

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
            Assert.AreEqual("flag1", items.First());
        }

        [TestMethod]
        public void Items_MultipleFlag()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3", "flag4");

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
            Assert.AreEqual(1, items.Count(x => x == "flag1"));
            Assert.AreEqual(1, items.Count(x => x == "flag2"));
            Assert.AreEqual(1, items.Count(x => x == "flag3"));
            Assert.AreEqual(1, items.Count(x => x == "flag4"));
        }

        #endregion

        #region TryParse

        [TestMethod]
        public void TryParse_Null()
        {
            bool parsed = Flags.Flags.TryParse(null, out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void TryParse_Empty()
        {
            bool parsed = Flags.Flags.TryParse(string.Empty, out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void TryParse_OneValue()
        {
            bool parsed = Flags.Flags.TryParse("flag1", out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(1, flags.Count);
            Assert.IsTrue(flags.IsSet("flag1"));
        }

        [TestMethod]
        public void TryParse_MultipleValues()
        {
            bool parsed = Flags.Flags.TryParse("flag1,flag2,flag3", out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(3, flags.Count);
            Assert.IsTrue(flags.IsSet("flag1"));
            Assert.IsTrue(flags.IsSet("flag2"));
            Assert.IsTrue(flags.IsSet("flag3"));
        }

        #endregion

        #region Parse

        [TestMethod]
        public void Parse_Null()
        {
            Flags.Flags flags = Flags.Flags.Parse(null);

            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Parse_Empty()
        {
            Flags.Flags flags = Flags.Flags.Parse(string.Empty);

            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Parse_OneValue()
        {
            Flags.Flags flags = Flags.Flags.Parse("flag1");

            Assert.IsNotNull(flags);
            Assert.AreEqual(1, flags.Count);
            Assert.IsTrue(flags.IsSet("flag1"));
        }

        [TestMethod]
        public void Parse_MultipleValues()
        {
            Flags.Flags flags = Flags.Flags.Parse("flag1,flag2,flag3");

            Assert.IsNotNull(flags);
            Assert.AreEqual(3, flags.Count);
            Assert.IsTrue(flags.IsSet("flag1"));
            Assert.IsTrue(flags.IsSet("flag2"));
            Assert.IsTrue(flags.IsSet("flag3"));
        }

        #endregion

        #region ToString

        [TestMethod]
        public void ToString_NoValue()
        {
            Flags.Flags flags = new Flags.Flags();

            Assert.AreEqual(string.Empty, flags.ToString());
        }


        [TestMethod]
        public void ToString_MultipleValues()
        {
            Flags.Flags flags = new Flags.Flags("flag1", "flag2", "flag3");

            Assert.AreEqual("flag1,flag2,flag3", flags.ToString());
        }

        #endregion
    }
}
