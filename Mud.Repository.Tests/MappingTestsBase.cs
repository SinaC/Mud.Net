using AutoBogus;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Mud.DataStructures.Flags;
using Mud.Repository.Filesystem.Json.Converters;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using System.Text.Json;

namespace Mud.Repository.Tests
{
    [TestClass]
    public abstract class MappingTestsBase
    {
        protected IServiceProvider _serviceProvider = default!;
        protected IFlagFactory _flagFactory = default!;
        protected JsonSerializerOptions _options = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProvider = serviceProviderMock.Object;

            serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlagValues))) // don't mock IServiceProvider.GetRequiredService because it's an extension method
                .Returns(() => new Rom24CharacterFlags());
            serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlags)))
                .Returns(() => new CharacterFlags(_serviceProvider.GetRequiredService<ICharacterFlagValues>()));
            serviceProviderMock.Setup(x => x.GetService(typeof(IFlagFactory<ICharacterFlags, ICharacterFlagValues>)))
                .Returns(() => new CharacterFlagsFactory(_serviceProvider));

            serviceProviderMock.Setup(x => x.GetService(typeof(IRoomFlagValues)))
                .Returns(() => new Rom24RoomFlags());
            serviceProviderMock.Setup(x => x.GetService(typeof(IRoomFlags)))
                .Returns(() => new RoomFlags(_serviceProvider.GetRequiredService<IRoomFlagValues>()));
            serviceProviderMock.Setup(x => x.GetService(typeof(IFlagFactory<IRoomFlags, IRoomFlagValues>)))
                .Returns(() => new RoomFlagsFactory(_serviceProvider));

            serviceProviderMock.Setup(x => x.GetService(typeof(IItemFlagValues)))
                .Returns(() => new Rom24ItemFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IItemFlags)))
                .Returns(() => new ItemFlags(_serviceProvider.GetRequiredService<IItemFlagValues>()));
            serviceProviderMock.Setup(x => x.GetService(typeof(IFlagFactory<IItemFlags, IItemFlagValues>)))
                .Returns(() => new ItemFlagsFactory(_serviceProvider));

            serviceProviderMock.Setup(x => x.GetService(typeof(IWeaponFlagValues)))
                .Returns(() => new Rom24WeaponFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IWeaponFlags)))
                .Returns(() => new WeaponFlags(_serviceProvider.GetRequiredService<IWeaponFlagValues>()));
            serviceProviderMock.Setup(x => x.GetService(typeof(IFlagFactory<IWeaponFlags, IWeaponFlagValues>)))
                .Returns(() => new WeaponFlagsFactory(_serviceProvider));

            serviceProviderMock.Setup(x => x.GetService(typeof(IIRVFlagValues)))
                .Returns(() => new Rom24IRVFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IIRVFlags)))
                .Returns(() => new IRVFlags(_serviceProvider.GetRequiredService<IIRVFlagValues>()));
            serviceProviderMock.Setup(x => x.GetService(typeof(IFlagFactory<IIRVFlags, IIRVFlagValues>)))
                .Returns(() => new IRVFlagsFactory(_serviceProvider));

            serviceProviderMock.Setup(x => x.GetService(typeof(IShieldFlagValues)))
               .Returns(() => new Rom24ShieldFlags());
            serviceProviderMock.Setup(x => x.GetService(typeof(IShieldFlags)))
                .Returns(() => new ShieldFlags(_serviceProvider.GetRequiredService<IShieldFlagValues>()));
            serviceProviderMock.Setup(x => x.GetService(typeof(IFlagFactory<IShieldFlags, IShieldFlagValues>)))
                .Returns(() => new ShieldFlagsFactory(_serviceProvider));


            _flagFactory = new FlagsFactory(_serviceProvider);

            _options = new JsonSerializerOptions { WriteIndented = true };
            _options.Converters.Add(new CharacterFlagsJsonConverter(_flagFactory));
            _options.Converters.Add(new IRVFlagsJsonConverter(_flagFactory));
            _options.Converters.Add(new ShieldFlagsJsonConverter(_flagFactory));
            _options.Converters.Add(new ItemFlagsJsonConverter(_flagFactory));
            _options.Converters.Add(new WeaponFlagsJsonConverter(_flagFactory));
            _options.Converters.Add(new RoomFlagsJsonConverter(_flagFactory));

            AutoFaker.Configure(builder =>
            {
                builder
                  //.WithBinder(new AutoBogus.Moq.MoqBinder())
                  .WithRepeatCount(5)    // Configures the number of items in a collection
                  .WithRecursiveDepth(10); // Configures how deep nested types should recurse
            });
        }
    }

    public class Rom24CharacterFlags : FlagValuesBase<string>, ICharacterFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "Blind",
            "Invisible",
            "DetectEvil",
            "DetectInvis",
            "DetectMagic",
            "DetectHidden",
            "DetectGood",
            "FaerieFire",
            "Infrared",
            "Curse",
            "Poison",
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

        public Rom24CharacterFlags()
        {
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            // NOP
        }

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag.ToString();
    }

    public class Rom24ShieldFlags : FlagValuesBase<string>, IShieldFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "Sanctuary",
            "ProtectGood",
            "ProtectEvil"
        };

        protected override HashSet<string> HashSet => Flags;

        public Rom24ShieldFlags()
        {
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            // NOP
        }

        public string PrettyPrint(string flag, bool shortDisplay)
           => flag.ToString();
    }

    public class Rom24RoomFlags : FlagValuesBase<string>, IRoomFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
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

        public Rom24RoomFlags()
        {
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            // NOP
        }
    }

    public class Rom24ItemFlagValues : FlagValuesBase<string>, IItemFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
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

        public Rom24ItemFlagValues()
        {
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            // NOP
        }
    }

    public class Rom24WeaponFlagValues : FlagValuesBase<string>, IWeaponFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
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

        public Rom24WeaponFlagValues()
        {
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            // NOP
        }

        public string PrettyPrint(string flag, bool shortDisplay)
            => flag.ToString();
    }

    public class Rom24IRVFlagValues : FlagValuesBase<string>, IIRVFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
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

        public Rom24IRVFlagValues()
        {
        }

        public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
        {
            // NOP
        }
    }
}
