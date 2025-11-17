using Microsoft.Extensions.DependencyInjection;
using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Importer.Mystery;
using Mud.Logger;
using Mud.Network.Interfaces;
using Mud.Network.Telnet;
using Mud.POC;
using Mud.Repository;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Interfaces.World;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;
using Mud.Settings.Interfaces;
using System.Reflection;
using System.Text;

namespace Mud.Server.TestApplication;

internal class Program
{
    private IServiceProvider _serviceProvider = null!;

    private static void Main(string[] args)
    {
        var program = new Program();
        program.Run();
    }

    private void Run()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        //TestSecondWindow();
        //TestPaging();
        //TestCommandParsing();
        //TestBasicCommands();
        //TestWorldOnline();
        TestWorldOffline();

        //TestLuaIntegration testLua = new TestLuaIntegration();
        //TestLuaBasicFunctionality testLua = new TestLuaBasicFunctionality();
        //TestLuaFunctionHiding testLua = new TestLuaFunctionHiding();
        //TestLuaRegisterFunction testLua = new TestLuaRegisterFunction();
        //testLua.Test();

    }

    //private static void TestSecondWindow()
    //{
    //    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
    //    {
    //        RedirectStandardError = true,
    //        RedirectStandardInput = true,
    //        RedirectStandardOutput = true,
    //        UseShellExecute = false,
    //        CreateNoWindow = true,
    //        WindowStyle = ProcessWindowStyle.Normal
    //    };

    //    Process p = Process.Start(psi);

    //    StreamWriter sw = p.StandardInput;
    //    StreamReader sr = p.StandardOutput;

    //    sw.WriteLine("Hello world!");
    //    sr.Close();
    //}


    private void ConfigureServices(IServiceCollection services)
    {
        var settings = new Settings.ConfigurationManager.Settings();
        var assemblyHelper = new AssemblyHelper();

        // Initialize log
        Log.Default.Initialize(settings.LogPath, "server.test.log");

        // Configure Logging
        services.AddLogging();

        // Register Services
        services.AddSingleton<ISettings>(settings);
        RegisterAllTypes(services, assemblyHelper.AllReferencedAssemblies);

        RegisterFlagValues<ICharacterFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IRoomFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IItemFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IWeaponFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IActFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IOffensiveFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IAssistFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IIRVFlagValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IBodyFormValues>(services, assemblyHelper.AllReferencedAssemblies);
        RegisterFlagValues<IBodyPartValues>(services, assemblyHelper.AllReferencedAssemblies);

        services.AddSingleton<IRandomManager>(new RandomManager()); // 2 ctors => injector can't decide which one to choose
        services.AddSingleton<IAssemblyHelper>(assemblyHelper);
        services.AddSingleton<IAbilityManager, Ability.AbilityManager>();
        services.AddSingleton<IGameActionManager, GameAction.GameActionManager>();
        services.AddSingleton<ITimeManager, Server.TimeManager>();
        services.AddSingleton<IWorld, World.World>();
        services.AddSingleton<IQuestManager, Quest.QuestManager>();
        services.AddSingleton<IAuraManager, Aura.AuraManager>();
        services.AddSingleton<IItemManager, Item.ItemManager>();
        services.AddSingleton<ICharacterManager, Character.CharacterManager>();
        services.AddSingleton<IRoomManager, Room.RoomManager>();
        services.AddSingleton<IAreaManager, Area.AreaManager>();
        services.AddSingleton<IServer, Server.Server>();
        services.AddSingleton<IWiznet, Server.Server>(); // Server also implements IWiznet
        services.AddSingleton<IPlayerManager, Server.Server>(); // Server also implements IPlayerManager
        services.AddSingleton<IAdminManager, Server.Server>(); // Server also implements IAdminManager
        services.AddSingleton<IServerAdminCommand, Server.Server>(); // Server also implements IServerAdminCommand
        services.AddSingleton<IServerPlayerCommand, Server.Server>(); // Server also implements IServerPlayerCommand
        services.AddSingleton<IClassManager, Class.ClassManager>();
        services.AddSingleton<IRaceManager, Race.RaceManager>();
        services.AddSingleton<IUniquenessManager, Server.UniquenessManager>();
        services.AddSingleton<ITableValues, Table.TableValues>();
        services.AddSingleton<IDispelManager, Aura.DispelManager>();
        services.AddSingleton<IEffectManager, Effects.EffectManager>();
        services.AddSingleton<IAffectManager, Affects.AffectManager>();
        services.AddSingleton<IWeaponEffectManager, Effects.WeaponEffectManager>();

        services.AddSingleton<ILoginRepository, Repository.Filesystem.LoginRepository>();
        services.AddSingleton<IPlayerRepository, Repository.Filesystem.PlayerRepository>();
        services.AddSingleton<IAdminRepository, Repository.Filesystem.AdminRepository>();

        services.AddAutoMapper(typeof(Repository.Filesystem.AutoMapperProfile).Assembly);
    }

    internal void RegisterAllTypes(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var iRegistrable = typeof(IRegistrable);
        foreach (var registrable in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRegistrable.IsAssignableFrom(t))))
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering type {0}.", registrable.FullName);
            services.AddTransient(registrable);
        }
    }

    internal void RegisterFlagValues<TFlagValue>(IServiceCollection services, IEnumerable<Assembly> assemblies)
        where TFlagValue : IFlagValues<string>
    {
        var iFlagValuesType = typeof(TFlagValue);
        var concreteFlagValuesType = assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iFlagValuesType.IsAssignableFrom(t))).SingleOrDefault();
        if (concreteFlagValuesType == null)
            Log.Default.WriteLine(LogLevels.Error, "Cannot find an implementation for {0}.", iFlagValuesType.FullName);
        else
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering implementation type {0} for {1}.", concreteFlagValuesType.FullName, iFlagValuesType.FullName);
            services.AddTransient(iFlagValuesType, concreteFlagValuesType);
        }
    }

    internal class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies =>
        [
            typeof(Server.Server).Assembly,
            typeof(AcidBlast).Assembly
        ];
    }


    private void CreateDummyWorld()
    {
        // Blueprints
        // Rooms
        RoomBlueprint room1Blueprint = new RoomBlueprint
        {
            Id = 1,
            Name = "room1",
            Description = "My first room"
        };
        RoomBlueprint room2Blueprint = new RoomBlueprint
        {
            Id = 2,
            Name = "room2",
            Description = "My second room"
        };
        // Characters
        CharacterNormalBlueprint mob2Blueprint = new CharacterNormalBlueprint
        {
            Id = 2,
            Name = "mob2",
            ShortDescription = "Second mob (female)",
            Description = "Second mob (female) is here",
            Sex = Sex.Female,
            Level = 10
        };
        CharacterNormalBlueprint mob3Blueprint = new CharacterNormalBlueprint
        {
            Id = 3,
            Name = "mob3",
            ShortDescription = "Third mob (male)",
            Description = "Third mob (male) is here",
            Sex = Sex.Male,
            Level = 10
        };
        CharacterNormalBlueprint mob4Blueprint = new CharacterNormalBlueprint
        {
            Id = 4,
            Name = "mob4",
            ShortDescription = "Fourth mob (neutral)",
            Description = "Fourth mob (neutral) is here",
            Sex = Sex.Neutral,
            Level = 10
        };
        CharacterNormalBlueprint mob5Blueprint = new CharacterNormalBlueprint
        {
            Id = 5,
            Name = "mob5",
            ShortDescription = "Fifth mob (female)",
            Description = "Fifth mob (female) is here",
            Sex = Sex.Female,
            Level = 10
        };
        ItemContainerBlueprint item1Blueprint = new ItemContainerBlueprint
        {
            Id = 1,
            Name = "item1",
            ShortDescription = "First item (container)",
            Description = "The first item (container) has been left here.",
            MaxWeight = 100,
            WeightMultiplier = 100
        };
        ItemWeaponBlueprint item2Blueprint = new ItemWeaponBlueprint
        {
            Id = 2,
            Name = "item2",
            ShortDescription = "Second item (weapon)",
            Description = "The second item (weapon) has been left here.",
            Type = WeaponTypes.Axe,
            DiceCount = 10,
            DiceValue = 20,
            DamageType = SchoolTypes.Fire,
            WearLocation = WearLocations.Wield
        };
        ItemArmorBlueprint item3Blueprint = new ItemArmorBlueprint
        {
            Id = 3,
            Name = "item3",
            ShortDescription = "Third item (armor|feet)",
            Description = "The third item (armor|feet) has been left here.",
            Bash = 100,
            Pierce = 110,
            Slash = 120,
            Exotic = 130,
            WearLocation = WearLocations.Feet
        };
        ItemLightBlueprint item4Blueprint = new ItemLightBlueprint
        {
            Id = 4,
            Name = "item4",
            ShortDescription = "Fourth item (light)",
            Description = "The fourth item (light) has been left here.",
            DurationHours = -1,
            WearLocation = WearLocations.Light
        };
        ItemWeaponBlueprint item5Blueprint = new ItemWeaponBlueprint
        {
            Id = 5,
            Name = "item5",
            ShortDescription = "Fifth item (weapon)",
            Description = "The fifth item (weapon) has been left here.",
            Type = WeaponTypes.Sword,
            DiceCount = 5,
            DiceValue = 40,
            DamageType = SchoolTypes.Pierce,
            WearLocation = WearLocations.Wield
        };
        //
        if (_serviceProvider.GetRequiredService<IItemManager>().GetItemBlueprint(_serviceProvider.GetRequiredService<ISettings>().CorpseBlueprintId) == null)
        {
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
            {
                Id = _serviceProvider.GetRequiredService<ISettings>().CorpseBlueprintId,
                Name = "corpse"
            }; // this is mandatory
            _serviceProvider.GetRequiredService<IItemManager>().AddItemBlueprint(corpseBlueprint);
        }

        // World
        IArea midgaard = _serviceProvider.GetRequiredService<IAreaManager>().Areas.FirstOrDefault(x => x.DisplayName == "Midgaard");
        IRoom room1 = _serviceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), room1Blueprint, midgaard);
        IRoom room2 = _serviceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), room2Blueprint, midgaard);
        _serviceProvider.GetRequiredService<IRoomManager>().AddExit(room1, room2, null, ExitDirections.North);
        _serviceProvider.GetRequiredService<IRoomManager>().AddExit(room2, room1, null, ExitDirections.North);

        //ICharacter mob1 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "Mob1", Repository.ClassManager["Mage"], Repository.RaceManager["Troll"], Sex.Male, room1); // playable
        ICharacter mob2 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob2Blueprint, room1);
        ICharacter mob3 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob3Blueprint, room2);
        ICharacter mob4 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob4Blueprint, room2);
        ICharacter mob5 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob5Blueprint, room2);

        IItemContainer item1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, room1) as IItemContainer;
        IItemContainer item1Dup1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, room2) as IItemContainer;
        IItemWeapon item2 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item2Blueprint, mob2) as IItemWeapon;
        IItemArmor item3 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, item1Dup1) as IItemArmor;
        //IItemLight item4 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item4Blueprint, mob1);
        //IItemWeapon item5 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item5Blueprint, mob1);
        //IItemContainer item1Dup2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item1Blueprint, mob1);
        IItemArmor item3Dup1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, mob3) as IItemArmor;
        IItemLight item4Dup1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item4Blueprint, mob4) as IItemLight;
        // Equip weapon on mob2
        mob2.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand).Item = item2;
        item2.ChangeContainer(null);
        item2.ChangeEquippedBy(mob2, true);
    }

    private void CreateMidgaard()
    {
        MysteryLoader importer = new MysteryLoader();
        importer.Load(@"D:\Projects\Repos\OldMud\area\midgaard.are");
        importer.Parse();
        //MysteryImporter importer = new MysteryImporter();
        //string path = @"D:\GitHub\OldMud\area";
        //string fileList = Path.Combine(path, "area.lst");
        //string[] areaFilenames = File.ReadAllLines(fileList);
        //foreach (string areaFilename in areaFilenames)
        //{
        //    if (areaFilename.Contains("$"))
        //        break;
        //    string areaFullName = Path.Combine(path, areaFilename);
        //    importer.Load(areaFullName);
        //    importer.Parse();
        //}

        Dictionary<int, IArea> areasByVnums = new Dictionary<int, IArea>();
        Dictionary<int, IRoom> roomsByVNums = new Dictionary<int, IRoom>();

        // Create Areas
        foreach (AreaData importedArea in importer.Areas)
        {
            // TODO: levels
            IArea area = _serviceProvider.GetRequiredService<IAreaManager>().AddArea(Guid.NewGuid(), new AreaBlueprint { Name = importedArea.Name, Builders = importedArea.Builders, Credits = importedArea.Credits});
            areasByVnums.Add(importedArea.VNum, area);
        }

        // Create Rooms
        foreach (RoomData importedRoom in importer.Rooms)
        {
            IArea area = areasByVnums[importedRoom.AreaVnum];
            RoomBlueprint roomBlueprint = new RoomBlueprint
            {
                Id = importedRoom.VNum,
                Name = importedRoom.Name,
                Description = importedRoom.Description,
            };
            IRoom room = _serviceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), roomBlueprint, area);
            roomsByVNums.Add(importedRoom.VNum, room);
        }
        // Create Exits
        foreach (RoomData room in importer.Rooms)
        {
            for (int i = 0; i < RoomData.MaxExits - 1; i++)
            {
                ExitData exit = room.Exits[i];
                if (exit != null)
                {
                    IRoom from;
                    roomsByVNums.TryGetValue(room.VNum, out from);
                    IRoom to;
                    roomsByVNums.TryGetValue(exit.DestinationVNum, out to);
                    if (from == null)
                        Log.Default.WriteLine(LogLevels.Error, "Origin room not found for vnum {0}", room.VNum);
                    else if (to == null)
                        Log.Default.WriteLine(LogLevels.Error, "Destination room not found for vnum {0}", room.VNum);
                    else
                    {
                        _serviceProvider.GetRequiredService<IRoomManager>().AddExit(from, to, null, (ExitDirections) i);
                    }
                }
            }
        }
        //// Handle resets
        //foreach (RoomData importedRoom in importer.Rooms.Where(x => x.Resets.Any()))
        //{
        //    IRoom room;
        //    roomsByVNums.TryGetValue(importedRoom.VNum, out room);
        //    foreach (ResetData reset in importedRoom.Resets)
        //    {
        //        switch (reset.Command)
        //        {
        //            case 'M':
        //                MobileData mob = importer.Mobiles.FirstOrDefault(x => x.VNum == reset.Arg1);
        //                if (mob != null)
        //                    DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), mob.Name, room);
        //                break;
        //            case 'O':
        //                ObjectData obj = importer.Objects.FirstOrDefault(x => x.VNum == reset.Arg1);
        //                if (obj != null) // TODO: itemType
        //                    DependencyContainer.Instance.GetInstance<IWorld>().AddItemContainer(Guid.NewGuid(), obj.Name, room);
        //                break;
        //            // TODO: other command  P, E, G, D, R, Z
        //        }
        //    }
        //}

        CharacterNormalBlueprint mob2Blueprint = new CharacterNormalBlueprint
        {
            Id = 2,
            Name = "mob2",
            ShortDescription = "Second mob (female)",
            Description = "Second mob (female) is here",
            Sex = Sex.Female,
            Level = 10
        };
        CharacterNormalBlueprint mob3Blueprint = new CharacterNormalBlueprint
        {
            Id = 3,
            Name = "mob3",
            ShortDescription = "Third mob (male)",
            Description = "Third mob (male) is here",
            Sex = Sex.Male,
            Level = 10
        };
        CharacterNormalBlueprint mob4Blueprint = new CharacterNormalBlueprint
        {
            Id = 4,
            Name = "mob4",
            ShortDescription = "Fourth mob (neutral)",
            Description = "Fourth mob (neutral) is here",
            Sex = Sex.Neutral,
            Level = 10
        };
        CharacterNormalBlueprint mob5Blueprint = new CharacterNormalBlueprint
        {
            Id = 5,
            Name = "mob5",
            ShortDescription = "Fifth mob (female)",
            Description = "Fifth mob (female) is here",
            Sex = Sex.Female,
            Level = 10
        };
        ItemContainerBlueprint item1Blueprint = new ItemContainerBlueprint
        {
            Id = 1,
            Name = "item1",
            ShortDescription = "First item (container)",
            Description = "The first item (container) has been left here.",
            MaxWeight = 100,
            WeightMultiplier = 100
        };
        ItemWeaponBlueprint item2Blueprint = new ItemWeaponBlueprint
        {
            Id = 2,
            Name = "item2",
            ShortDescription = "Second item (weapon)",
            Description = "The second item (weapon) has been left here.",
            Type = WeaponTypes.Axe,
            DiceCount = 10,
            DiceValue = 20,
            DamageType = SchoolTypes.Fire,
            WearLocation = WearLocations.Wield
        };
        ItemArmorBlueprint item3Blueprint = new ItemArmorBlueprint
        {
            Id = 3,
            Name = "item3",
            ShortDescription = "Third item (armor|feet)",
            Description = "The third item (armor|feet) has been left here.",
            Bash = 100,
            Pierce = 110,
            Slash = 120,
            Exotic = 130,
            WearLocation = WearLocations.Feet
        };
        ItemLightBlueprint item4Blueprint = new ItemLightBlueprint
        {
            Id = 4,
            Name = "item4",
            ShortDescription = "Fourth item (light)",
            Description = "The fourth item (light) has been left here.",
            DurationHours = -1,
            WearLocation = WearLocations.Light
        };
        ItemWeaponBlueprint item5Blueprint = new ItemWeaponBlueprint
        {
            Id = 5,
            Name = "item5",
            ShortDescription = "Fifth item (weapon)",
            Description = "The fifth item (weapon) has been left here.",
            Type = WeaponTypes.Sword,
            DiceCount = 5,
            DiceValue = 40,
            DamageType = SchoolTypes.Pierce,
            WearLocation = WearLocations.Wield
        };

        //
        if (_serviceProvider.GetRequiredService<IItemManager>().GetItemBlueprint(_serviceProvider.GetRequiredService<ISettings>().CorpseBlueprintId) == null)
        {
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
            {
                Id = _serviceProvider.GetRequiredService<ISettings>().CorpseBlueprintId,
                Name = "corpse"
            }; // this is mandatory
            _serviceProvider.GetRequiredService<IItemManager>().AddItemBlueprint(corpseBlueprint);
        }

        // Add dummy mobs and items to allow impersonate :)
        IRoom templeOfMota = _serviceProvider.GetRequiredService<IRoomManager>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
        IRoom templeSquare = _serviceProvider.GetRequiredService<IRoomManager>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple square");

        //ICharacter mob1 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "mob1", Repository.ClassManager["Mage"], Repository.RaceManager["Troll"], Sex.Male, templeOfMota); // playable
        ICharacter mob2 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
        ICharacter mob3 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
        ICharacter mob4 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
        ICharacter mob5 = _serviceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

        IItemContainer item1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, templeOfMota) as IItemContainer;
        IItemContainer item1Dup1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, templeOfMota) as IItemContainer;
        IItemWeapon item2 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item2Blueprint, mob2) as IItemWeapon;
        IItemArmor item3 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, item1Dup1) as IItemArmor;
        //IItemLight item4 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item4Blueprint, mob1);
        //IItemWeapon item5 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item5Blueprint, mob1);
        //IItemContainer item1Dup2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item1Blueprint, mob1);
        IItemArmor item3Dup1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, mob3) as IItemArmor;
        IItemLight item4Dup1 = _serviceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item4Blueprint, mob4) as IItemLight;
        // Equip weapon on mob2
        mob2.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand).Item = item2;
        item2.ChangeContainer(null);
        item2.ChangeEquippedBy(mob2, true);
    }

    private void TestPaging()
    {
        TestPaging paging = new TestPaging();
        paging.SetData(new StringBuilder("1/Lorem ipsum dolor sit amet, " + Environment.NewLine +
                                         "2/consectetur adipiscing elit, " + Environment.NewLine +
                                         "3/sed do eiusmod tempor incididunt " + Environment.NewLine +
                                         "4/ut labore et dolore magna aliqua. " + Environment.NewLine +
                                         "5/Ut enim ad minim veniam, " + Environment.NewLine +
                                         "6/quis nostrud exercitation ullamco " + Environment.NewLine +
                                         "7/laboris nisi ut aliquip ex " + Environment.NewLine +
                                         "8/ea commodo consequat. " + Environment.NewLine +
                                         "9/Duis aute irure dolor in " + Environment.NewLine +
                                         "10/reprehenderit in voluptate velit " + Environment.NewLine +
                                         "11/esse cillum dolore eu fugiat " + Environment.NewLine +
                                         "12/nulla pariatur. " + Environment.NewLine +
                                         "13/Excepteur sint occaecat " + Environment.NewLine +
                                         "14/cupidatat non proident, " + Environment.NewLine +
                                         "15/sunt in culpa qui officia deserunt " + Environment.NewLine +
                                         "16/mollit anim id est laborum."));
        bool hasPaging1 = paging.HasPaging;
        string line1 = paging.GetNextLines(1);
        bool hasPaging2 = paging.HasPaging;
        string line2_10 = paging.GetNextLines(9);
        bool hasPaging3 = paging.HasPaging;
        string line11_19 = paging.GetNextLines(9);
        bool hasPaging4 = paging.HasPaging;
    }

    private void TestBasicCommands()
    {
        //IPlayer player1 = _serviceProvider.GetRequiredService<IPlayerManager>().AddPlayer(new ConsoleClient("Player1"), "Player1");
        //IPlayer player2 = _serviceProvider.GetRequiredService<IPlayerManager>().AddPlayer(new ConsoleClient("Player2"), "Player2");
        //IAdmin admin = _serviceProvider.GetRequiredService<IAdminManager>().AddAdmin(new ConsoleClient("Admin1"), "Admin1");

        //CreateDummyWorld();

        //player1.ProcessInput("impersonate mob1");
        //player1.ProcessInput("order"); // not controlling anyone
        //player1.ProcessInput("charm mob2");
        //player1.ProcessInput("test");
        //player1.ProcessInput("order test");

        //player1.ProcessInput("look");

        //player2.ProcessInput("gossip Hellow :)");
        //player2.ProcessInput("tell player1 Tsekwa =D");

        //player2.ProcessInput("i mob3");
        //player2.ProcessInput("charm mob2"); // not in same room
        //player2.ProcessInput("charm mob3"); // cannot charm itself (player2 is impersonated in mob3)
        //player2.ProcessInput("ch mob4");

        //player2.ProcessInput("look");

        //player1.ProcessInput("say Hello World!");

        //player2.ProcessInput("order charm mob5");

        //player2.ProcessInput("north"); // no exit on north
        //player2.ProcessInput("south");
        //player1.ProcessInput("south"); // no exit on south

        //player1.ProcessInput("say Hello World!");

        //player1.ProcessInput("/who");
        //admin.ProcessInput("who");

        ////player1.ProcessCommand("/commands");
        ////player1.ProcessCommand("commands");
        ////mob1.ProcessCommand("commands");
        ////admin.ProcessCommand("commands");
        ////Console.ReadLine();
    }

    private void TestCommandParsing()
    {
        //// server doesn't need to be started, we are not testing real runtime but basic commands
        //IArea area = _serviceProvider.GetRequiredService<IAreaManager>().AddArea(Guid.NewGuid(), new AreaBlueprint{Name = "testarea", Builders = "SinaC", Credits = "Credits"});
        //// Blueprints
        //RoomBlueprint room1Blueprint = new RoomBlueprint
        //{
        //    Id = 1,
        //    Name = "room1",
        //    Description = "My first room"
        //};
        //// World
        //IRoom room = _serviceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), room1Blueprint, area);

        //IPlayer player = _serviceProvider.GetRequiredService<IPlayerManager>().AddPlayer(new ConsoleClient("Player"), "Player");
        //player.ProcessInput("test");
        //player.ProcessInput("test arg1");
        //player.ProcessInput("test 'arg1' 'arg2' 'arg3' 'arg4'");
        //player.ProcessInput("test 'arg1 arg2' 'arg3 arg4'");
        //player.ProcessInput("test 'arg1 arg2\" arg3 arg4");
        //player.ProcessInput("test 3.arg1");
        //player.ProcessInput("test 2.'arg1'");
        //player.ProcessInput("test 2'.arg1'");
        //player.ProcessInput("test 2.'arg1 arg2' 3.arg3 5.arg4");
        //player.ProcessInput("test 2."); // INVALID
        //player.ProcessInput("test ."); // INVALID
        //player.ProcessInput("test '2.arg1'");
        //player.ProcessInput("unknown"); // INVALID
        //player.ProcessInput("/test");

        //IPlayableCharacter character = _serviceProvider.GetRequiredService<ICharacterManager>().AddPlayableCharacter(Guid.NewGuid(), new PlayableCharacterData
        //{
        //    Name = "toto",
        //    Class = _serviceProvider.GetRequiredService<IClassManager>()["Mage"].Name,
        //    Race = _serviceProvider.GetRequiredService<IRaceManager>()["Troll"].Name,
        //    Sex = Sex.Male,
        //    Level = 1,
        //    Experience = 0,
        //    RoomId = room.Blueprint.Id
        //}, player, room);
        //character.ProcessInput("look");
        //character.ProcessInput("tell"); // INVALID because Player commands are not accessible by Character
        //character.ProcessInput("unknown"); // INVALID

        //player.ProcessInput("impersonate"); // impossible but doesn't cause an error log to un-impersonate, player must already be impersonated
        //player.ProcessInput("impersonate character");
        //player.ProcessInput("/tell");
        //player.ProcessInput("tell"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
        //player.ProcessInput("look");

        //player.ProcessInput("impersonate"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
        //player.ProcessInput("/impersonate");
        //player.ProcessInput("/tell");
        //player.ProcessInput("tell");
        //player.ProcessInput("look"); // INVALID because Character commands are not accessible by Player unless if impersonating

        //IAdmin admin = _serviceProvider.GetRequiredService<IAdminManager>().AddAdmin(new ConsoleClient("Admin"), "Admin");
        //admin.ProcessInput("incarnate");
        //admin.ProcessInput("unknown"); // INVALID
    }

    private void TestImport()
    {
        MysteryLoader importer = new MysteryLoader();
        importer.Load(@"D:\GitHub\OldMud\area\midgaard.are");
        importer.Parse();

        //MysteryImporter importer = new MysteryImporter();
        //string path = @"D:\GitHub\OldMud\area";
        //string fileList = Path.Combine(path, "area.lst");
        //string[] areaFilenames = File.ReadAllLines(fileList);
        //foreach (string areaFilename in areaFilenames)
        //{
        //    if (areaFilename.Contains("$"))
        //        break;
        //    string areaFullName = Path.Combine(path, areaFilename);
        //    importer.Load(areaFullName);
        //    importer.Parse();
        //}
    }

    private void TestWorldOnline()
    {
        Console.WriteLine("Let's go");

        //CreateDummyWorld();
        CreateMidgaard();

        INetworkServer telnetServer = new TelnetServer(11000);
        _serviceProvider.GetRequiredService<IServer>().Initialize(new List<INetworkServer> {telnetServer});
        _serviceProvider.GetRequiredService<IServer>().Start();

        bool stopped = false;
        while (!stopped)
        {
            if (Console.KeyAvailable)
            {
                string line = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    // server commands
                    if (line.StartsWith("#"))
                    {
                        line = line.Replace("#", string.Empty).ToLower();
                        if (line == "exit" || line == "quit")
                        {
                            stopped = true;
                            break;
                        }
                        else if (line == "alist")
                        {
                            Console.WriteLine("Admins:");
                            foreach (IAdmin a in _serviceProvider.GetRequiredService<IAdminManager>().Admins)
                                Console.WriteLine(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
                        }
                        else if (line == "plist")
                        {
                            Console.WriteLine("players:");
                            foreach (IPlayer p in _serviceProvider.GetRequiredService<IPlayerManager>().Players)
                                Console.WriteLine(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
                        }

                        // TODO: characters/rooms/items
                    }
                }
            }
            else
                Thread.Sleep(100);
        }

        _serviceProvider.GetRequiredService<IServer>().Stop();
    }

    private void TestWorldOffline()
    {
        Console.WriteLine("Let's go");
        //CreateDummyWorld();

        ConsoleNetworkServer consoleNetworkServer = new ConsoleNetworkServer(_serviceProvider);
        consoleNetworkServer.Initialize();
        CreateMidgaard();
        _serviceProvider.GetRequiredService<IServer>().Initialize(new List<INetworkServer> { consoleNetworkServer });
        consoleNetworkServer.AddClient("Player1", false, true);
        _serviceProvider.GetRequiredService<IServer>().Start(); // this call is blocking because consoleNetworkServer.Start is blocking

        _serviceProvider.GetRequiredService<IServer>().Stop();
    }
}
