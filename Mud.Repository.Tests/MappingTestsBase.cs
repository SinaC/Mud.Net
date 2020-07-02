using AutoBogus;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Repository.Tests
{
    [TestClass]
    public abstract class MappingTestsBase
    {
        private SimpleInjector.Container _originalContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalContainer = DependencyContainer.Current;
            DependencyContainer.SetManualContainer(new SimpleInjector.Container());

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile<Mongo.AutoMapperProfile>();
                cfg.AddProfile<Filesystem.AutoMapperProfile>();
            });
            DependencyContainer.Current.RegisterInstance(mapperConfiguration.CreateMapper());
            DependencyContainer.Current.RegisterInstance<ICharacterFlagValues>(new Rom24CharacterFlags());
            DependencyContainer.Current.RegisterInstance<IRoomFlagValues>(new Rom24RoomFlags());
            DependencyContainer.Current.RegisterInstance<IItemFlagValues>(new Rom24ItemFlagValues());
            DependencyContainer.Current.RegisterInstance<IWeaponFlagValues>(new Rom24WeaponFlagValues());
            DependencyContainer.Current.RegisterInstance<IIRVFlagValues>(new Rom24IRVFlagValues());

            AutoFaker.Configure(builder =>
            {
                builder
                  //.WithBinder(new AutoBogus.Moq.MoqBinder())
                  .WithRepeatCount(5)    // Configures the number of items in a collection
                  .WithRecursiveDepth(10); // Configures how deep nested types should recurse
            });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(_originalContainer);
        }
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

    internal class Rom24ItemFlagValues : FlagValuesBase<string>, IItemFlagValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "None",
            "Glowing",
            "Humming",
            "Dark",
            "Lock",
            "Evil",
            "Invis",
            "Magic",
            "NoDrop", // Cannot be dropped once in inventory (cannot be put in container) [can be uncursed]
            "Bless",
            "AntiGood",
            "AntiEvil",
            "AntiNeutral",
            "NoRemove", // Cannot be removed once equipped [can be uncursed]
            "Inventory",
            "NoPurge",
            "RotDeath", // Disappear when holder dies
            "VisibleDeath", // Visible when holder dies
            "NonMetal",
            "NoLocate",
            "MeltOnDrop", // Melt when dropped
            "HadTimer",
            "SellExtract",
            "BurnProof",
            "NoUncurse",
            "NoSacrifice",
        };

        protected override HashSet<string> HashSet => Flags;
    }

    internal class Rom24WeaponFlagValues : FlagValuesBase<string>, IWeaponFlagValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Flaming",
            "Frost",
            "Vampiric",
            "Sharp",
            "Vorpal",
            "TwoHands",
            "Shocking",
            "Poison",
            "Holy",
        };

        protected override HashSet<string> HashSet => Flags;
    }

    internal class Rom24IRVFlagValues : FlagValuesBase<string>, IIRVFlagValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Summon",
            "Charm",
            "Magic",
            "Weapon",
            "Bash",
            "Pierce",
            "Slash",
            "Fire",
            "Cold",
            "Lightning",
            "Acid",
            "Poison",
            "Negative",
            "Holy",
            "Energy",
            "Mental",
            "Disease",
            "Drowning",
            "Light",
            "Sound",
            "Wood",
            "Silver",
            "Iron",
        };

        protected override HashSet<string> HashSet => Flags;
    }
}
