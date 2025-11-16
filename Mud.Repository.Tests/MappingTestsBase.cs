using AutoBogus;
using AutoMapper;
using Moq;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Repository.Tests
{
    [TestClass]
    public abstract class MappingTestsBase
    {
        protected IServiceProvider _serviceProvider = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlagValues))) // don't mock IServiceProvider.GetRequiredService because it's an extension method
                .Returns(new Rom24CharacterFlags());
            serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlagValues)))
                .Returns(new Rom24CharacterFlags());
            serviceProviderMock.Setup(x => x.GetService(typeof(IRoomFlagValues)))
                .Returns(new Rom24RoomFlags());
            serviceProviderMock.Setup(x => x.GetService(typeof(IItemFlagValues)))
                .Returns(new Rom24ItemFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IWeaponFlagValues)))
                .Returns(new Rom24WeaponFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IIRVFlagValues)))
                .Returns(new Rom24IRVFlagValues());
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile<Filesystem.AutoMapperProfile>();
            });
            mapperConfiguration.AssertConfigurationIsValid();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMapper))) 
                .Returns(mapperConfiguration.CreateMapper());

            AutoFaker.Configure(builder =>
            {
                builder
                  //.WithBinder(new AutoBogus.Moq.MoqBinder())
                  .WithRepeatCount(5)    // Configures the number of items in a collection
                  .WithRecursiveDepth(10); // Configures how deep nested types should recurse
            });

            _serviceProvider = serviceProviderMock.Object;
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
