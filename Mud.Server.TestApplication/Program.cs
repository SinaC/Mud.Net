using Mud.Container;
using Mud.Domain;
using Mud.Importer.Mystery;
using Mud.Logger;
using Mud.Network.Interfaces;
using Mud.Network.Telnet;
using Mud.POC;
using Mud.POC.TestLua;
using Mud.Repository;
using Mud.Server.Ability;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
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
using Mud.Server.Server;
using Mud.Settings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Mud.Server.TestApplication
{
    internal class Program
    {
        internal class AssemblyHelper : IAssemblyHelper
        {
            public IEnumerable<Assembly> AllReferencedAssemblies => new Assembly[] { typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly };
        }


        internal static void RegisterAllTypes(IAssemblyHelper assemblyHelper)
        {
            Type iRegistrable = typeof(IRegistrable);
            foreach (var registrable in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iRegistrable.IsAssignableFrom(t))))
                DependencyContainer.Current.Register(registrable);
        }

        private static void Main(string[] args)
        {
            // Initialize settings
            ISettings settings = new Settings.ConfigurationManager.Settings();
            DependencyContainer.Current.RegisterInstance<ISettings>(settings);

            // Initialize log
            Log.Default.Initialize(settings.LogPath, "server.test.log");

            //// Register all needed types
            //RegisterAllTypes(new AssemblyHelper());

            //// Initialize IOC container
            //DependencyContainer.Current.RegisterInstance<IRandomManager>(new RandomManager()); // 2 ctors => injector can't decide which one to choose
            //DependencyContainer.Current.RegisterInstance<IAssemblyHelper>(new AssemblyHelper());
            //DependencyContainer.Current.RegisterInstance<IAbilityManager>(new AbilityManager(new AssemblyHelper())); // this is needed because AbilityManager will register type and container doesn't accept registering after first resolve
            //DependencyContainer.Current.RegisterInstance<IGameActionManager>(new GameActionManager(new AssemblyHelper())); // this is needed because AbilityManager will register type and container doesn't accept registering after first resolve
            //DependencyContainer.Current.Register<ITimeManager, TimeManager>(SimpleInjector.Lifestyle.Singleton);
            //DependencyContainer.Current.Register<IWorld, World.World>(SimpleInjector.Lifestyle.Singleton);
            //DependencyContainer.Current.Register<IQuestManager, World.World>(SimpleInjector.Lifestyle.Singleton); // World also implements IQuestManager
            //DependencyContainer.Current.Register<IAuraManager, World.World>(SimpleInjector.Lifestyle.Singleton); // World also implements IAuraManager
            //DependencyContainer.Current.Register<IItemManager, World.World>(SimpleInjector.Lifestyle.Singleton); // World also implements IItemManager
            //DependencyContainer.Current.Register<ICharacterManager, World.World>(SimpleInjector.Lifestyle.Singleton); // World also implements ICharacterManager
            //DependencyContainer.Current.Register<IRoomManager, World.World>(SimpleInjector.Lifestyle.Singleton); // World also implements IRoomManager
            //DependencyContainer.Current.Register<IAreaManager, World.World>(SimpleInjector.Lifestyle.Singleton); // World also implements IAreaManager
            //DependencyContainer.Current.Register<IServer, Server.Server>(SimpleInjector.Lifestyle.Singleton);
            //DependencyContainer.Current.Register<IWiznet, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IWiznet
            //DependencyContainer.Current.Register<IPlayerManager, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IPlayerManager
            //DependencyContainer.Current.Register<IAdminManager, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IAdminManager
            //DependencyContainer.Current.Register<IServerAdminCommand, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IServerAdminCommand
            //DependencyContainer.Current.Register<IServerPlayerCommand, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IServerPlayerCommand
            //DependencyContainer.Current.Register<IClassManager, Class.ClassManager>(SimpleInjector.Lifestyle.Singleton);
            //DependencyContainer.Current.Register<IRaceManager, Race.RaceManager>(SimpleInjector.Lifestyle.Singleton);
            //DependencyContainer.Current.Register<IUniquenessManager, Server.UniquenessManager>(SimpleInjector.Lifestyle.Singleton);
            //DependencyContainer.Current.Register<ITableValues, Table.TableValues>(SimpleInjector.Lifestyle.Singleton);


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

        private static void CreateDummyWorld()
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
            if (DependencyContainer.Current.GetInstance<IItemManager>().GetItemBlueprint(DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId) == null)
            {
                ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
                {
                    Id = DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId,
                    Name = "corpse"
                }; // this is mandatory
                DependencyContainer.Current.GetInstance<IItemManager>().AddItemBlueprint(corpseBlueprint);
            }

            // World
            IArea midgaard = DependencyContainer.Current.GetInstance<IAreaManager>().Areas.FirstOrDefault(x => x.DisplayName == "Midgaard");
            IRoom room1 = DependencyContainer.Current.GetInstance<IRoomManager>().AddRoom(Guid.NewGuid(), room1Blueprint, midgaard);
            IRoom room2 = DependencyContainer.Current.GetInstance<IRoomManager>().AddRoom(Guid.NewGuid(), room2Blueprint, midgaard);
            DependencyContainer.Current.GetInstance<IRoomManager>().AddExit(room1, room2, null, ExitDirections.North);
            DependencyContainer.Current.GetInstance<IRoomManager>().AddExit(room2, room1, null, ExitDirections.North);

            //ICharacter mob1 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "Mob1", Repository.ClassManager["Mage"], Repository.RaceManager["Troll"], Sex.Male, room1); // playable
            ICharacter mob2 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob2Blueprint, room1);
            ICharacter mob3 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob3Blueprint, room2);
            ICharacter mob4 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob4Blueprint, room2);
            ICharacter mob5 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob5Blueprint, room2);

            IItemContainer item1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, room1) as IItemContainer;
            IItemContainer item1Dup1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, room2) as IItemContainer;
            IItemWeapon item2 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item2Blueprint, mob2) as IItemWeapon;
            IItemArmor item3 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, item1Dup1) as IItemArmor;
            //IItemLight item4 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item4Blueprint, mob1);
            //IItemWeapon item5 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item5Blueprint, mob1);
            //IItemContainer item1Dup2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item1Blueprint, mob1);
            IItemArmor item3Dup1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, mob3) as IItemArmor;
            IItemLight item4Dup1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item4Blueprint, mob4) as IItemLight;
            // Equip weapon on mob2
            mob2.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand).Item = item2;
            item2.ChangeContainer(null);
            item2.ChangeEquippedBy(mob2, true);
        }

        private static void CreateMidgaard()
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
                IArea area = DependencyContainer.Current.GetInstance<IAreaManager>().AddArea(Guid.NewGuid(), new AreaBlueprint { Name = importedArea.Name, Builders = importedArea.Builders, Credits = importedArea.Credits});
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
                IRoom room = DependencyContainer.Current.GetInstance<IRoomManager>().AddRoom(Guid.NewGuid(), roomBlueprint, area);
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
                            DependencyContainer.Current.GetInstance<IRoomManager>().AddExit(from, to, null, (ExitDirections) i);
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
            if (DependencyContainer.Current.GetInstance<IItemManager>().GetItemBlueprint(DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId) == null)
            {
                ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
                {
                    Id = DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId,
                    Name = "corpse"
                }; // this is mandatory
                DependencyContainer.Current.GetInstance<IItemManager>().AddItemBlueprint(corpseBlueprint);
            }

            // Add dummy mobs and items to allow impersonate :)
            IRoom templeOfMota = DependencyContainer.Current.GetInstance<IRoomManager>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
            IRoom templeSquare = DependencyContainer.Current.GetInstance<IRoomManager>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple square");

            //ICharacter mob1 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "mob1", Repository.ClassManager["Mage"], Repository.RaceManager["Troll"], Sex.Male, templeOfMota); // playable
            ICharacter mob2 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
            ICharacter mob3 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
            ICharacter mob4 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
            ICharacter mob5 = DependencyContainer.Current.GetInstance<ICharacterManager>().AddNonPlayableCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

            IItemContainer item1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, templeOfMota) as IItemContainer;
            IItemContainer item1Dup1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item1Blueprint, templeOfMota) as IItemContainer;
            IItemWeapon item2 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item2Blueprint, mob2) as IItemWeapon;
            IItemArmor item3 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, item1Dup1) as IItemArmor;
            //IItemLight item4 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item4Blueprint, mob1);
            //IItemWeapon item5 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item5Blueprint, mob1);
            //IItemContainer item1Dup2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), item1Blueprint, mob1);
            IItemArmor item3Dup1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item3Blueprint, mob3) as IItemArmor;
            IItemLight item4Dup1 = DependencyContainer.Current.GetInstance<IItemManager>().AddItem(Guid.NewGuid(), item4Blueprint, mob4) as IItemLight;
            // Equip weapon on mob2
            mob2.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand).Item = item2;
            item2.ChangeContainer(null);
            item2.ChangeEquippedBy(mob2, true);
        }

        private static void TestPaging()
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

        private static void TestBasicCommands()
        {
            //IPlayer player1 = DependencyContainer.Current.GetInstance<IPlayerManager>().AddPlayer(new ConsoleClient("Player1"), "Player1");
            //IPlayer player2 = DependencyContainer.Current.GetInstance<IPlayerManager>().AddPlayer(new ConsoleClient("Player2"), "Player2");
            //IAdmin admin = DependencyContainer.Current.GetInstance<IAdminManager>().AddAdmin(new ConsoleClient("Admin1"), "Admin1");

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

        private static void TestCommandParsing()
        {
            //// server doesn't need to be started, we are not testing real runtime but basic commands
            //IArea area = DependencyContainer.Current.GetInstance<IAreaManager>().AddArea(Guid.NewGuid(), new AreaBlueprint{Name = "testarea", Builders = "SinaC", Credits = "Credits"});
            //// Blueprints
            //RoomBlueprint room1Blueprint = new RoomBlueprint
            //{
            //    Id = 1,
            //    Name = "room1",
            //    Description = "My first room"
            //};
            //// World
            //IRoom room = DependencyContainer.Current.GetInstance<IRoomManager>().AddRoom(Guid.NewGuid(), room1Blueprint, area);

            //IPlayer player = DependencyContainer.Current.GetInstance<IPlayerManager>().AddPlayer(new ConsoleClient("Player"), "Player");
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

            //IPlayableCharacter character = DependencyContainer.Current.GetInstance<ICharacterManager>().AddPlayableCharacter(Guid.NewGuid(), new PlayableCharacterData
            //{
            //    Name = "toto",
            //    Class = DependencyContainer.Current.GetInstance<IClassManager>()["Mage"].Name,
            //    Race = DependencyContainer.Current.GetInstance<IRaceManager>()["Troll"].Name,
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

            //IAdmin admin = DependencyContainer.Current.GetInstance<IAdminManager>().AddAdmin(new ConsoleClient("Admin"), "Admin");
            //admin.ProcessInput("incarnate");
            //admin.ProcessInput("unknown"); // INVALID
        }

        private static void TestImport()
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

        private static void TestWorldOnline()
        {
            Console.WriteLine("Let's go");

            //CreateDummyWorld();
            CreateMidgaard();

            INetworkServer telnetServer = new TelnetServer(11000);
            DependencyContainer.Current.GetInstance<IServer>().Initialize(new List<INetworkServer> {telnetServer});
            DependencyContainer.Current.GetInstance<IServer>().Start();

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
                                foreach (IAdmin a in DependencyContainer.Current.GetInstance<IAdminManager>().Admins)
                                    Console.WriteLine(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
                            }
                            else if (line == "plist")
                            {
                                Console.WriteLine("players:");
                                foreach (IPlayer p in DependencyContainer.Current.GetInstance<IPlayerManager>().Players)
                                    Console.WriteLine(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
                            }

                            // TODO: characters/rooms/items
                        }
                    }
                }
                else
                    Thread.Sleep(100);
            }

            DependencyContainer.Current.GetInstance<IServer>().Stop();
        }

        private static void TestWorldOffline()
        {
            Console.WriteLine("Let's go");
            //CreateDummyWorld();

            ConsoleNetworkServer consoleNetworkServer = new ConsoleNetworkServer();
            consoleNetworkServer.Initialize();
            CreateMidgaard();
            DependencyContainer.Current.GetInstance<IServer>().Initialize(new List<INetworkServer> { consoleNetworkServer });
            consoleNetworkServer.AddClient("Player1", false, true);
            DependencyContainer.Current.GetInstance<IServer>().Start(); // this call is blocking because consoleNetworkServer.Start is blocking

            DependencyContainer.Current.GetInstance<IServer>().Stop();
        }
    }
}
