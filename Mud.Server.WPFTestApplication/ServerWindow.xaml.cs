﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AutoMapper;
using Mud.Container;
using Mud.Domain;
using Mud.Importer.Rom;
using Mud.Logger;
using Mud.Network;
using Mud.Network.Telnet;
using Mud.Repository;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Server;
using Mud.Settings;

namespace Mud.Server.WPFTestApplication
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window, INetworkServer
    {
        private static ServerWindow _serverWindowInstance;

        private static IServer Server => DependencyContainer.Current.GetInstance<IServer>();
        private static IServerAdminCommand ServerPlayerCommand => DependencyContainer.Current.GetInstance<IServerAdminCommand>();
        private static IPlayerManager PlayerManager => DependencyContainer.Current.GetInstance<IPlayerManager>();
        private static IAdminManager AdminManager => DependencyContainer.Current.GetInstance<IAdminManager>();
        private static IWorld World => DependencyContainer.Current.GetInstance<IWorld>();

        public ServerWindow()
        {
            _serverWindowInstance = this;

            // Initialize settings
            ISettings settings = new Settings.ConfigurationManager.Settings();
            DependencyContainer.Current.RegisterInstance(settings);

            // Initialize log
            Log.Default.Initialize(settings.LogPath, "server.log");

            // Initialize IOC container
            DependencyContainer.Current.Register<ITimeManager, TimeManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IWorld, World.World>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IServer, Server.Server>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IWiznet, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IWiznet
            DependencyContainer.Current.Register<IPlayerManager, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IPlayerManager
            DependencyContainer.Current.Register<IAdminManager, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IAdminManager
            DependencyContainer.Current.Register<IServerAdminCommand, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IServerAdminCommand
            DependencyContainer.Current.Register<IServerPlayerCommand, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IServerPlayerCommand
            DependencyContainer.Current.Register<IAbilityManager, Abilities.AbilityManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IClassManager, Classes.ClassManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IRaceManager, Races.RaceManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IUniquenessManager, Server.UniquenessManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.RegisterInstance<IRandomManager>(new RandomManager()); // 2 ctors => injector cant choose which one to chose
            DependencyContainer.Current.Register<ITableValues, Tables.TableValues>(SimpleInjector.Lifestyle.Singleton);

            if (settings.UseMongo)
            {
                DependencyContainer.Current.Register<ILoginRepository, Repository.Mongo.LoginRepository>(SimpleInjector.Lifestyle.Singleton);
                DependencyContainer.Current.Register<IPlayerRepository, Repository.Mongo.PlayerRepository>(SimpleInjector.Lifestyle.Singleton);
                DependencyContainer.Current.Register<IAdminRepository, Repository.Mongo.AdminRepository>(SimpleInjector.Lifestyle.Singleton);
            }
            else 
            {
                DependencyContainer.Current.Register<ILoginRepository, Repository.Filesystem.LoginRepository>(SimpleInjector.Lifestyle.Singleton);
                DependencyContainer.Current.Register<IPlayerRepository, Repository.Filesystem.PlayerRepository>(SimpleInjector.Lifestyle.Singleton);
                DependencyContainer.Current.Register<IAdminRepository, Repository.Filesystem.AdminRepository>(SimpleInjector.Lifestyle.Singleton);
            }

            // Initialize mapping
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;

                cfg.AddProfile<Repository.Filesystem.AutoMapperProfile>();
                cfg.AddProfile<Repository.Mongo.AutoMapperProfile>();
            });
            DependencyContainer.Current.RegisterInstance(mapperConfiguration.CreateMapper());

            //
            TestLootTable();

            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void TestLootTable()
        {
            TreasureTable<int> tableSpider = new TreasureTable<int>
            {
                Name = "TreasureList_Spider",
                Entries = new List<TreasureTableEntry<int>>
                {
                    new TreasureTableEntry<int>
                    {
                        Value = 1, // Spider venom
                        Occurancy = 25,
                        MaxOccurancy = 1
                    },
                    new TreasureTableEntry<int>
                    {
                        Value = 2, // Spider webbing
                        Occurancy = 65,
                        MaxOccurancy = 1
                    },
                    new TreasureTableEntry<int>
                    {
                        Value = 3, // Severed spider leg
                        Occurancy = 10,
                        MaxOccurancy = 1
                    }
                }
            };
            TreasureTable<int> tableRareLoot = new TreasureTable<int>
            {
                Name = "TreasureList_RareLoot",
                Entries = new List<TreasureTableEntry<int>>
                {
                    new TreasureTableEntry<int>
                    {
                        Value = 4, // Ubber-sword
                        Occurancy = 1,
                        MaxOccurancy = 1,
                    }
                }
            };
            TreasureTable<int> tableEmpty = new TreasureTable<int>
            {
                Name = "TreasureList_Empty",
            };
            CharacterLootTable<int> spiderTable = new CharacterLootTable<int>
            {
                MinLoot = 1,
                MaxLoot = 3,
                Entries = new List<CharacterLootTableEntry<int>>
                {
                    new CharacterLootTableEntry<int>
                    {
                        Value = tableSpider,
                        Occurancy = 45,
                        Max = 2
                    },
                    new CharacterLootTableEntry<int>
                    {
                        Value = tableRareLoot,
                        Occurancy = 5,
                        Max = 1
                    },
                    new CharacterLootTableEntry<int>
                    {
                        Value = tableEmpty,
                        Occurancy = 50,
                        Max = 1
                    }
                },
                //AlwaysDrop = new List<int>
                //{
                //    99
                //}
            };
            for (int i = 0; i < 10; i++)
            {
                List<int> loots = spiderTable.GenerateLoots();
                Log.Default.WriteLine(LogLevels.Info, "***LOOT {0}: {1}", i, string.Join(",", loots));
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            try
            {
                CreateWorld();
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "*** Fatal error. Stopping application ***");
                Log.Default.WriteLine(LogLevels.Error, ex.ToString()); //Fatal exception -> stop application
                Application.Current.Shutdown(0);
                return;
            }

            //
            INetworkServer telnetServer = new TelnetServer(DependencyContainer.Current.GetInstance<ISettings>().TelnetPort);
            Server.Initialize(new List<INetworkServer> { telnetServer, this });
            Server.Start();

            //CreateNewClientWindow();
            InputTextBox.Focus();
        }

        private void InputTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SendButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); // http://stackoverflow.com/questions/728432/how-to-programmatically-click-a-button-in-wpf
            else if (e.Key == Key.N && Keyboard.Modifiers != ModifierKeys.None) // Alt+N, Ctrl+N
                CreateNewClientWindow();
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text.ToLower();
            if (input == "exit" || input == "quit" || input == "stop")
            {
                Server.Stop();
                Application.Current.Shutdown();
            }
            else if (input == "alist")
            {
                OutputText("Admins:");
                foreach (IAdmin a in AdminManager.Admins)
                    OutputText(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
            }
            else if (input == "plist")
            {
                OutputText("players:");
                foreach (IPlayer p in PlayerManager.Players)
                    OutputText(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
            }
            else if (input == "dump")
            {
                Server.Dump();
            }
            else if (input.StartsWith("promote"))
            {
                string[] tokens = input.Split(' ');
                if (tokens.Length <= 1)
                {
                    OutputText("A player name must be specified");
                }
                else
                {
                    string playerName = tokens[1];
                    if (AdminManager.Admins.Any(x => x.Name == playerName))
                    {
                        OutputText("Already admin");
                    }
                    else
                    {
                        IPlayer player = PlayerManager.Players.FirstOrDefault(x => x.Name == playerName);
                        if (player == null)
                        {
                            OutputText("No such player");
                        }
                        else
                        {
                            AdminLevels level;
                            if (tokens.Length >= 2)
                            {
                                if (!EnumHelpers.TryFindByName(tokens[2], out level))
                                    level = AdminLevels.Angel;
                            }
                            else
                                level = AdminLevels.Angel;
                            OutputText($"Promoting {player.Name} to {level}");
                            ServerPlayerCommand.Promote(player, level);
                        }
                    }
                }
            }
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        }

        private void NewClientButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreateNewClientWindow();
        }

        private void CreateNewClientWindow()
        {
            ClientWindow window = new ClientWindow
            {
                ColorAccepted = true
            };
            NewClientConnected?.Invoke(window);
            window.Closed += (sender, args) =>
            {
                if (window.IsConnected)
                    ClientDisconnected?.Invoke(window);
            };
            // Display client window
            window.Show();
        }

        public event NewClientConnectedEventHandler NewClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;

        public void Initialize()
        {
            // NOP
        }

        public void Start()
        {
            // NOP
        }

        public void Stop()
        {
            // NOP
        }

        // Don't remove, used from nlog.config
        public static void LogMethod(string level, string message)
        {
            _serverWindowInstance.Dispatcher.InvokeAsync(() =>
            {
                //_serverWindowInstance.OutputRichTextBox.AppendText(message+Environment.NewLine);
                Brush color;
                if (level == "Error")
                    color = Brushes.Red;
                else if (level == "Warn")
                    color = Brushes.Yellow;
                else if (level == "Info")
                    color = Brushes.White;
                else if (level == "Debug")
                    color = Brushes.LightGray;
                else if (level == "Trace")
                    color = Brushes.DarkGray;
                else
                    color = Brushes.Orchid; // should never happen
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(level + ": " + message)
                {
                    Foreground = color
                });
                _serverWindowInstance.OutputRichTextBox.Document.Blocks.Add(paragraph);
                _serverWindowInstance.OutputScrollViewer.ScrollToBottom();
            });
        }

        private void OutputText(string text)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(text);
            _serverWindowInstance.OutputRichTextBox.Document.Blocks.Add(paragraph);
        }

        private static void CreateWorld()
        {
            string path = DependencyContainer.Current.GetInstance<ISettings>().ImportAreaPath;

            RomImporter importer = new RomImporter();
            //importer.Import(path, "limbo.are", "midgaard.are", "hitower.are");
            importer.ImportByList(path, "area.lst");

            // Area
            foreach (AreaBlueprint blueprint in importer.Areas)
            {
                World.AddAreaBlueprint(blueprint);
                World.AddArea(Guid.NewGuid(), blueprint);
            }

            // Rooms
            foreach (RoomBlueprint blueprint in importer.Rooms)
            {
                World.AddRoomBlueprint(blueprint);
                IArea area = World.Areas.FirstOrDefault(x => x.Blueprint.Id == blueprint.AreaId);
                if (area == null)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Area id {0} not found", blueprint.AreaId);
                }
                else
                    World.AddRoom(Guid.NewGuid(), blueprint, area);
            }

            foreach (IRoom room in World.Rooms)
            {
                foreach(ExitBlueprint exitBlueprint in room.Blueprint.Exits.Where(x => x != null))
                {
                    IRoom to = World.Rooms.FirstOrDefault(x => x.Blueprint.Id == exitBlueprint.Destination);
                    if (to == null)
                        Log.Default.WriteLine(LogLevels.Warning, "Destination room {0} not found for room {1} direction {2}", exitBlueprint.Destination, room.Blueprint.Id, exitBlueprint.Direction);
                    else
                        World.AddExit(room, to, exitBlueprint, exitBlueprint.Direction);
                }
            }

            // Characters
            foreach (CharacterBlueprintBase blueprint in importer.Characters)
                World.AddCharacterBlueprint(blueprint);

            // Items
            foreach(ItemBlueprintBase blueprint in importer.Items)
                World.AddItemBlueprint(blueprint);

            // Custom blueprint to test
            ItemQuestBlueprint questItem1Blueprint = new ItemQuestBlueprint
            {
                Id = 80000,
                Name = "Quest item 1",
                ShortDescription = "Quest item 1",
                Description = "The quest item 1 has been left here."
            };
            World.AddItemBlueprint(questItem1Blueprint);
            ItemQuestBlueprint questItem2Blueprint = new ItemQuestBlueprint
            {
                Id = 90000,
                Name = "Quest item 2",
                ShortDescription = "Quest item 2",
                Description = "The quest item 2 has been left here."
            };
            World.AddItemBlueprint(questItem2Blueprint);

            // MANDATORY ITEM
            if (World.GetItemBlueprint(DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId) == null)
            {
                ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
                {
                    Id = DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId,
                    NoTake = true,
                    Name = "corpse"
                };
                World.AddItemBlueprint(corpseBlueprint);
            }
            // MANDATORY ROOM
            RoomBlueprint voidBlueprint = World.GetRoomBlueprint(DependencyContainer.Current.GetInstance<ISettings>().NullRoomId);
            if (voidBlueprint == null)
            {
                IArea area = World.Areas.First();
                Log.Default.WriteLine(LogLevels.Error, "NullRoom not found -> creation of null room with id {0} in area {1}", DependencyContainer.Current.GetInstance<ISettings>().NullRoomId, area.DisplayName);
                voidBlueprint = new RoomBlueprint
                {
                    Id = DependencyContainer.Current.GetInstance<ISettings>().NullRoomId,
                    Name = "The void",
                    RoomFlags = RoomFlags.ImpOnly | RoomFlags.NoRecall | RoomFlags.NoScan | RoomFlags.NoWhere | RoomFlags.Private
                };
                World.AddRoomBlueprint(voidBlueprint);
                World.AddRoom(Guid.NewGuid(), voidBlueprint, area);
            }

            // Add dummy mobs and items to allow impersonate :)
            IRoom templeOfMota = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
            IRoom templeSquare = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple square");
            IRoom marketSquare = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "market square");
            IRoom commonSquare = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "the common square");

            World.AddItem(Guid.NewGuid(), questItem2Blueprint, templeSquare); // TODO: this should be added dynamically when player takes the quest

            // Quest
            QuestKillLootTable<int> quest1KillLoot = new QuestKillLootTable<int>
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
                KillObjectives = new QuestKillObjectiveBlueprint[]
                {
                    new QuestKillObjectiveBlueprint
                    {
                        Id = 0,
                        CharacterBlueprintId = 3062, // fido
                        Count = 3
                    }
                },
                ItemObjectives = new QuestItemObjectiveBlueprint[]
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
                LocationObjectives = new QuestLocationObjectiveBlueprint[]
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
            World.AddQuestBlueprint(questBlueprint1);

            QuestBlueprint questBlueprint2 = new QuestBlueprint
            {
                Id = 2,
                Title = "Simple exploration quest",
                Description = "Explore temple of mota, temple square, market square and common square",
                Level = 10,
                Experience = 10000,
                Gold = 20,
                TimeLimit = 5,
                LocationObjectives = new QuestLocationObjectiveBlueprint[]
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
            World.AddQuestBlueprint(questBlueprint2);

            CharacterQuestorBlueprint mob10Blueprint = new CharacterQuestorBlueprint
            {
                Id = 10,
                Name = "mob10 questor",
                ShortDescription = "Tenth mob (neutral questor)",
                Description = "Tenth mob (neutral questor) is here",
                Sex = Sex.Neutral,
                Level = 60,
                QuestBlueprints = new QuestBlueprint[]
                {
                    questBlueprint1,
                    questBlueprint2
                }
            };
            World.AddCharacterBlueprint(mob10Blueprint);
            ICharacter mob10 = World.AddNonPlayableCharacter(Guid.NewGuid(), mob10Blueprint, commonSquare);
        }
    }
}
