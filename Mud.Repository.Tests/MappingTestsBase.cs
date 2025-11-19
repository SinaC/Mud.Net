using AutoBogus;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using static Mud.Repository.Filesystem.AutoMapperProfile;

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
                .Returns(new Rom24CharacterFlags(new Mock<ILogger<Rom24CharacterFlags>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IRoomFlagValues)))
                .Returns(new Rom24RoomFlags(new Mock<ILogger<Rom24RoomFlags>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IItemFlagValues)))
                .Returns(new Rom24ItemFlagValues(new Mock<ILogger<Rom24ItemFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IWeaponFlagValues)))
                .Returns(new Rom24WeaponFlagValues(new Mock<ILogger<Rom24WeaponFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IIRVFlagValues)))
                .Returns(new Rom24IRVFlagValues(new Mock<ILogger<Rom24IRVFlagValues>>().Object));
            var mapper = CreateMapper();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMapper))) 
                .Returns(mapper);

            AutoFaker.Configure(builder =>
            {
                builder
                  //.WithBinder(new AutoBogus.Moq.MoqBinder())
                  .WithRepeatCount(5)    // Configures the number of items in a collection
                  .WithRecursiveDepth(10); // Configures how deep nested types should recurse
            });

            _serviceProvider = serviceProviderMock.Object;
        }

        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile<Filesystem.AutoMapperProfile>(); 
                cfg.ConstructServicesUsing(t =>
                {
                    if (t == typeof(StringToItemFlagsConverter))
                        return new StringToItemFlagsConverter(_serviceProvider);
                    if (t == typeof(StringToIRVFlagsConverter))
                        return new StringToIRVFlagsConverter(_serviceProvider);
                    if (t == typeof(StringToWeaponFlagsConverter))
                        return new StringToWeaponFlagsConverter(_serviceProvider);
                    if (t == typeof(StringToCharacterFlagsConverter))
                        return new StringToCharacterFlagsConverter(_serviceProvider);
                    if (t == typeof(StringToRoomFlagsConverter))
                        return new StringToRoomFlagsConverter(_serviceProvider);
                    return null;
                });
            });

            config.AssertConfigurationIsValid();

            var mapper = new Mapper(config);
            return mapper;
        }

    }

    public class Rom24CharacterFlags : FlagValuesBase<string>, ICharacterFlagValues
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

        public Rom24CharacterFlags(ILogger<Rom24CharacterFlags> logger)
        : base(logger)
        {

        }
    }

    public class Rom24RoomFlags : FlagValuesBase<string>, IRoomFlagValues
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

        public Rom24RoomFlags(ILogger<Rom24RoomFlags> logger)
        : base(logger)
        {

        }
    }

    public class Rom24ItemFlagValues : FlagValuesBase<string>, IItemFlagValues
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

        public Rom24ItemFlagValues(ILogger<Rom24ItemFlagValues> logger)
        : base(logger)
        {

        }
    }

    public class Rom24WeaponFlagValues : FlagValuesBase<string>, IWeaponFlagValues
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

        public Rom24WeaponFlagValues(ILogger<Rom24WeaponFlagValues> logger)
        : base(logger)
        {

        }
    }

    public class Rom24IRVFlagValues : FlagValuesBase<string>, IIRVFlagValues
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

        public Rom24IRVFlagValues(ILogger<Rom24IRVFlagValues> logger)
        : base(logger)
        {

        }
    }
}
