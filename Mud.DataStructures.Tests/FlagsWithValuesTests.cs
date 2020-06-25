using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;
using Mud.DataStructures.Flags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.DataStructures.Tests
{
    [TestClass]
    public class FlagsWithValuesTests
    {
        [TestMethod]
        public void Ctor()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Ctor_MultipleValues_Valid()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Berserk");

            Assert.AreEqual(3, flags.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_MultipleValues_Invalid()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Invalid", "Berserk");

            Assert.AreEqual(3, flags.Count);
        }

        [TestMethod]
        public void HasAny_SameFlagTypes()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Berserk");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Curse", "Charm", "Berserk");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        // Doesn't compile
        //[TestMethod]
        //public void HasAny_DifferentFlagTypes()
        //{
        //    Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Berserk");
        //    Flags<IRoomFlags> flags2 = new Flags<IRoomFlags>("Dark", "NoMob", "Berserk");

        //    bool hasAny = flags.HasAny(flags2);
        //}

        [TestMethod]
        public void HasAll_SameFlagTypes()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Berserk");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Curse", "Charm", "Berserk");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }


        #region Set

        [TestMethod]
        public void Set_OneFlag_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void Set_OneFlag_IsSet()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Set_OneFlag_IsSetMixedCase()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Set_OneFlag_CheckAnotherFlag()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind");

            Assert.IsFalse(flags.IsSet("Charm"));
        }

        [TestMethod]
        public void Set_TwoFlags_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind");
            flags.Set("Charm");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void Set_TwoFlags_Set()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind");
            flags.Set("Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
        }

        [TestMethod]
        public void Set_MultipleMixedCaseFlags_Set()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("BLind");
            flags.Set("BlInd");
            flags.Set("BliNd");
            flags.Set("BLIND");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Set_MultipleMixedCaseFlags_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("BLind");
            flags.Set("BlInd");
            flags.Set("BliNd");
            flags.Set("BLIND");

            Assert.AreEqual(1, flags.Count);
        }

        #endregion

        #region Set params

        [TestMethod]
        public void SetParams_Multiple_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind", "Charm");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void SetParams_Multiple_IsSet()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind", "Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
        }

        [TestMethod]
        public void SetParams_MultipleMixedCase_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind", "Blind");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void SetParams_MultipleMixedCase_IsSet()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            flags.Set("Blind", "Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        #endregion

        #region Set flags

        [TestMethod]
        public void SetFlags_DifferentFlags_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Berserk", "Slow", "Sneak");

            flags.Set(flags2);

            Assert.AreEqual(6, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        [TestMethod]
        public void SetFlags_IdenticalFlags_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            flags.Set(flags2);

            Assert.AreEqual(3, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        [TestMethod]
        public void SetFlags_IdenticalAndDifferentFlags_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Hide", "Berserk", "Slow");

            flags.Set(flags2);

            Assert.AreEqual(5, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        #endregion

        #region Unset

        [TestMethod]
        public void Unset_ExistingFlag_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind");

            flags.Unset("Blind");

            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Unset_ExistingFlag_IsSet()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind");

            flags.Unset("Blind");

            Assert.IsFalse(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Unset_InexistingFlag_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind");

            flags.Unset("Charm");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void Unset_InexistingFlag_IsSet()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind");

            flags.Unset("Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsFalse(flags.IsSet("Charm"));
        }

        #endregion

        #region Unset params

        [TestMethod]
        public void UnsetParams_MultipleExisting_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            flags.Unset("Blind", "Hide");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void UnsetParams_MultipleExisting_IsSet()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            flags.Unset("Blind", "Hide");

            Assert.IsFalse(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
            Assert.IsFalse(flags.IsSet("Hide"));
        }

        [TestMethod]
        public void UnsetParams_MultipleExistingAndInexisting_Count()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            flags.Unset("Blind", "Berserk");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void UnsetParams_MultipleExistingAndInexisting_IsSet()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            flags.Unset("Blind", "Berserk");

            Assert.IsFalse(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
            Assert.IsTrue(flags.IsSet("Hide"));
        }

        #endregion

        #region HasAny

        [TestMethod]
        public void HasAny_Existing()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            bool hasAny = flags.HasAny("Blind", "Hide");

            Assert.IsTrue(hasAny);
        }

        [TestMethod]
        public void HasAny_Inexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            bool hasAny = flags.HasAny("Slow", "Berserk");

            Assert.IsFalse(hasAny);
        }

        [TestMethod]
        public void HasAny_MultipleExistingAndInexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            bool hasAny = flags.HasAny("Blind", "Berserk");

            Assert.IsTrue(hasAny);
        }

        #endregion

        #region HasAny Flag

        [TestMethod]
        public void HasAnyFlag_Existing()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Blind", "Hide");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        [TestMethod]
        public void HasAnyFlag_Inexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Slow", "Berserk");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsFalse(hasAny);
        }

        [TestMethod]
        public void HasAnyFlag_MultipleExistingAndInexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Blind", "Berserk");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        #endregion

        #region HasAll

        [TestMethod]
        public void HasAll_Existing()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            bool hasAll = flags.HasAll("Blind", "Hide");

            Assert.IsTrue(hasAll);
        }

        [TestMethod]
        public void HasAll_Inexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            bool hasAll = flags.HasAll("Slow", "Berserk");

            Assert.IsFalse(hasAll);
        }

        [TestMethod]
        public void HasAll_MultipleExistingAndInexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            bool hasAll = flags.HasAll("Blind", "Berserk");

            Assert.IsFalse(hasAll);
        }

        #endregion

        #region HasAll Flag

        [TestMethod]
        public void HasAllFlag_Existing()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Blind", "Hide");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsTrue(hasAll);
        }

        [TestMethod]
        public void HasAllFlag_Inexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Slow", "Berserk");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }

        [TestMethod]
        public void HasAllFlag_MultipleExistingAndInexisting()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");
            Flags<ICharacterFlags> flags2 = new Flags<ICharacterFlags>("Blind", "Berserk");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }

        #endregion

        #region Items

        [TestMethod]
        public void Items_NoFlag()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
        }

        [TestMethod]
        public void Items_OneFlag()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind");

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
            Assert.AreEqual("Blind", items.First());
        }

        [TestMethod]
        public void Items_MultipleFlag()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide", "Berserk");

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
            Assert.AreEqual(1, items.Count(x => x == "Blind"));
            Assert.AreEqual(1, items.Count(x => x == "Charm"));
            Assert.AreEqual(1, items.Count(x => x == "Hide"));
            Assert.AreEqual(1, items.Count(x => x == "Berserk"));
        }

        #endregion

        #region TryParse

        [TestMethod]
        public void TryParse_Null()
        {
            bool parsed = Flags<ICharacterFlags>.TryParse(null, out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void TryParse_Empty()
        {
            bool parsed = Flags<ICharacterFlags>.TryParse(string.Empty, out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void TryParse_OneValue()
        {
            bool parsed = Flags<ICharacterFlags>.TryParse("Blind", out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(1, flags.Count);
            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void TryParse_MultipleValues()
        {
            bool parsed = Flags<ICharacterFlags>.TryParse("Blind,Charm,Hide", out var flags);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(flags);
            Assert.AreEqual(3, flags.Count);
            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
            Assert.IsTrue(flags.IsSet("Hide"));
        }

        #endregion

        #region Parse

        [TestMethod]
        public void Parse_Null()
        {
            Flags<ICharacterFlags> flags = Flags<ICharacterFlags>.Parse(null);

            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Parse_Empty()
        {
            Flags<ICharacterFlags> flags = Flags<ICharacterFlags>.Parse(string.Empty);

            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Parse_OneValue()
        {
            Flags<ICharacterFlags> flags = Flags<ICharacterFlags>.Parse("Blind");

            Assert.IsNotNull(flags);
            Assert.AreEqual(1, flags.Count);
            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Parse_MultipleValues()
        {
            Flags<ICharacterFlags> flags = Flags<ICharacterFlags>.Parse("Blind,Charm,Hide");

            Assert.IsNotNull(flags);
            Assert.AreEqual(3, flags.Count);
            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
            Assert.IsTrue(flags.IsSet("Hide"));
        }

        #endregion

        #region ToString

        [TestMethod]
        public void ToString_NoValue()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>();

            Assert.AreEqual(string.Empty, flags.ToString());
        }


        [TestMethod]
        public void ToString_MultipleValues()
        {
            Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Hide");

            Assert.AreEqual("Blind,Charm,Hide", flags.ToString());
        }

        #endregion


        #region TestInitialize/TestCleanup

        private SimpleInjector.Container _originalContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalContainer = DependencyContainer.Current;
            DependencyContainer.SetManualContainer(new SimpleInjector.Container());
            DependencyContainer.Current.RegisterInstance<ICharacterFlags>(new Rom24CharacterFlags()); // TODO: do this with reflection ?
            DependencyContainer.Current.RegisterInstance<IRoomFlags>(new Rom24RoomFlags()); // TODO: do this with reflection ?
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(_originalContainer);
        }

        #endregion
    }

    internal interface ICharacterFlags : IFlagValues<string>
    {
    }

    internal interface IRoomFlags : IFlagValues<string>
    {
    }

    internal class Rom24CharacterFlags : FlagValuesBase<string>, ICharacterFlags
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Blind",
            "Invisible",
            "DetectEvil",
            "DetectInvis",
            "DetectMagic",
            "DetectHidden",
            "DetectGood",
            "Sanctuary",
            "FaerieFire",
            "Infrared",
            "Curse",
            "Poison",
            "ProtectEvil",
            "ProtectGood",
            "Sneak",
            "Hide",
            "Sleep",
            "Charm",
            "Flying",
            "PassDoor",
            "Haste",
            "Calm",
            "Plague",
            "Weaken",
            "DarkVision",
            "Berserk",
            "Swim",
            "Regeneration",
            "Slow",
            "Test", // TEST PURPOSE
        };

        protected override HashSet<string> HashSet => Flags;
    }

    internal class Rom24RoomFlags : FlagValuesBase<string>, IRoomFlags
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Dark",
            "NoMob",
            "Indoors",
            "NoScan",
            "Private",
            "Safe",
            "Solitary",
            "NoRecall",
            "ImpOnly",
            "GodsOnly",
            "NewbiesOnly",
            "Law",
            "NoWhere",
            "Test", // TEST PURPOSE
        };

        protected override HashSet<string> HashSet => Flags;
    }
}
