﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            ICharacterFlags flags = new CharacterFlags();

            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Ctor_MultipleValues_Valid()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Berserk");

            Assert.AreEqual(3, flags.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_MultipleValues_Invalid()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Invalid", "Berserk");

            Assert.AreEqual(3, flags.Count);
        }

        [TestMethod]
        public void HasAny_SameFlagTypes()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Berserk");
            ICharacterFlags flags2 = new CharacterFlags("Curse", "Charm", "Berserk");

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
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Berserk");
            ICharacterFlags flags2 = new CharacterFlags("Curse", "Charm", "Berserk");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }


        #region Set

        [TestMethod]
        public void Set_OneFlag_Count()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void Set_OneFlag_IsSet()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Set_OneFlag_IsSetMixedCase()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Set_OneFlag_CheckAnotherFlag()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind");

            Assert.IsFalse(flags.IsSet("Charm"));
        }

        [TestMethod]
        public void Set_TwoFlags_Count()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind");
            flags.Set("Charm");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void Set_TwoFlags_Set()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind");
            flags.Set("Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
        }

        [TestMethod]
        public void Set_MultipleMixedCaseFlags_Set()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("BLind");
            flags.Set("BlInd");
            flags.Set("BliNd");
            flags.Set("BLIND");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Set_MultipleMixedCaseFlags_Count()
        {
            ICharacterFlags flags = new CharacterFlags();

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
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind", "Charm");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void SetParams_Multiple_IsSet()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind", "Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
        }

        [TestMethod]
        public void SetParams_MultipleMixedCase_Count()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind", "Blind");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void SetParams_MultipleMixedCase_IsSet()
        {
            ICharacterFlags flags = new CharacterFlags();

            flags.Set("Blind", "Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
        }

        #endregion

        #region Set flags

        [TestMethod]
        public void SetFlags_DifferentFlags_Count()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            IFlags<string,                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        ICharacterFlagValues> flags2 = new CharacterFlags("Berserk", "Slow", "Sneak");

            flags.Set(flags2);

            Assert.AreEqual(6, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        [TestMethod]
        public void SetFlags_IdenticalFlags_Count()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Blind", "Charm", "Hide");

            flags.Set(flags2);

            Assert.AreEqual(3, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        [TestMethod]
        public void SetFlags_IdenticalAndDifferentFlags_Count()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Hide", "Berserk", "Slow");

            flags.Set(flags2);

            Assert.AreEqual(5, flags.Count);
            Assert.AreEqual(3, flags2.Count);
        }

        #endregion

        #region Unset

        [TestMethod]
        public void Unset_ExistingFlag_Count()
        {
            ICharacterFlags flags = new CharacterFlags("Blind");

            flags.Unset("Blind");

            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void Unset_ExistingFlag_IsSet()
        {
            ICharacterFlags flags = new CharacterFlags("Blind");

            flags.Unset("Blind");

            Assert.IsFalse(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void Unset_InexistingFlag_Count()
        {
            ICharacterFlags flags = new CharacterFlags("Blind");

            flags.Unset("Charm");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void Unset_InexistingFlag_IsSet()
        {
            ICharacterFlags flags = new CharacterFlags("Blind");

            flags.Unset("Charm");

            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsFalse(flags.IsSet("Charm"));
        }

        #endregion

        #region Unset params

        [TestMethod]
        public void UnsetParams_MultipleExisting_Count()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            flags.Unset("Blind", "Hide");

            Assert.AreEqual(1, flags.Count);
        }

        [TestMethod]
        public void UnsetParams_MultipleExisting_IsSet()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            flags.Unset("Blind", "Hide");

            Assert.IsFalse(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
            Assert.IsFalse(flags.IsSet("Hide"));
        }

        [TestMethod]
        public void UnsetParams_MultipleExistingAndInexisting_Count()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            flags.Unset("Blind", "Berserk");

            Assert.AreEqual(2, flags.Count);
        }

        [TestMethod]
        public void UnsetParams_MultipleExistingAndInexisting_IsSet()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

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
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            bool hasAny = flags.HasAny("Blind", "Hide");

            Assert.IsTrue(hasAny);
        }

        [TestMethod]
        public void HasAny_Inexisting()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            bool hasAny = flags.HasAny("Slow", "Berserk");

            Assert.IsFalse(hasAny);
        }

        [TestMethod]
        public void HasAny_MultipleExistingAndInexisting()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            bool hasAny = flags.HasAny("Blind", "Berserk");

            Assert.IsTrue(hasAny);
        }

        #endregion

        #region HasAny Flag

        [TestMethod]
        public void HasAnyFlag_Existing()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Blind", "Hide");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        [TestMethod]
        public void HasAnyFlag_Inexisting()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Slow", "Berserk");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsFalse(hasAny);
        }

        [TestMethod]
        public void HasAnyFlag_MultipleExistingAndInexisting()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Blind", "Berserk");

            bool hasAny = flags.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        #endregion

        #region HasAll

        [TestMethod]
        public void HasAll_Existing()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            bool hasAll = flags.HasAll("Blind", "Hide");

            Assert.IsTrue(hasAll);
        }

        [TestMethod]
        public void HasAll_Inexisting()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            bool hasAll = flags.HasAll("Slow", "Berserk");

            Assert.IsFalse(hasAll);
        }

        [TestMethod]
        public void HasAll_MultipleExistingAndInexisting()
        {
            Flags<ICharacterFlagValues> flags = new CharacterFlags("Blind", "Charm", "Hide");

            bool hasAll = flags.HasAll("Blind", "Berserk");

            Assert.IsFalse(hasAll);
        }

        #endregion

        #region HasAll Flag

        [TestMethod]
        public void HasAllFlag_Existing()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Blind", "Hide");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsTrue(hasAll);
        }

        [TestMethod]
        public void HasAllFlag_Inexisting()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Slow", "Berserk");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }

        [TestMethod]
        public void HasAllFlag_MultipleExistingAndInexisting()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");
            ICharacterFlags flags2 = new CharacterFlags("Blind", "Berserk");

            bool hasAll = flags.HasAll(flags2);

            Assert.IsFalse(hasAll);
        }

        #endregion

        #region Items

        [TestMethod]
        public void Items_NoFlag()
        {
            ICharacterFlags flags = new CharacterFlags();

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
        }

        [TestMethod]
        public void Items_OneFlag()
        {
            ICharacterFlags flags = new CharacterFlags("Blind");

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
            Assert.AreEqual("Blind", items.First());
        }

        [TestMethod]
        public void Items_MultipleFlag()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide", "Berserk");

            IEnumerable<string> items = flags.Items;

            Assert.AreEqual(flags.Count, items.Count());
            Assert.AreEqual(1, items.Count(x => x == "Blind"));
            Assert.AreEqual(1, items.Count(x => x == "Charm"));
            Assert.AreEqual(1, items.Count(x => x == "Hide"));
            Assert.AreEqual(1, items.Count(x => x == "Berserk"));
        }

        #endregion

        #region Ctor parse

        [TestMethod]
        public void CtorParse_Null()
        {
            ICharacterFlags flags = new CharacterFlags((string)null);

            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void CtorParse_Empty()
        {
            ICharacterFlags flags = new CharacterFlags(string.Empty);

            Assert.IsNotNull(flags);
            Assert.AreEqual(0, flags.Count);
        }

        [TestMethod]
        public void CtorParse_OneValue()
        {
            ICharacterFlags flags = new CharacterFlags("Blind");

            Assert.IsNotNull(flags);
            Assert.AreEqual(1, flags.Count);
            Assert.IsTrue(flags.IsSet("Blind"));
        }

        [TestMethod]
        public void CtorParse_MultipleValues()
        {
            ICharacterFlags flags = new CharacterFlags("Blind,Charm,Hide");

            Assert.IsNotNull(flags);
            Assert.AreEqual(3, flags.Count);
            Assert.IsTrue(flags.IsSet("Blind"));
            Assert.IsTrue(flags.IsSet("Charm"));
            Assert.IsTrue(flags.IsSet("Hide"));
        }

        #endregion

        #region Map

        [TestMethod]
        public void Map_NoValue()
        {
            ICharacterFlags flags = new CharacterFlags();

            Assert.AreEqual(string.Empty, flags.Map());
        }


        [TestMethod]
        public void Map_MultipleValues()
        {
            ICharacterFlags flags = new CharacterFlags("Blind", "Charm", "Hide");

            Assert.AreEqual("Blind,Charm,Hide", flags.Map());
        }

        #endregion

        #region ICharacterFlags

        [TestMethod]
        public void HasAny_ICharacterFlags()
        {
            ICharacterFlags flags1 = new CharacterFlags("Calm", "Berserk", "Blind");
            ICharacterFlags flags2 = new CharacterFlags("Sanctuary", "Blind", "Invisible");

            bool hasAny = flags1.HasAny(flags2);

            Assert.IsTrue(hasAny);
        }

        // Doesn't compile
        //[TestMethod]
        //public void HasAny_ICharacterFlags_IRoomFlags()
        //{
        //    ICharacterFlags flags1 = new CharacterFlags("Calm", "Berserk", "Test");
        //    IRoomFlags flags2 = new RoomFlags("Dark", "NoMob", "Test");

        //    bool hasAny = flags1.HasAny(flags2);

        //    Assert.IsTrue(hasAny);
        //}

        #endregion

        #region TestInitialize/TestCleanup

        private SimpleInjector.Container _originalContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalContainer = DependencyContainer.Current;
            DependencyContainer.SetManualContainer(new SimpleInjector.Container());
            DependencyContainer.Current.RegisterInstance<ICharacterFlagValues>(new Rom24CharacterFlags()); // TODO: do this with reflection ?
            DependencyContainer.Current.RegisterInstance<IRoomFlagValues>(new Rom24RoomFlags()); // TODO: do this with reflection ?
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(_originalContainer);
        }

        #endregion
    }

    internal class CharacterFlags : Flags<ICharacterFlagValues>, ICharacterFlags
    {
        public CharacterFlags()
            : base()
        {
        }

        public CharacterFlags(string flags)
            : base(flags)
        {
        }

        public CharacterFlags(params string[] flags)
            : base(flags)
        {
        }
    }

    internal class RoomFlags : Flags<IRoomFlagValues>, IRoomFlags
    {
    }

    internal interface ICharacterFlags : IFlags<string, ICharacterFlagValues>
    {
    }

    internal interface IRoomFlags : IFlags<string, IRoomFlagValues>
    {
    }

    internal interface ICharacterFlagValues : IFlagValues<string>
    {
    }

    internal interface IRoomFlagValues : IFlagValues<string>
    {
    }

    internal class Rom24CharacterFlags : FlagValuesBase<string>, ICharacterFlagValues
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

    internal class Rom24RoomFlags : FlagValuesBase<string>, IRoomFlagValues
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
