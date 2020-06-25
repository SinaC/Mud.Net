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
            DependencyContainer.Current.RegisterInstance<ICharacterFlagValues>(new Rom24CharacterFlags()); // TODO: do this with reflection ?
            DependencyContainer.Current.RegisterInstance<IRoomFlagValues>(new Rom24RoomFlags()); // TODO: do this with reflection ?

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
}
