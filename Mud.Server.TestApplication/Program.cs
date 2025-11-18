using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Importer.Mystery;
using Mud.Importer.Rom;
using Mud.Logger;
using Mud.Network.Interfaces;
using Mud.Network.Telnet;
using Mud.POC;
using Mud.Repository;
using Mud.Server.Area;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character;
using Mud.Server.Flags;
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
using Mud.Server.Item;
using Mud.Server.Quest;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;
using Mud.Server.Room;
using Mud.Settings.Interfaces;
using System.Reflection;
using System.Text;

namespace Mud.Server.TestApplication;

internal class Program
{
    private IServiceProvider ServiceProvider { get; set; }

    static void Main(string[] args)
    {
        var program = new Program();
        program.Run();
    }

    private void Run()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        ServiceProvider.GetRequiredService<ILogger<Program>>().LogError("***ERROR***");

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
        //services.AddLogging(lb => lb.AddConsole());
        services.AddLogging(builder => builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        }));

        // Register Services
        services.AddSingleton<ISettings>(settings);

        RegisterAllTypes(services, assemblyHelper.AllReferencedAssemblies);

        services.AddSingleton<IRandomManager>(new RandomManager()); // 2 ctors => injector can't decide which one to choose
        services.AddSingleton<IAssemblyHelper>(assemblyHelper);
        services.AddSingleton<IAbilityManager, Ability.AbilityManager>();
        services.AddSingleton<IGameActionManager, GameAction.GameActionManager>();
        services.AddSingleton<ITimeManager, Server.TimeManager>();
        services.AddSingleton<IQuestManager, Quest.QuestManager>();
        services.AddSingleton<IAuraManager, Aura.AuraManager>();
        services.AddSingleton<IItemManager, Item.ItemManager>();
        services.AddSingleton<ICharacterManager, Character.CharacterManager>();
        services.AddSingleton<IRoomManager, Room.RoomManager>();
        services.AddSingleton<IAreaManager, Area.AreaManager>();
        services.AddSingleton<IAdminManager, Admin.AdminManager>();
        services.AddSingleton<IWiznet, Server.Wiznet>();
        services.AddSingleton<IResetManager, Server.ResetManager>();
        services.AddSingleton<IServer, Server.Server>();
        services.AddSingleton<IWorld, Server.Server>(); // Server also implements IWorld
        services.AddSingleton<IPlayerManager, Server.Server>(); // Server also implements IPlayerManager
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
        // register commands and abilities
        var iRegistrable = typeof(IRegistrable);
        foreach (var registrable in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRegistrable.IsAssignableFrom(t))))
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering type {0}.", registrable.FullName);
            services.AddTransient(registrable);
        }
        // register races
        var iRace = typeof(IRace);
        foreach (var race in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRace.IsAssignableFrom(t))))
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering race type {0}.", race.FullName);
            services.AddSingleton(iRace, race);
        }
        // register classes
        var iClass = typeof(IClass);
        foreach (var cl in assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iClass.IsAssignableFrom(t))))
        {
            Log.Default.WriteLine(LogLevels.Info, "Registering class type {0}.", cl.FullName);
            services.AddSingleton(iClass, cl);
        }
        // register flag
        RegisterFlagValues<ICharacterFlagValues>(services, assemblies);
        RegisterFlagValues<IRoomFlagValues>(services, assemblies);
        RegisterFlagValues<IItemFlagValues>(services, assemblies);
        RegisterFlagValues<IWeaponFlagValues>(services, assemblies);
        RegisterFlagValues<IActFlagValues>(services, assemblies);
        RegisterFlagValues<IOffensiveFlagValues>(services, assemblies);
        RegisterFlagValues<IAssistFlagValues>(services, assemblies);
        RegisterFlagValues<IIRVFlagValues>(services, assemblies);
        RegisterFlagValues<IBodyFormValues>(services, assemblies);
        RegisterFlagValues<IBodyPartValues>(services, assemblies);

        // register group
        services.AddTransient<IGroup, Group.Group>();
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
            typeof(Commands.Actor.Commands).Assembly,
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
        if (ServiceProvider.GetRequiredService<IItemManager>().GetItemBlueprint(ServiceProvider.GetRequiredService<ISettings>().CorpseBlueprintId) == null)
        {
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
            {
                Id = ServiceProvider.GetRequiredService<ISettings>().CorpseBlueprintId,
                Name = "corpse"
            }; // this is mandatory
            ServiceProvider.GetRequiredService<IItemManager>().AddItemBlueprint(corpseBlueprint);
        }

        // World
        IArea midgaard = ServiceProvider.GetRequiredService<IAreaManager>().Areas.FirstOrDefault(x => x.DisplayName == "Midgaard");
        IRoom room1 = ServiceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), room1Blueprint, midgaard);
        IRoom room2 = ServiceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), room2Blueprint, midgaard);
        ServiceProvider.GetRequiredService<IRoomManager>().AddExit(room1, room2, null, ExitDirections.North);
        ServiceProvider.GetRequiredService<IRoomManager>().AddExit(room2, room1, null, ExitDirections.North);

        //ICharacter mob1 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "Mob1", Repository.ClassManager["Mage"], Repository.RaceManager["Troll"], Sex.Male, room1); // playable
        ICharacter mob2 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob2Blueprint, room1);
        ICharacter mob3 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob3Blueprint, room2);
        ICharacter mob4 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob4Blueprint, room2);
        ICharacter mob5 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob5Blueprint, room2);

        IItemContainer item1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, room1) as IItemContainer;
        IItemContainer item1Dup1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, room2) as IItemContainer;
        IItemWeapon item2 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item2Blueprint, mob2) as IItemWeapon;
        IItemArmor item3 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, item1Dup1) as IItemArmor;
        //IItemLight item4 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item4Blueprint, mob1);
        //IItemWeapon item5 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item5Blueprint, mob1);
        //IItemContainer item1Dup2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item1Blueprint, mob1);
        IItemArmor item3Dup1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, mob3) as IItemArmor;
        IItemLight item4Dup1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item4Blueprint, mob4) as IItemLight;
        // Equip weapon on mob2
        mob2.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand).Item = item2;
        item2.ChangeContainer(null);
        item2.ChangeEquippedBy(mob2, true);
    }

    /*
    private void CreateMidgaard()
    {
        MysteryLoader importer = new MysteryLoader();
        importer.Load(@"D:\Projects\Repos\OldMud\area\midgaard.are");
        importer.Parse();
        //MysteryImporter importer = new MysteryImporter(ServiceProvider);
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
            IArea area = ServiceProvider.GetRequiredService<IAreaManager>().AddArea(Guid.NewGuid(), new AreaBlueprint { Name = importedArea.Name, Builders = importedArea.Builders, Credits = importedArea.Credits});
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
            IRoom room = ServiceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), roomBlueprint, area);
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
                        ServiceProvider.GetRequiredService<IRoomManager>().AddExit(from, to, null, (ExitDirections) i);
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
        if (ServiceProvider.GetRequiredService<IItemManager>().GetItemBlueprint(ServiceProvider.GetRequiredService<ISettings>().CorpseBlueprintId) == null)
        {
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
            {
                Id = ServiceProvider.GetRequiredService<ISettings>().CorpseBlueprintId,
                Name = "corpse"
            }; // this is mandatory
            ServiceProvider.GetRequiredService<IItemManager>().AddItemBlueprint(corpseBlueprint);
        }

        // Add dummy mobs and items to allow impersonate :)
        IRoom templeOfMota = ServiceProvider.GetRequiredService<IRoomManager>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
        IRoom templeSquare = ServiceProvider.GetRequiredService<IRoomManager>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple square");

        //ICharacter mob1 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "mob1", Repository.ClassManager["Mage"], Repository.RaceManager["Troll"], Sex.Male, templeOfMota); // playable
        ICharacter mob2 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
        ICharacter mob3 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
        ICharacter mob4 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
        ICharacter mob5 = ServiceProvider.GetRequiredService<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

        IItemContainer item1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, templeOfMota) as IItemContainer;
        IItemContainer item1Dup1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, templeOfMota) as IItemContainer;
        IItemWeapon item2 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item2Blueprint, mob2) as IItemWeapon;
        IItemArmor item3 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, item1Dup1) as IItemArmor;
        //IItemLight item4 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item4Blueprint, mob1);
        //IItemWeapon item5 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item5Blueprint, mob1);
        //IItemContainer item1Dup2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item1Blueprint, mob1);
        IItemArmor item3Dup1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, mob3) as IItemArmor;
        IItemLight item4Dup1 = ServiceProvider.GetRequiredService<IItemManager>().AddItem(Guid.NewGuid(), item4Blueprint, mob4) as IItemLight;
        // Equip weapon on mob2
        mob2.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand).Item = item2;
        item2.ChangeContainer(null);
        item2.ChangeEquippedBy(mob2, true);
    }
    */

    private void CreateWorld()
    {
        var path = @"D:\Projects\Repos\Mud.Net\Datas\Areas\Rom24\";
        RomImporter importer = new(ServiceProvider);
        //MysteryImporter importer = new MysteryImporter();
        //RotImporter importer = new RotImporter();
        importer.Import(path, "limbo.are", "midgaard.are", "smurf.are", "hitower.are");
        //importer.ImportByList(path, "area.lst");

        var AreaManager = ServiceProvider.GetRequiredService<IAreaManager>();
        var RoomManager = ServiceProvider.GetRequiredService<IRoomManager>();
        var CharacterManager = ServiceProvider.GetRequiredService<ICharacterManager>();
        var ItemManager = ServiceProvider.GetRequiredService<IItemManager>();
        var QuestManager = ServiceProvider.GetRequiredService<IQuestManager>();
        var RandomManager = ServiceProvider.GetRequiredService<IRandomManager>();

        // Area
        foreach (AreaBlueprint blueprint in importer.Areas)
        {
            AreaManager.AddAreaBlueprint(blueprint);
            AreaManager.AddArea(Guid.NewGuid(), blueprint);
        }

        // Rooms
        foreach (RoomBlueprint blueprint in importer.Rooms)
        {
            RoomManager.AddRoomBlueprint(blueprint);
            IArea area = AreaManager.Areas.FirstOrDefault(x => x.Blueprint.Id == blueprint.AreaId);
            if (area == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Area id {0} not found", blueprint.AreaId);
            }
            else
                RoomManager.AddRoom(Guid.NewGuid(), blueprint, area);
        }

        foreach (IRoom room in RoomManager.Rooms)
        {
            foreach (ExitBlueprint exitBlueprint in room.Blueprint.Exits.Where(x => x != null))
            {
                IRoom to = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == exitBlueprint.Destination);
                if (to == null)
                    Log.Default.WriteLine(LogLevels.Warning, "Destination room {0} not found for room {1} direction {2}", exitBlueprint.Destination, room.Blueprint.Id, exitBlueprint.Direction);
                else
                    RoomManager.AddExit(room, to, exitBlueprint, exitBlueprint.Direction);
            }
        }

        // Characters
        foreach (CharacterBlueprintBase blueprint in importer.Characters)
            CharacterManager.AddCharacterBlueprint(blueprint);

        // Items
        foreach (ItemBlueprintBase blueprint in importer.Items)
            ItemManager.AddItemBlueprint(blueprint);

        // Custom blueprint to test
        ItemQuestBlueprint questItem1Blueprint = new ItemQuestBlueprint
        {
            Id = 80000,
            Name = "Quest item 1",
            ShortDescription = "Quest item 1",
            Description = "The quest item 1 has been left here."
        };
        ItemManager.AddItemBlueprint(questItem1Blueprint);
        ItemQuestBlueprint questItem2Blueprint = new ItemQuestBlueprint
        {
            Id = 80001,
            Name = "Quest item 2",
            ShortDescription = "Quest item 2",
            Description = "The quest item 2 has been left here."
        };
        ItemManager.AddItemBlueprint(questItem2Blueprint);
        CharacterNormalBlueprint construct = new CharacterNormalBlueprint
        {
            Id = 80000,
            Name = "Construct",
            ShortDescription = "A construct",
            LongDescription = "A construct waiting orders",
            Description = "A construct is here, built from various of gears and springs",
            Sex = Sex.Neutral,
            Level = 40,
            Wealth = 0,
            Alignment = 0,
            DamageNoun = "buzz",
            DamageType = SchoolTypes.Bash,
            DamageDiceCount = 5,
            DamageDiceValue = 10,
            DamageDiceBonus = 10,
            HitPointDiceCount = 20,
            HitPointDiceValue = 30,
            HitPointDiceBonus = 300,
            ManaDiceCount = 0,
            ManaDiceValue = 0,
            ManaDiceBonus = 0,
            HitRollBonus = 10,
            ArmorBash = 300,
            ArmorPierce = 200,
            ArmorSlash = 400,
            ArmorExotic = 0,
            ActFlags = new ActFlags(ServiceProvider, "Pet"),
            OffensiveFlags = new OffensiveFlags(ServiceProvider, "Bash"),
            CharacterFlags = new CharacterFlags(ServiceProvider, "Haste"),
            Immunities = new IRVFlags(ServiceProvider),
            Resistances = new IRVFlags(ServiceProvider, "Slash", "Fire"),
            Vulnerabilities = new IRVFlags(ServiceProvider, "Acid"),
        };
        CharacterManager.AddCharacterBlueprint(construct);

        // MANDATORY ITEMS
        if (ItemManager.GetItemBlueprint(10) == null)
        {
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
            {
                Id = 10,
                NoTake = true,
                Name = "corpse"
            };
            ItemManager.AddItemBlueprint(corpseBlueprint);
        }
        if (ItemManager.GetItemBlueprint(5) == null)
        {
            ItemMoneyBlueprint moneyBlueprint = new ItemMoneyBlueprint
            {
                Id = 5,
                NoTake = true,
                Name = "coins"
            };
            ItemManager.AddItemBlueprint(moneyBlueprint);
        }
        // MANDATORY ROOM
        RoomBlueprint voidBlueprint = RoomManager.GetRoomBlueprint(1);
        if (voidBlueprint == null)
        {
            IArea area = AreaManager.Areas.First();
            Log.Default.WriteLine(LogLevels.Error, "NullRoom not found -> creation of null room with id {0} in area {1}", 1, area.DisplayName);
            voidBlueprint = new RoomBlueprint
            {
                Id = 1,
                Name = "The void",
                RoomFlags = new RoomFlags(ServiceProvider, "NoRecall", "NoScan", "NoWhere")
            };
            RoomManager.AddRoomBlueprint(voidBlueprint);
            RoomManager.AddRoom(Guid.NewGuid(), voidBlueprint, area);
        }

        // Add dummy mobs and items to allow impersonate :)
        IRoom templeOfMota = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3001);
        IRoom templeSquare = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3005);
        IRoom marketSquare = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3014);
        IRoom commonSquare = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3025);

        if (templeOfMota == null || templeSquare == null || marketSquare == null || commonSquare == null)
            return;

        ItemManager.AddItem(Guid.NewGuid(), questItem2Blueprint, templeSquare); // TODO: this should be added dynamically when player takes the quest

        // Quest
        QuestKillLootTable<int> quest1KillLoot = new QuestKillLootTable<int>(RandomManager)
        {
            Name = "Quest 1 kill 1 table",
            Entries = new List<QuestKillLootTableEntry<int>>
            {
                new QuestKillLootTableEntry<int>
                {
                    Value = questItem1Blueprint.Id,
                    Percentage = 80,
                }
            }
        };
        QuestBlueprint questBlueprint1 = new QuestBlueprint
        {
            Id = 1,
            Title = "Complex quest",
            Description = "Kill 3 fido, get one quest item 2, get 2 quest item 1 on beggar and explore temple square",
            Level = 50,
            Experience = 50000,
            Gold = 20,
            ShouldQuestItemBeDestroyed = true,
            KillObjectives = new[]
            {
                new QuestKillObjectiveBlueprint
                {
                    Id = 0,
                    CharacterBlueprintId = 3062, // fido
                    Count = 3
                }
            },
            ItemObjectives = new[]
            {
                new QuestItemObjectiveBlueprint
                {
                    Id = 1,
                    ItemBlueprintId = questItem2Blueprint.Id,
                    Count = 1
                },
                new QuestItemObjectiveBlueprint
                {
                    Id = 2,
                    ItemBlueprintId = questItem1Blueprint.Id,
                    Count = 2
                }
            },
            LocationObjectives = new[]
            {
                new QuestLocationObjectiveBlueprint
                {
                    Id = 3,
                    RoomBlueprintId = templeSquare.Blueprint.Id,
                }
            },
            KillLootTable = new Dictionary<int, QuestKillLootTable<int>> // when killing mob 3065, we receive quest item 1 (80%)
            {
                { 3065, quest1KillLoot } // beggar
            }
            // TODO: rewards
        };
        QuestManager.AddQuestBlueprint(questBlueprint1);

        QuestBlueprint questBlueprint2 = new QuestBlueprint
        {
            Id = 2,
            Title = "Simple exploration quest",
            Description = "Explore temple of mota, temple square, market square and common square",
            Level = 10,
            Experience = 10000,
            Gold = 20,
            TimeLimit = 5,
            LocationObjectives = new[]
            {
                new QuestLocationObjectiveBlueprint
                {
                    Id = 0,
                    RoomBlueprintId = templeOfMota.Blueprint.Id
                },
                new QuestLocationObjectiveBlueprint
                {
                    Id = 1,
                    RoomBlueprintId = templeSquare.Blueprint.Id
                },
                new QuestLocationObjectiveBlueprint
                {
                    Id = 2,
                    RoomBlueprintId = marketSquare.Blueprint.Id
                },
                new QuestLocationObjectiveBlueprint
                {
                    Id = 3,
                    RoomBlueprintId = commonSquare.Blueprint.Id
                }
            },
            // TODO: rewards
        };
        QuestManager.AddQuestBlueprint(questBlueprint2);

        CharacterQuestorBlueprint mob10Blueprint = new CharacterQuestorBlueprint
        {
            Id = 10,
            Name = "mob10 questor",
            ShortDescription = "Tenth mob (neutral questor)",
            Description = "Tenth mob (neutral questor) is here",
            Sex = Sex.Neutral,
            Level = 60,
            QuestBlueprints = new[]
            {
                questBlueprint1,
                questBlueprint2
            }
        };
        CharacterManager.AddCharacterBlueprint(mob10Blueprint);
        ICharacter mob10 = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), mob10Blueprint, commonSquare);
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
        //IPlayer player1 = ServiceProvider.GetRequiredService<IPlayerManager>().AddPlayer(new ConsoleClient("Player1"), "Player1");
        //IPlayer player2 = ServiceProvider.GetRequiredService<IPlayerManager>().AddPlayer(new ConsoleClient("Player2"), "Player2");
        //IAdmin admin = ServiceProvider.GetRequiredService<IAdminManager>().AddAdmin(new ConsoleClient("Admin1"), "Admin1");

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
        //IArea area = ServiceProvider.GetRequiredService<IAreaManager>().AddArea(Guid.NewGuid(), new AreaBlueprint{Name = "testarea", Builders = "SinaC", Credits = "Credits"});
        //// Blueprints
        //RoomBlueprint room1Blueprint = new RoomBlueprint
        //{
        //    Id = 1,
        //    Name = "room1",
        //    Description = "My first room"
        //};
        //// World
        //IRoom room = ServiceProvider.GetRequiredService<IRoomManager>().AddRoom(Guid.NewGuid(), room1Blueprint, area);

        //IPlayer player = ServiceProvider.GetRequiredService<IPlayerManager>().AddPlayer(new ConsoleClient("Player"), "Player");
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

        //IPlayableCharacter character = ServiceProvider.GetRequiredService<ICharacterManager>().AddPlayableCharacter(Guid.NewGuid(), new PlayableCharacterData
        //{
        //    Name = "toto",
        //    Class = ServiceProvider.GetRequiredService<IClassManager>()["Mage"].Name,
        //    Race = ServiceProvider.GetRequiredService<IRaceManager>()["Troll"].Name,
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

        //IAdmin admin = ServiceProvider.GetRequiredService<IAdminManager>().AddAdmin(new ConsoleClient("Admin"), "Admin");
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
        //CreateMidgaard();
        CreateWorld();

        INetworkServer telnetServer = new TelnetServer(11000);
        ServiceProvider.GetRequiredService<IServer>().Initialize(new List<INetworkServer> {telnetServer});
        ServiceProvider.GetRequiredService<IServer>().Start();

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
                            foreach (IAdmin a in ServiceProvider.GetRequiredService<IAdminManager>().Admins)
                                Console.WriteLine(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
                        }
                        else if (line == "plist")
                        {
                            Console.WriteLine("players:");
                            foreach (IPlayer p in ServiceProvider.GetRequiredService<IPlayerManager>().Players)
                                Console.WriteLine(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
                        }

                        // TODO: characters/rooms/items
                    }
                }
            }
            else
                Thread.Sleep(100);
        }

        ServiceProvider.GetRequiredService<IServer>().Stop();
    }

    private void TestWorldOffline()
    {
        Console.WriteLine("Let's go");
        //CreateDummyWorld();

        ConsoleNetworkServer consoleNetworkServer = new ConsoleNetworkServer(ServiceProvider);
        consoleNetworkServer.Initialize();
        //CreateMidgaard();
        CreateWorld();
        ServiceProvider.GetRequiredService<IServer>().Initialize(new List<INetworkServer> { consoleNetworkServer });
        consoleNetworkServer.AddClient("Player1", false, true);
        ServiceProvider.GetRequiredService<IServer>().Start(); // this call is blocking because consoleNetworkServer.Start is blocking

        ServiceProvider.GetRequiredService<IServer>().Stop();
    }
}
