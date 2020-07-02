using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;
using Mud.Server.Tests.Mocking;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.Server.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        protected IWorld World => Container.DependencyContainer.Current.GetInstance<IWorld>();
        protected IRoomManager RoomManager => Container.DependencyContainer.Current.GetInstance<IRoomManager>();
        protected IItemManager ItemManager => Container.DependencyContainer.Current.GetInstance<IItemManager>();
        protected ICharacterManager CharacterManager => Container.DependencyContainer.Current.GetInstance<ICharacterManager>();
        protected IQuestManager QuestManager => Container.DependencyContainer.Current.GetInstance<IQuestManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            (Container.DependencyContainer.Current.GetInstance<IWorld>() as WorldMock).Clear();
            (Container.DependencyContainer.Current.GetInstance<IRoomManager>() as RoomManagerMock).Clear();
            (Container.DependencyContainer.Current.GetInstance<IItemManager>() as ItemManagerMock).Clear();
            (Container.DependencyContainer.Current.GetInstance<ICharacterManager>() as CharacterManagerMock).Clear();
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Container.DependencyContainer.Current.Options.EnableAutoVerification = false;

            Container.DependencyContainer.Current.RegisterInstance<ISettings>(new SettingsMock());
            Container.DependencyContainer.Current.RegisterInstance<ITimeManager>(new TimeManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IRaceManager>(new RaceManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IClassManager>(new ClassManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IAbilityManager>(new AbilityManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IWorld>(new WorldMock());
            Container.DependencyContainer.Current.RegisterInstance<IWiznet>(new WiznetMock());
            Container.DependencyContainer.Current.RegisterInstance<IRoomManager>(new RoomManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IAreaManager>(new AreaManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<ICharacterManager>(new CharacterManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IItemManager>(new ItemManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IQuestManager>(new QuestManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<IRandomManager>(new RandomManager());
            Container.DependencyContainer.Current.RegisterInstance<IGameActionManager>(new GameActionManager(new AssemblyHelper()));
            //Container.DependencyContainer.Current.RegisterInstance<IPlayerManager>(new PlayerManagerMock());
            //Container.DependencyContainer.Current.RegisterInstance<IAdminManager>(new AdminManagerMock());
            //Container.DependencyContainer.Current.RegisterInstance<IServerPlayerCommand>(new ServerPlayerCommandMock());
            //Container.DependencyContainer.Current.RegisterInstance<IServerAdminCommand>(new ServerAdminCommandMock());
            //Container.DependencyContainer.Current.RegisterInstance<ILoginRepository>(new LoginRepositoryMock());
            //Container.DependencyContainer.Current.RegisterInstance<IPlayerRepository>(new PlayerRepositoryMock());
            //Container.DependencyContainer.Current.RegisterInstance<IAdminRepository>(new AdminRepositoryMock());
            //Container.DependencyContainer.Current.RegisterInstance<ITableValues>(new TableValuesMock());
            Container.DependencyContainer.Current.RegisterInstance<IAuraManager>(new AuraManagerMock());
            Container.DependencyContainer.Current.RegisterInstance<ICharacterFlagValues>(new Rom24CharacterFlagValues());
            Container.DependencyContainer.Current.RegisterInstance<IRoomFlagValues>(new Rom24RoomFlagValues());
            Container.DependencyContainer.Current.RegisterInstance<IItemFlagValues>(new Rom24ItemFlagValues());
            Container.DependencyContainer.Current.RegisterInstance<IWeaponFlagValues>(new Rom24WeaponFlagValues());
            Container.DependencyContainer.Current.RegisterInstance<IIRVFlagValues>(new Rom24IRVFlagValues());
            Container.DependencyContainer.Current.RegisterInstance<IActFlagValues>(new Rom24ActFlagValues());
            Container.DependencyContainer.Current.RegisterInstance<IAssistFlagValues>(new Rom24AssistFlagValues());
            Container.DependencyContainer.Current.RegisterInstance<IBodyFormValues>(new Rom24BodyFormValues());
            Container.DependencyContainer.Current.RegisterInstance<IBodyPartValues>(new Rom24BodyPartValues());
            Container.DependencyContainer.Current.RegisterInstance<IOffensiveFlagValues>(new Rom24OffensiveFlagValues());

            IAssemblyHelper assemblyHelper = new AssemblyHelper();
            Type iRegistrable = typeof(IRegistrable);
            foreach (var registrable in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRegistrable.IsAssignableFrom(t))))
                Container.DependencyContainer.Current.Register(registrable);
        }

        internal class AssemblyHelper : IAssemblyHelper
        {
            public IEnumerable<Assembly> AllReferencedAssemblies => new[] { typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly };
        }

    }

    internal class Rom24CharacterFlagValues : FlagValuesBase<string>, ICharacterFlagValues
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

    internal class Rom24RoomFlagValues : FlagValuesBase<string>, IRoomFlagValues
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

    internal class Rom24ActFlagValues : FlagValuesBase<string>, IActFlagValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Sentinel",
            "Scavenger",
            "StayArea",
            "Aggressive",
            "Wimpy",
            "Pet",
            "Undead",
            "NoAlign",
            "NoPurge",
            "Outdoors",
            "Indoors",
            "UpdateAlways",
            "Train",
            "IsHealer",
            "Gain",
            "Practice",
            "Aware",
            "Warrior",
            "Thief",
            "Cleric",
            "Mage",
        };

        protected override HashSet<string> HashSet => Flags;
    }

    internal class Rom24AssistFlagValues : FlagValuesBase<string>, IAssistFlagValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "AreaAttack",
            "Backstab",
            "Bash",
            "Berserk",
            "Disarm",
            "Dodge",
            "Fade",
            "Fast",
            "Kick",
            "DirtKick",
            "Parry",
            "Rescue",
            "Tail",
            "Trip",
            "Crush",
            "Bite",
        };

        protected override HashSet<string> HashSet => Flags;
    }

    internal class Rom24BodyFormValues : FlagValuesBase<string>, IBodyFormValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Edible",
            "Poison",
            "Magical",
            "InstantDecay",
            "Other", // defined by material
            "Animal",
            "Sentient",
            "Undead",
            "Construct",
            "Mist",
            "Intangible",
            "Biped",
            "Centaur",
            "Insect",
            "Spider",
            "Crustacean",
            "Worm",
            "Blob",
            "Mammal",
            "Bird",
            "Reptile",
            "Snake",
            "Dragon",
            "Amphibian",
            "Fish",
            "ColdBlood",
            "Fur",
            "FourArms",
        };

        protected override HashSet<string> HashSet => Flags;
    }

    internal class Rom24BodyPartValues : FlagValuesBase<string>, IBodyPartValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "Head",
            "Arms",
            "Legs",
            "Heart",
            "Brains",
            "Guts",
            "Hands",
            "Feet",
            "Fingers",
            "Ear",
            "Eye",
            "LongTongue",
            "Eyestalks",
            "Tentacles",
            "Fins",
            "Wings",
            "Tail",
            "Body",
            "Claws",
            "Fangs",
            "Horns",
            "Scales",
            "Tusks",
        };

        protected override HashSet<string> HashSet => Flags;
    }

    internal class Rom24OffensiveFlagValues : FlagValuesBase<string>, IOffensiveFlagValues
    {
        public static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "AreaAttack",
            "Backstab",
            "Bash",
            "Berserk",
            "Disarm",
            "Dodge",
            "Fade",
            "Fast",
            "Kick",
            "DirtKick",
            "Parry",
            "Rescue",
            "Tail",
            "Trip",
            "Crush",
            "Bite",
        };

        protected override HashSet<string> HashSet => Flags;
    }
}
