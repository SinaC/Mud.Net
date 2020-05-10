using System;
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
using Mud.Logger;
using Mud.Network;
using Mud.Network.Telnet;
using Mud.Repository;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Item;
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
            DependencyContainer.Current.RegisterInstance<ISettings>(settings);

            // Initialize log
            Log.Default.Initialize(settings.LogPath, "server.log");

            // Initialize IOC container
            DependencyContainer.Current.Register<IWorld, World.World>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IServer, Server.Server>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<ITimeHandler, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements ITimeHandler
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
            if (input == "exit" || input == "quit")
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

        private static RoomBlueprint CreateRoomBlueprint(Importer.Mystery.RoomData data)
        {
            RoomBlueprint blueprint = new RoomBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                ExtraDescriptions = RoomBlueprint.BuildExtraDescriptions(data.ExtraDescr)
                // Exits will be done when each room blueprint is created
            };
            World.AddRoomBlueprint(blueprint);
            return blueprint;
        }

        private static RoomBlueprint CreateRoomBlueprint(Importer.Rom.RoomData data)
        {
            RoomBlueprint blueprint = new RoomBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                ExtraDescriptions = RoomBlueprint.BuildExtraDescriptions(data.ExtraDescr)
                // Exits will be done when each room blueprint is created
            };
            World.AddRoomBlueprint(blueprint);
            return blueprint;
        }

        private static ItemBlueprintBase CreateItemBlueprint(Importer.Mystery.ObjectData data)
        {
            (ItemFlags itemFlags, bool noTake) extraFlags = ConvertMysteryItemExtraFlags(data);
            ItemBlueprintBase blueprint;
            if (data.ItemType == "weapon")
            {
                (SchoolTypes damageType, WeaponFlags weaponFlags) weaponInfo = ConvertWeaponDamageType(data.Values[3], data.Values[4]);
                blueprint = new ItemWeaponBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    Type = ConvertWeaponType(data.Values[0]),
                    DiceCount = Convert.ToInt32(data.Values[1]),
                    DiceValue = Convert.ToInt32(data.Values[2]),
                    // Values[3] slash/pierce/bash/... attack_table in const.C
                    // Values[4] flaming/sharp/... weapon_type2 in tables.C
                    DamageType = weaponInfo.damageType,
                    Flags = weaponInfo.weaponFlags,
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "container")
            {
                blueprint = new ItemContainerBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    ItemCount = Convert.ToInt32(data.Values[3]),
                    WeightMultiplier = Convert.ToInt32(data.Values[4]),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "armor")
            {
                WearLocations wearLocations = ConvertWearLocation(data);
                if (wearLocations == WearLocations.Shield)
                    blueprint = new ItemShieldBlueprint
                    {
                        Id = data.VNum,
                        Name = data.Name,
                        ShortDescription = data.ShortDescr,
                        Description = data.Description,
                        ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                        Cost = Convert.ToInt32(data.Cost),
                        Level = data.Level,
                        Weight = data.Weight,
                        WearLocation = ConvertWearLocation(data),
                        Armor = Convert.ToInt32(data.Values[0]) + Convert.ToInt32(data.Values[1]) + Convert.ToInt32(data.Values[2]) + Convert.ToInt32(data.Values[3]), // TODO
                        ItemFlags = extraFlags.itemFlags,
                        NoTake = extraFlags.noTake,
                    };
                else
                    blueprint = new ItemArmorBlueprint
                    {
                        Id = data.VNum,
                        Name = data.Name,
                        ShortDescription = data.ShortDescr,
                        Description = data.Description,
                        ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                        Cost = Convert.ToInt32(data.Cost),
                        Level = data.Level,
                        Weight = data.Weight,
                        WearLocation = ConvertWearLocation(data),
                        Pierce = Convert.ToInt32(data.Values[0]),
                        Bash = Convert.ToInt32(data.Values[1]),
                        Slash = Convert.ToInt32(data.Values[2]),
                        Exotic = Convert.ToInt32(data.Values[3]),
                        ItemFlags = extraFlags.itemFlags,
                        NoTake = extraFlags.noTake,
                    };
            }
            else if (data.ItemType == "light")
            {
                blueprint = new ItemLightBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    DurationHours = Convert.ToInt32(data.Values[2]),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "furniture")
            {
                blueprint = new ItemFurnitureBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    MaxPeople = Convert.ToInt32(data.Values[0]),
                    MaxWeight = Convert.ToInt32(data.Values[1]),
                    FurnitureActions = ConvertFurnitureActions(data.Values[2]),
                    FurniturePlacePreposition = ConvertFurniturePreposition(data.Values[2]),
                    HealBonus = Convert.ToInt32(data.Values[3]),
                    ResourceBonus = Convert.ToInt32(data.Values[4]),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "drink")
            {
                blueprint = new ItemDrinkContainerBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    MaxLiquidAmount = Convert.ToInt32(data.Values[0]),
                    CurrentLiquidAmount = Convert.ToInt32(data.Values[1]),
                    LiquidType = data.Values[2].ToString(),
                    IsPoisoned = Convert.ToInt32(data.Values[3]) != 0,
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "food")
            {
                blueprint = new ItemFoodBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    FullHours = Convert.ToInt32(data.Values[0]),
                    HungerHours = Convert.ToInt32(data.Values[1]),
                    IsPoisoned = Convert.ToInt32(data.Values[3]) != 0,
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "fountain")
            {
                blueprint = new ItemFountainBlueprint // TODO: fountain
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    LiquidType = data.Values[2].ToString(),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "jewelry" || data.ItemType == "treasure")
            {
                blueprint = new ItemJewelryBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "key")
            {
                blueprint = new ItemKeyBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "portal")
            {
                blueprint = new ItemPortalBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    Destination = Convert.ToInt32(data.Values[3]) <= 0 ? ItemPortal.NoDestinationRoomId : Convert.ToInt32(data.Values[3]),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "warp_stone")
            {
                blueprint = new ItemWarpstoneBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    ItemFlags = extraFlags.itemFlags,
                    NoTake = extraFlags.noTake,
                };
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, $"ItemBlueprint cannot be created: [{data.VNum}] [{data.ItemType}] [{data.WearFlags}] : {data.Name}");
                // TODO: other item type
                blueprint = null;
            }
            if (blueprint != null)
                World.AddItemBlueprint(blueprint);
            return blueprint;
        }

        private static ItemBlueprintBase CreateItemBlueprint(Importer.Rom.ObjectData data)
        {
            ItemBlueprintBase blueprint;
            if (data.ItemType == "weapon")
            {
                blueprint = new ItemWeaponBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    // TODO: weapon type Values[0]
                    DiceCount = Convert.ToInt32(data.Values[1]),
                    DiceValue = Convert.ToInt32(data.Values[2]),
                    // TODO: damage type Values[3]
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else if (data.ItemType == "container")
            {
                blueprint = new ItemContainerBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    ItemCount = Convert.ToInt32(data.Values[3]),
                    WeightMultiplier = Convert.ToInt32(data.Values[4]),
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else if (data.ItemType == "armor")
            {
                blueprint = new ItemArmorBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    Pierce = Convert.ToInt32(data.Values[0]),
                    Bash = Convert.ToInt32(data.Values[1]),
                    Slash = Convert.ToInt32(data.Values[2]),
                    Exotic = Convert.ToInt32(data.Values[3]),
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else if (data.ItemType == "light")
            {
                blueprint = new ItemLightBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    DurationHours = Convert.ToInt32(data.Values[2]),
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else if (data.ItemType == "furniture")
            {
                blueprint = new ItemFurnitureBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    MaxPeople = Convert.ToInt32(data.Values[0]),
                    MaxWeight = Convert.ToInt32(data.Values[1]),
                    FurnitureActions = ConvertFurnitureActions(data.Values[2]),
                    FurniturePlacePreposition = ConvertFurniturePreposition(data.Values[2]),
                    HealBonus = Convert.ToInt32(data.Values[3]),
                    ResourceBonus = Convert.ToInt32(data.Values[4]),
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else if (data.ItemType == "drink")
            {
                blueprint = new ItemDrinkContainerBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    MaxLiquidAmount = Convert.ToInt32(data.Values[0]),
                    CurrentLiquidAmount = Convert.ToInt32(data.Values[1]),
                    LiquidType = data.Values[2].ToString(),
                    IsPoisoned = Convert.ToInt32(data.Values[3]) != 0,
                    //ItemFlags = extraFlags.itemFlags,
                    //NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "fountain")
            {
                blueprint = new ItemFountainBlueprint // TODO: fountain
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Level = data.Level,
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    LiquidType = data.Values[2].ToString(),
                    //ItemFlags = extraFlags.itemFlags,
                    //NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "jewelry" || data.ItemType == "treasure")
            {
                blueprint = new ItemJewelryBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else if (data.ItemType == "key")
            {
                blueprint = new ItemKeyBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else if (data.ItemType == "portal")
            {
                blueprint = new ItemPortalBlueprint
                {
                    Id = data.VNum,
                    Name = data.Name,
                    ShortDescription = data.ShortDescr,
                    Description = data.Description,
                    ExtraDescriptions = ItemBlueprintBase.BuildExtraDescriptions(data.ExtraDescr),
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    Destination = Convert.ToInt32(data.Values[3]),
                    ItemFlags = ConvertRomItemExtraFlags(data)
                };
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, $"ItemBlueprint cannot be created: [{data.VNum}] [{data.ItemType}] [{data.WearFlags}] : {data.Name}");
                // TODO: other item type
                blueprint = null;
            }
            if (blueprint != null)
                World.AddItemBlueprint(blueprint);
            return blueprint;
        }

        private static CharacterNormalBlueprint CreateCharacterBlueprint(Importer.Mystery.MobileData data)
        {
            Sex sex = Sex.Neutral;
            if (data.Sex.ToLower() == "female")
                sex = Sex.Female;
            else if (data.Sex.ToLower() == "male")
                sex = Sex.Male;
            CharacterNormalBlueprint blueprint = new CharacterNormalBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                Level = data.Level,
                LongDescription = data.LongDescr,
                ShortDescription = data.ShortDescr,
                Sex = sex,
                Alignment = data.Alignment,
                Immunities = ConvertMysteryIRV(data.ImmFlags),
                Resistances = ConvertMysteryIRV(data.ResFlags),
                Vulnerabilities = ConvertMysteryIRV(data.VulnFlags),
                CharacterFlags = ConvertMysteryCharacterFlags(data.AffectedBy, data.AffectedBy2),
                ActFlags = ConvertMysteryActFlags(data.Act),
                OffensiveFlags = ConvertMysteryOffensiveFlags(data.OffFlags),
                // TODO: CharacterFlags, Immunities, Resistances, Vulnerabilities, ...
            };
            World.AddCharacterBlueprint(blueprint);
            return blueprint;
        }

        private static CharacterNormalBlueprint CreateCharacterBlueprint(Importer.Rom.MobileData data)
        {
            Sex sex = Sex.Neutral;
            if (data.Sex.ToLower() == "female")
                sex = Sex.Female;
            else if (data.Sex.ToLower() == "male")
                sex = Sex.Male;
            CharacterNormalBlueprint blueprint = new CharacterNormalBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                Level = data.Level,
                LongDescription = data.LongDescr,
                ShortDescription = data.ShortDescr,
                Sex = sex,
            };
            World.AddCharacterBlueprint(blueprint);
            return blueprint;
        }

        private long ConvertToLong(char c)
        {
            return (long)1 << (c - 48);
        }

        private static bool HasBit(long data, long bit) => (data & bit) == bit;
        
        private static ExitFlags ConvertExitInfo(long exitInfo)
        {
            ExitFlags flags = 0;
            if ((exitInfo & Importer.Mystery.MysteryImporter.A) == Importer.Mystery.MysteryImporter.A)
                flags |= ExitFlags.Door;
            if ((exitInfo & Importer.Mystery.MysteryImporter.B) == Importer.Mystery.MysteryImporter.B)
                flags |= ExitFlags.Closed;
            if ((exitInfo & Importer.Mystery.MysteryImporter.B) == Importer.Mystery.MysteryImporter.B)
                flags |= ExitFlags.Locked;
            if ((exitInfo & Importer.Mystery.MysteryImporter.H) == Importer.Mystery.MysteryImporter.H)
                flags |= ExitFlags.Easy;
            if ((exitInfo & Importer.Mystery.MysteryImporter.I) == Importer.Mystery.MysteryImporter.I)
                flags |= ExitFlags.Hard;
            if ((exitInfo & Importer.Mystery.MysteryImporter.M) == Importer.Mystery.MysteryImporter.M)
                flags |= ExitFlags.Hidden;
            return flags;
        }

        private static IRVFlags ConvertMysteryIRV(long value)
        {
            IRVFlags flags = 0;
            if ((value & Importer.Mystery.MysteryImporter.A) == Importer.Mystery.MysteryImporter.A) flags |= IRVFlags.Summon;
            if ((value & Importer.Mystery.MysteryImporter.B) == Importer.Mystery.MysteryImporter.B) flags |= IRVFlags.Charm;
            if ((value & Importer.Mystery.MysteryImporter.C) == Importer.Mystery.MysteryImporter.C) flags |= IRVFlags.Magic;
            if ((value & Importer.Mystery.MysteryImporter.D) == Importer.Mystery.MysteryImporter.D) flags |= IRVFlags.Weapon;
            if ((value & Importer.Mystery.MysteryImporter.E) == Importer.Mystery.MysteryImporter.E) flags |= IRVFlags.Bash;
            if ((value & Importer.Mystery.MysteryImporter.F) == Importer.Mystery.MysteryImporter.F) flags |= IRVFlags.Pierce;
            if ((value & Importer.Mystery.MysteryImporter.G) == Importer.Mystery.MysteryImporter.G) flags |= IRVFlags.Slash;
            if ((value & Importer.Mystery.MysteryImporter.H) == Importer.Mystery.MysteryImporter.H) flags |= IRVFlags.Fire;
            if ((value & Importer.Mystery.MysteryImporter.I) == Importer.Mystery.MysteryImporter.I) flags |= IRVFlags.Cold;
            if ((value & Importer.Mystery.MysteryImporter.J) == Importer.Mystery.MysteryImporter.J) flags |= IRVFlags.Lightning;
            if ((value & Importer.Mystery.MysteryImporter.K) == Importer.Mystery.MysteryImporter.K) flags |= IRVFlags.Acid;
            if ((value & Importer.Mystery.MysteryImporter.L) == Importer.Mystery.MysteryImporter.L) flags |= IRVFlags.Poison;
            if ((value & Importer.Mystery.MysteryImporter.M) == Importer.Mystery.MysteryImporter.M) flags |= IRVFlags.Negative;
            if ((value & Importer.Mystery.MysteryImporter.N) == Importer.Mystery.MysteryImporter.N) flags |= IRVFlags.Holy;
            if ((value & Importer.Mystery.MysteryImporter.O) == Importer.Mystery.MysteryImporter.O) flags |= IRVFlags.Energy;
            if ((value & Importer.Mystery.MysteryImporter.P) == Importer.Mystery.MysteryImporter.P) flags |= IRVFlags.Mental;
            if ((value & Importer.Mystery.MysteryImporter.Q) == Importer.Mystery.MysteryImporter.Q) flags |= IRVFlags.Disease;
            if ((value & Importer.Mystery.MysteryImporter.R) == Importer.Mystery.MysteryImporter.R) flags |= IRVFlags.Drowning;
            if ((value & Importer.Mystery.MysteryImporter.S) == Importer.Mystery.MysteryImporter.S) flags |= IRVFlags.Light;
            if ((value & Importer.Mystery.MysteryImporter.T) == Importer.Mystery.MysteryImporter.T) flags |= IRVFlags.Sound;
            // U 
            // V PARALYSIS
            if ((value & Importer.Mystery.MysteryImporter.X) == Importer.Mystery.MysteryImporter.X) flags |= IRVFlags.Wood;
            if ((value & Importer.Mystery.MysteryImporter.Y) == Importer.Mystery.MysteryImporter.Y) flags |= IRVFlags.Silver;
            if ((value & Importer.Mystery.MysteryImporter.Z) == Importer.Mystery.MysteryImporter.Z) flags |= IRVFlags.Iron;

            return flags;
        }

        private static CharacterFlags ConvertMysteryCharacterFlags(long affectedBy, long affectedBy2)
        {
            CharacterFlags flags = CharacterFlags.None;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_BLIND)) flags |= CharacterFlags.Blind;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_INVISIBLE)) flags |= CharacterFlags.Invisible;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_DETECT_EVIL)) flags |= CharacterFlags.DetectEvil;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_DETECT_INVIS)) flags |= CharacterFlags.DetectInvis;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_DETECT_MAGIC)) flags |= CharacterFlags.DetectMagic;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_DETECT_HIDDEN)) flags |= CharacterFlags.DetectHidden;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_DETECT_GOOD)) flags |= CharacterFlags.DetectGood;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_SANCTUARY)) flags |= CharacterFlags.Sanctuary;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_FAERIE_FIRE)) flags |= CharacterFlags.FaerieFire;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_INFRARED)) flags |= CharacterFlags.Infrared;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_CURSE)) flags |= CharacterFlags.Curse;
            //if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_ROOTED)) flags |= CharacterFlags.
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_POISON)) flags |= CharacterFlags.Poison;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_PROTECT_EVIL)) flags |= CharacterFlags.ProtectEvil;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_PROTECT_GOOD)) flags |= CharacterFlags.ProtectGood;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_SNEAK)) flags |= CharacterFlags.Sneak;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_HIDE)) flags |= CharacterFlags.Hide;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_SLEEP)) flags |= CharacterFlags.Sleep;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_CHARM)) flags |= CharacterFlags.Charm;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_FLYING)) flags |= CharacterFlags.Flying;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_PASS_DOOR)) flags |= CharacterFlags.PassDoor;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_HASTE)) flags |= CharacterFlags.Haste;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_CALM)) flags |= CharacterFlags.Calm;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_PLAGUE)) flags |= CharacterFlags.Plague;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_WEAKEN)) flags |= CharacterFlags.Weaken;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_DARK_VISION)) flags |= CharacterFlags.DarkVision;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_BERSERK)) flags |= CharacterFlags.Berserk;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_SWIM)) flags |= CharacterFlags.Swim;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_REGENERATION)) flags |= CharacterFlags.Regeneration;
            if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_SLOW)) flags |= CharacterFlags.Slow;
            //if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_SILENCE)) flags |= CharacterFlags.

            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_WALK_ON_WATER      (A)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_WATER_BREATH       (B)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_DETECT_EXITS       (C)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_MAGIC_MIRROR       (D)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_FAERIE_FOG         (E)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_NOEQUIPMENT        (F)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_FREE_MOVEMENT      (G)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_INCREASED_CASTING  (H)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_NOSPELL            (I)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_NECROTISM          (J)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_HIGHER_MAGIC_ATTRIBUTES (K)
            //if (HasBit(affectedBy2, (long)Importer.Mystery.AffectedBy2.AFF2_CONFUSION          (L)
            return flags;
        }

        private static ActFlags ConvertMysteryActFlags(long act)
        {
            ActFlags flags = ActFlags.None;

            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_IS_NPC)) flags |= ActFlags.
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_SENTINEL)) flags |= ActFlags.Sentinel;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_SCAVENGER)) flags |= ActFlags.Scavenger;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_AWARE)) flags |= ActFlags.Aware;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_AGGRESSIVE)) flags |= ActFlags.Aggressive;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_STAY_AREA)) flags |= ActFlags.StayArea;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_WIMPY)) flags |= ActFlags.Wimpy;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_PET)) flags |= ActFlags.Pet;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_TRAIN)) flags |= ActFlags.Train;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_PRACTICE)) flags |= ActFlags.Practice;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_FREE_WANDER
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_MOUNTABLE
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_IS_MOUNTED
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_UNDEAD)) flags |= ActFlags.Undead;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_NOSLEEP))
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_CLERIC
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_MAGE = MysteryImporter.R,
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_THIEF = MysteryImporter.S,
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_WARRIOR = MysteryImporter.T,
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_NOALIGN)) flags |= ActFlags.Noalign;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_NOPURGE)) flags |= ActFlags.Nopurge;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_OUTDOORS)) flags |= ActFlags.Outdoors;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_INDOORS)) flags |= ActFlags.Indoors;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_CREATED
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_IS_HEALER)) flags |= ActFlags.IsHealer;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_GAIN)) flags |= ActFlags.Gain;
            if (HasBit(act, (long)Importer.Mystery.Act.ACT_UPDATE_ALWAYS)) flags |= ActFlags.UpdateAlways;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_RESERVED
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_IS_SAFE
            return flags;
        }

        private static OffensiveFlags ConvertMysteryOffensiveFlags(long off)
        {
            OffensiveFlags flags = OffensiveFlags.None;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_AREA_ATTACK)) flags |= OffensiveFlags.AreaAttack;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_BACKSTAB)) flags |= OffensiveFlags.Backstab;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_BASH)) flags |= OffensiveFlags.Bash;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_BERSERK)) flags |= OffensiveFlags.Berserk;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_DISARM)) flags |= OffensiveFlags.Disarm;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_DODGE)) flags |= OffensiveFlags.Dodge;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_FADE)) flags |= OffensiveFlags.Fade;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_FAST)) flags |= OffensiveFlags.Fast;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_KICK)) flags |= OffensiveFlags.Kick;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_KICK_DIRT)) flags |= OffensiveFlags.DirtKick;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_PARRY)) flags |= OffensiveFlags.Parry;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_RESCUE)) flags |= OffensiveFlags.Rescue;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_TAIL)) flags |= OffensiveFlags.Tail;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_TRIP)) flags |= OffensiveFlags.Trip;
            if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_CRUSH)) flags |= OffensiveFlags.Crush;
            //if (HasBit(off, (long)Importer.Mystery.Offensive.ASSIST_ALL))
            //if (HasBit(off, (long)Importer.Mystery.Offensive.ASSIST_ALIGN))
            //if (HasBit(off, (long)Importer.Mystery.Offensive.ASSIST_RACE = MysteryImporter.R,
            //if (HasBit(off, (long)Importer.Mystery.Offensive.ASSIST_PLAYERS = MysteryImporter.S,
            //if (HasBit(off, (long)Importer.Mystery.Offensive.ASSIST_GUARD = MysteryImporter.T,
            //if (HasBit(off, (long)Importer.Mystery.Offensive.ASSIST_VNUM = MysteryImporter.U,
            //if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_COUNTER = MysteryImporter.V,
            //if (HasBit(off, (long)Importer.Mystery.Offensive.OFF_BITE = MysteryImporter.W,
            return flags;
        }

        private static FurnitureActions ConvertFurnitureActions(object value)
        {
            FurnitureActions actions = FurnitureActions.None;

            int flag = value == null ? 0 : Convert.ToInt32(value);
            if ((flag & Importer.Mystery.MysteryImporter.A) == Importer.Mystery.MysteryImporter.A
                || (flag & Importer.Mystery.MysteryImporter.B) == Importer.Mystery.MysteryImporter.B
                || (flag & Importer.Mystery.MysteryImporter.C) == Importer.Mystery.MysteryImporter.C)
                actions |= FurnitureActions.Stand;
            if ((flag & Importer.Mystery.MysteryImporter.D) == Importer.Mystery.MysteryImporter.D
                || (flag & Importer.Mystery.MysteryImporter.E) == Importer.Mystery.MysteryImporter.E
                || (flag & Importer.Mystery.MysteryImporter.F) == Importer.Mystery.MysteryImporter.F)
                actions |= FurnitureActions.Sit;
            if ((flag & Importer.Mystery.MysteryImporter.G) == Importer.Mystery.MysteryImporter.G
                || (flag & Importer.Mystery.MysteryImporter.H) == Importer.Mystery.MysteryImporter.H
                || (flag & Importer.Mystery.MysteryImporter.I) == Importer.Mystery.MysteryImporter.I)
                actions |= FurnitureActions.Rest;
            if ((flag & Importer.Mystery.MysteryImporter.J) == Importer.Mystery.MysteryImporter.J
                || (flag & Importer.Mystery.MysteryImporter.K) == Importer.Mystery.MysteryImporter.K
                || (flag & Importer.Mystery.MysteryImporter.L) == Importer.Mystery.MysteryImporter.L)
                actions |= FurnitureActions.Sleep;
            return actions;
        }

        private static FurniturePlacePrepositions ConvertFurniturePreposition(object value)
        {
            FurniturePlacePrepositions preposition = FurniturePlacePrepositions.None;

            int flag = value == null ? 0 : Convert.ToInt32(value);
            if ((flag & Importer.Mystery.MysteryImporter.A) == Importer.Mystery.MysteryImporter.A
                || (flag & Importer.Mystery.MysteryImporter.D) == Importer.Mystery.MysteryImporter.D
                || (flag & Importer.Mystery.MysteryImporter.G) == Importer.Mystery.MysteryImporter.G
                || (flag & Importer.Mystery.MysteryImporter.J) == Importer.Mystery.MysteryImporter.J)
                preposition = FurniturePlacePrepositions.At;
            else if ((flag & Importer.Mystery.MysteryImporter.B) == Importer.Mystery.MysteryImporter.B
                || (flag & Importer.Mystery.MysteryImporter.E) == Importer.Mystery.MysteryImporter.E
                || (flag & Importer.Mystery.MysteryImporter.H) == Importer.Mystery.MysteryImporter.H
                || (flag & Importer.Mystery.MysteryImporter.K) == Importer.Mystery.MysteryImporter.K)
                preposition = FurniturePlacePrepositions.On;
            else if ((flag & Importer.Mystery.MysteryImporter.C) == Importer.Mystery.MysteryImporter.C
                || (flag & Importer.Mystery.MysteryImporter.F) == Importer.Mystery.MysteryImporter.F
                || (flag & Importer.Mystery.MysteryImporter.I) == Importer.Mystery.MysteryImporter.I
                || (flag & Importer.Mystery.MysteryImporter.L) == Importer.Mystery.MysteryImporter.L)
                preposition = FurniturePlacePrepositions.In;
            return preposition;
        }

        private static WearLocations ConvertWearLocation(Importer.Mystery.ObjectData data)
        {
            //#define ITEM_TAKE		(A)
            //#define ITEM_WEAR_FINGER	(B)
            //#define ITEM_WEAR_NECK		(C)
            //#define ITEM_WEAR_BODY		(D)
            //#define ITEM_WEAR_HEAD		(E)
            //#define ITEM_WEAR_LEGS		(F)
            //#define ITEM_WEAR_FEET		(G)
            //#define ITEM_WEAR_HANDS		(H)
            //#define ITEM_WEAR_ARMS		(I)
            //#define ITEM_WEAR_SHIELD	(J)
            //#define ITEM_WEAR_ABOUT		(K)
            //#define ITEM_WEAR_WAIST		(L)
            //#define ITEM_WEAR_WRIST		(M)
            //#define ITEM_WIELD		(N)
            //#define ITEM_HOLD		(O)
            //#define ITEM_WEAR_FLOAT		(Q)
            //#define ITEM_WEAR_EAR           (R)
            //#define ITEM_WEAR_EYES          (S)
            switch (data.WearFlags & ~1 /*remove TAKE*/)
            {
                case 0:
                    if (data.ItemType == "light")
                        return WearLocations.Light;
                    else
                        return WearLocations.None;
                case Importer.Mystery.MysteryImporter.B: // B finger
                    return WearLocations.Ring;
                case Importer.Mystery.MysteryImporter.C: // C neck
                    return WearLocations.Amulet;
                case Importer.Mystery.MysteryImporter.D: // D body
                    return WearLocations.Chest;
                case Importer.Mystery.MysteryImporter.E: // E head
                    return WearLocations.Head;
                case Importer.Mystery.MysteryImporter.F: // F legs
                    return WearLocations.Legs;
                case Importer.Mystery.MysteryImporter.G: // G feet
                    return WearLocations.Feet;
                case Importer.Mystery.MysteryImporter.H: // H hands
                    return WearLocations.Hands;
                case Importer.Mystery.MysteryImporter.I: // I arms
                    return WearLocations.Arms;
                case Importer.Mystery.MysteryImporter.J: // J shield
                    return WearLocations.Shield;
                case Importer.Mystery.MysteryImporter.K: // K about
                    return WearLocations.Cloak;
                case Importer.Mystery.MysteryImporter.L: // L waist
                    return WearLocations.Waist;
                case Importer.Mystery.MysteryImporter.M: // M wrist
                    return WearLocations.Wrists;
                case Importer.Mystery.MysteryImporter.N: // N wield
                    return WearLocations.Wield;
                case Importer.Mystery.MysteryImporter.O: // O hold
                    return WearLocations.Hold;
                //case Importer.Mystery.MysteryImporter.Q: // Q float
                //case Importer.Mystery.MysteryImporter.R: // R ear
                case Importer.Mystery.MysteryImporter.S: // S eyes
                    return WearLocations.Head; // eyes
                default:
                    return WearLocations.None;
            }
        }

        private static WearLocations ConvertWearLocation(Importer.Rom.ObjectData data)
        {
            //#define ITEM_TAKE		(A)
            //#define ITEM_WEAR_FINGER	(B)
            //#define ITEM_WEAR_NECK		(C)
            //#define ITEM_WEAR_BODY		(D)
            //#define ITEM_WEAR_HEAD		(E)
            //#define ITEM_WEAR_LEGS		(F)
            //#define ITEM_WEAR_FEET		(G)
            //#define ITEM_WEAR_HANDS		(H)
            //#define ITEM_WEAR_ARMS		(I)
            //#define ITEM_WEAR_SHIELD	(J)
            //#define ITEM_WEAR_ABOUT		(K)
            //#define ITEM_WEAR_WAIST		(L)
            //#define ITEM_WEAR_WRIST		(M)
            //#define ITEM_WIELD		(N)
            //#define ITEM_HOLD		(O)
            //#define ITEM_WEAR_FLOAT		(Q)
            //#define ITEM_WEAR_EAR           (R)
            //#define ITEM_WEAR_EYES          (S)
            switch (data.WearFlags & ~1 /*remove TAKE*/)
            {
                case 0:
                    if (data.ItemType == "light")
                        return WearLocations.Light;
                    else
                        return WearLocations.None;
                case Importer.Mystery.MysteryImporter.B: // B finger
                    return WearLocations.Ring;
                case Importer.Mystery.MysteryImporter.C: // C neck
                    return WearLocations.Amulet;
                case Importer.Mystery.MysteryImporter.D: // D body
                    return WearLocations.Chest;
                case Importer.Mystery.MysteryImporter.E: // E head
                    return WearLocations.Head;
                case Importer.Mystery.MysteryImporter.F: // F legs
                    return WearLocations.Legs;
                case Importer.Mystery.MysteryImporter.G: // G feet
                    return WearLocations.Feet;
                case Importer.Mystery.MysteryImporter.H: // H hands
                    return WearLocations.Hands;
                case Importer.Mystery.MysteryImporter.I: // I arms
                    return WearLocations.Arms;
                case Importer.Mystery.MysteryImporter.J: // J shield
                    return WearLocations.Shield;
                case Importer.Mystery.MysteryImporter.K: // K about
                    return WearLocations.Cloak;
                case Importer.Mystery.MysteryImporter.L: // L waist
                    return WearLocations.Waist;
                case Importer.Mystery.MysteryImporter.M: // M wrist
                    return WearLocations.Wrists;
                case Importer.Mystery.MysteryImporter.N: // N wield
                    return WearLocations.Wield;
                case Importer.Mystery.MysteryImporter.O: // O hold
                    return WearLocations.Hold;
                //case Importer.Mystery.MysteryImporter.Q: // Q float
                //case Importer.Mystery.MysteryImporter.R: // R ear
                case Importer.Mystery.MysteryImporter.S: // S eyes
                    return WearLocations.Head; // eyes
                default:
                    return WearLocations.None;
            }
        }

        private static WeaponTypes ConvertWeaponType(object value)
        {
            string weaponType = (string)value;
            switch (weaponType)
            {
                case "exotic": // Exotic
                    // TODO:
                    return WeaponTypes.Fist;
                case "sword": // Sword
                    return WeaponTypes.Sword1H;
                case "dagger": // Dagger
                    return WeaponTypes.Dagger;
                case "spear": // Spear
                    return WeaponTypes.Mace2H;
                case "mace": // Mace
                    return WeaponTypes.Mace1H;
                case "axe": // Axe
                    return WeaponTypes.Mace2H;
                case "flail": // Flail
                    // TODO:
                    return WeaponTypes.Fist;
                case "whip": // Whip
                    // TODO:
                    return WeaponTypes.Fist;
                case "polearm": // Polearm
                    // TODO:
                    return WeaponTypes.Polearm;
                case "staff(weapon)": // Staff
                    return WeaponTypes.Stave;
                case "arrow": // Arrow
                    // TODO:
                    return WeaponTypes.Fist;
                case "ranged": // Ranged
                    // TODO:
                    return WeaponTypes.Fist;
            }
            return WeaponTypes.Fist;
        }

        private static (SchoolTypes schoolType, WeaponFlags weaponFlags) ConvertWeaponDamageType(object attackTableValue, object weaponType2Value)
        {
            SchoolTypes schoolType = SchoolTypes.None;
            string attackTable = (string)attackTableValue;
            int weaponType2 = weaponType2Value == null ? 0 : Convert.ToInt32(weaponType2Value);
            if (attackTable == "acid") // Acid
                schoolType = SchoolTypes.Acid;
            if (attackTable == "wrath") // Wrath
                schoolType = SchoolTypes.Energy;
            if (attackTable == "magic") // Magic
                schoolType = SchoolTypes.Energy;
            if (attackTable == "divine") // Divine power
                schoolType = SchoolTypes.Holy;
            if (attackTable == "shbite") // Shocking bite
                schoolType = SchoolTypes.Lightning;
            if (attackTable == "flbite") // Flaming bite
                schoolType = SchoolTypes.Fire;
            if (attackTable == "frbite") // Frost bite
                schoolType = SchoolTypes.Cold;
            if (attackTable == "acbite") // Acidic bite
                schoolType = SchoolTypes.Acid;
            if (attackTable == "drain") // Life drain
                schoolType = SchoolTypes.Negative;
            if (attackTable == "slime") // Slime
                schoolType = SchoolTypes.Acid;
            if (attackTable == "shock") // Shock
                schoolType = SchoolTypes.Lightning;
            if (attackTable == "flame") // Flame
                schoolType = SchoolTypes.Fire;
            if (attackTable == "chill") // Chill
                schoolType = SchoolTypes.Cold;

            WeaponFlags weaponFlags = WeaponFlags.None;
            // originally a flag but converted to a single value
            if ((weaponType2 & Importer.Mystery.MysteryImporter.A) == Importer.Mystery.MysteryImporter.A) // Flaming
                weaponFlags |= WeaponFlags.Flaming;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.B) == Importer.Mystery.MysteryImporter.B) // Frost
                weaponFlags |= WeaponFlags.Frost;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.C) == Importer.Mystery.MysteryImporter.C) // Vampiric
                weaponFlags |= WeaponFlags.Vampiric;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.D) == Importer.Mystery.MysteryImporter.D) // Sharp
                weaponFlags |= WeaponFlags.Sharp;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.E) == Importer.Mystery.MysteryImporter.E) // Vorpal
                weaponFlags |= WeaponFlags.Vorpal;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.F) == Importer.Mystery.MysteryImporter.F) // Two-hands
                weaponFlags |= WeaponFlags.TwoHands;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.G) == Importer.Mystery.MysteryImporter.G) // Shocking
                weaponFlags |= WeaponFlags.Shocking;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.H) == Importer.Mystery.MysteryImporter.H) // Poison
                weaponFlags |= WeaponFlags.Poison;
            if ((weaponType2 & Importer.Mystery.MysteryImporter.I) == Importer.Mystery.MysteryImporter.I) // Holy
                weaponFlags |= WeaponFlags.Holy;
            // J: Weighted
            // K: Necrotism

            //
            return (schoolType, weaponFlags);
        }

        private static (ItemFlags, bool) ConvertMysteryItemExtraFlags(Importer.Mystery.ObjectData data)
        {
            ItemFlags itemFlags = ItemFlags.None;

            bool noTake = false;
            if ((data.WearFlags & Importer.Mystery.MysteryImporter.A) != Importer.Mystery.MysteryImporter.A) noTake = true; // WearFlags A means TAKE

            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.A) == Importer.Mystery.MysteryImporter.A) itemFlags |= ItemFlags.Glowing; // A ITEM_GLOW
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.B) == Importer.Mystery.MysteryImporter.B) itemFlags |= ItemFlags.Humming; // B ITEM_HUM
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.C) == Importer.Mystery.MysteryImporter.C) itemFlags |= ItemFlags.Dark; // C ITEM_DARK
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.D) == Importer.Mystery.MysteryImporter.D) itemFlags |= ItemFlags.Lock; // D ITEM_LOCK
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.E) == Importer.Mystery.MysteryImporter.E) itemFlags |= ItemFlags.Evil; // E ITEM_EVIL
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.F) == Importer.Mystery.MysteryImporter.F) itemFlags |= ItemFlags.Invis; // F ITEM_INVIS
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.G) == Importer.Mystery.MysteryImporter.G) itemFlags |= ItemFlags.Magic; // G ITEM_MAGIC
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.H) == Importer.Mystery.MysteryImporter.H) itemFlags |= ItemFlags.NoDrop; // H ITEM_NODROP
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.I) == Importer.Mystery.MysteryImporter.I) itemFlags |= ItemFlags.Bless; // I ITEM_BLESS
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.J) == Importer.Mystery.MysteryImporter.J) itemFlags |= ItemFlags.AntiGood; // J ITEM_ANTI_GOOD
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.K) == Importer.Mystery.MysteryImporter.K) itemFlags |= ItemFlags.AntiEvil; // K ITEM_ANTI_EVIL
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.L) == Importer.Mystery.MysteryImporter.L) itemFlags |= ItemFlags.AntiNeutral; // L ITEM_ANTI_NEUTRAL
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.M) == Importer.Mystery.MysteryImporter.M) itemFlags |= ItemFlags.NoRemove; // M ITEM_NOREMOVE
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.N) == Importer.Mystery.MysteryImporter.N) itemFlags |= ItemFlags.Inventory; // N ITEM_INVENTORY
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.O) == Importer.Mystery.MysteryImporter.O) itemFlags |= ItemFlags.NoPurge; // O ITEM_NOPURGE
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.P) == Importer.Mystery.MysteryImporter.P) itemFlags |= ItemFlags.RotDeath; // P ITEM_ROT_DEATH
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.Q) == Importer.Mystery.MysteryImporter.Q) itemFlags |= ItemFlags.VisibleDeath; // Q ITEM_VIS_DEATH
            //if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.R) == Importer.Mystery.MysteryImporter.R) itemFlags |= ItemFlags.; // R ITEM_DONATED
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.S) == Importer.Mystery.MysteryImporter.S) itemFlags |= ItemFlags.NonMetal; // S NONMETAL
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.T) == Importer.Mystery.MysteryImporter.T) itemFlags |= ItemFlags.NoLocate; // T ITEM_NOLOCATE
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.U) == Importer.Mystery.MysteryImporter.U) itemFlags |= ItemFlags.MeltOnDrop; // U ITEM_MELT_DROP
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.V) == Importer.Mystery.MysteryImporter.V) itemFlags |= ItemFlags.HadTimer; // V ITEM_HAD_TIMER
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.W) == Importer.Mystery.MysteryImporter.W) itemFlags |= ItemFlags.SellExtract; // W ITEM_SELL_EXTRACT
            // X ITEM_NOSAC
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.Y) == Importer.Mystery.MysteryImporter.Y) itemFlags |= ItemFlags.BurnProof; // Y ITEM_BURN_PROOF
            if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.Z) == Importer.Mystery.MysteryImporter.Z) itemFlags |= ItemFlags.NoUncurse; // Z ITEM_NOUNCURSE
            // aa ITEM_NOIDENT
            //if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.bb) == Importer.Mystery.MysteryImporter.bb) itemFlags |= ItemFlags.Indestructible; // bb ITEM_NOCOND

            return (itemFlags, noTake);
        }

        private static ItemFlags ConvertRomItemExtraFlags(Importer.Rom.ObjectData data)
        {
            // TODO
            return ItemFlags.None;
        }

        private static void CreateWorld()
        {
            string path =  DependencyContainer.Current.GetInstance<ISettings>().ImportAreaPath;

            Importer.Mystery.MysteryImporter mysteryImporter = new Importer.Mystery.MysteryImporter();
            mysteryImporter.Load(System.IO.Path.Combine(path, "limbo.are"));
            mysteryImporter.Parse();
            mysteryImporter.Load(System.IO.Path.Combine(path, "midgaard.are"));
            mysteryImporter.Parse();
            //mysteryImporter.Load(System.IO.Path.Combine(path, "amazon.are"));
            //mysteryImporter.Parse();

            //string fileList = System.IO.Path.Combine(path, "area.lst");
            //string[] areaFilenames = System.IO.File.ReadAllLines(fileList);
            //foreach (string areaFilename in areaFilenames.Where(x => !x.Contains("limbo")))
            //{
            //    if (areaFilename.Contains("$"))
            //        break;
            //    string areaFullName = System.IO.Path.Combine(path, areaFilename);
            //    mysteryImporter.Load(areaFullName);
            //    mysteryImporter.Parse();
            //}


            foreach (var itemTypeAndCount in mysteryImporter.Objects.GroupBy(o => o.ItemType, (itemType, list) => new {itemType, count = list.Count()}).OrderBy(x => x.count))
                Log.Default.WriteLine(LogLevels.Info, "{0} -> {1}", itemTypeAndCount.itemType, itemTypeAndCount.count);

            Dictionary<int, IArea> areasByVnums = new Dictionary<int, IArea>();
            Dictionary<int, IRoom> roomsByVNums = new Dictionary<int, IRoom>();

            // Create Rooms blueprints
            foreach (Importer.Mystery.RoomData importedRoom in mysteryImporter.Rooms)
                CreateRoomBlueprint(importedRoom);
            // Create Character blueprints
            foreach (Importer.Mystery.MobileData mobile in mysteryImporter.Mobiles)
                CreateCharacterBlueprint(mobile);
            // Create Item blueprints
            foreach (Importer.Mystery.ObjectData obj in mysteryImporter.Objects)
                CreateItemBlueprint(obj);
            // Create Areas
            foreach (Importer.Mystery.AreaData importedArea in mysteryImporter.Areas)
            {
                // TODO: levels
                IArea area = World.AddArea(Guid.NewGuid(), importedArea.Name, 1, 99, importedArea.Builders, importedArea.Credits);
                areasByVnums.Add(importedArea.VNum, area);
            }

            // Create Rooms
            foreach (Importer.Mystery.RoomData importedRoom in mysteryImporter.Rooms)
            {
                IArea area = areasByVnums[importedRoom.AreaVnum];
                IRoom room = World.AddRoom(Guid.NewGuid(), World.GetRoomBlueprint(importedRoom.VNum), area);
                roomsByVNums.Add(importedRoom.VNum, room);
            }

            // Create Exits
            foreach (Importer.Mystery.RoomData importedRoom in mysteryImporter.Rooms)
            {
                for (int i = 0; i < Importer.Mystery.RoomData.MaxExits - 1; i++)
                {
                    Importer.Mystery.ExitData exit = importedRoom.Exits[i];
                    if (exit != null)
                    {
                        IRoom from;
                        roomsByVNums.TryGetValue(importedRoom.VNum, out from);
                        IRoom to;
                        roomsByVNums.TryGetValue(exit.DestinationVNum, out to);
                        if (from == null)
                            Log.Default.WriteLine(LogLevels.Warning, "Origin room not found for vnum {0}", importedRoom.VNum);
                        else if (to == null)
                            Log.Default.WriteLine(LogLevels.Warning, "Destination room not found for vnum {0}", importedRoom.VNum);
                        else
                        {
                            ExitBlueprint exitBlueprint = new ExitBlueprint
                            {
                                Destination = exit.DestinationVNum,
                                Description = exit.Description,
                                Key = exit.Key,
                                Keyword = exit.Keyword,
                                Flags = ConvertExitInfo(exit.ExitInfo)
                            };
                            // TODO: add exit to room blueprint
                            World.AddExit(from, to, exitBlueprint, (ExitDirections)i);
                        }
                    }
                }
            }

            // Handle resets
            // TODO: handle rom resets
            INonPlayableCharacter lastCharacter = null;
            IItemContainer lastContainer = null;
            Dictionary<string, int> itemTypes = new Dictionary<string, int>();
            foreach (Importer.Mystery.RoomData importedRoom in mysteryImporter.Rooms.Where(x => x.Resets.Any()))
            {
                IRoom room;
                roomsByVNums.TryGetValue(importedRoom.VNum, out room);
                foreach (Importer.Mystery.ResetData reset in importedRoom.Resets)
                {
                    switch (reset.Command)
                    {
                        case 'M':
                            {
                                CharacterBlueprintBase blueprint = World.GetCharacterBlueprint(reset.Arg1);
                                if (blueprint != null)
                                {
                                    lastCharacter = World.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, room);
                                    Log.Default.WriteLine(LogLevels.Debug, $"Room {importedRoom.VNum}: M: Mob {reset.Arg1} added");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: M: Mob {reset.Arg1} not found");
                                break;
                            }
                        case 'O':
                            {
                                ItemBlueprintBase blueprint = World.GetItemBlueprint(reset.Arg1);
                                if (blueprint != null)
                                {
                                    IItem item = World.AddItem(Guid.NewGuid(), blueprint.Id, room);
                                    if (item != null)
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {importedRoom.VNum}: O: Obj {reset.Arg1} added room");
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: O: Obj {reset.Arg1} not created");
                                    lastContainer = item as IItemContainer; // even if item is not a container, we have to convert it
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: O: Obj {reset.Arg1} not found");
                                //ObjectData obj = importer.Objects.FirstOrDefault(x => x.VNum == reset.Arg1);
                                //if (obj != null)
                                //{
                                //    if (obj.ItemType == "weapon")
                                //    {
                                //        ItemWeaponBlueprint blueprint = World.GetItemBlueprint(reset.Arg1) as ItemWeaponBlueprint;
                                //        World.AddItemWeapon(Guid.NewGuid(), blueprint, room);
                                //        Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: Weapon {reset.Arg1} added on floor");
                                //    }
                                //    else if (obj.ItemType == "container")
                                //    {
                                //        ItemContainerBlueprint blueprint = World.GetItemBlueprint(reset.Arg1) as ItemContainerBlueprint;
                                //        lastContainer = World.AddItemContainer(Guid.NewGuid(), blueprint, room);
                                //        Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: Container {reset.Arg1} added on floor");
                                //    }
                                //}
                                //else
                                //    Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: Obj {reset.Arg1} not found");
                                break;
                            }
                        // P: put object arg1 (add arg2 times at once with max occurence arg4) in object arg3
                        case 'P':
                            {
                                ItemBlueprintBase blueprint = World.GetItemBlueprint(reset.Arg1);
                                if (blueprint != null)
                                {
                                    if (lastContainer != null)
                                    {
                                        World.AddItem(Guid.NewGuid(), blueprint.Id, lastContainer);
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {importedRoom.VNum}: P: Obj {reset.Arg1} added in {lastContainer.Blueprint.Id}");
                                    }
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: P: Last item was not a container");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: P: Obj {reset.Arg1} (type: {mysteryImporter.Objects.FirstOrDefault(x => x.VNum == reset.Arg1)?.ItemType ?? "unknown"}) not found");
                                break;
                            }
                        // G: give object arg1 to mobile 
                        case 'G':
                            {
                                ItemBlueprintBase blueprint = World.GetItemBlueprint(reset.Arg1);
                                if (blueprint != null)
                                {
                                    if (lastCharacter != null)
                                    {
                                        World.AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {importedRoom.VNum}: G: Obj {reset.Arg1} added on {lastCharacter.Blueprint.Id}");
                                    }
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: G: Last character doesn't exist");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: G: Obj {reset.Arg1} (type: {mysteryImporter.Objects.FirstOrDefault(x => x.VNum == reset.Arg1)?.ItemType ?? "unknown"}) not found");
                                break;
                            }
                        // E: equip object arg1 to mobile
                        case 'E':
                            {
                                ItemBlueprintBase blueprint = World.GetItemBlueprint(reset.Arg1);
                                if (blueprint != null)
                                {
                                    if (lastCharacter != null)
                                    {
                                        IItem item = World.AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {importedRoom.VNum}: E: Obj {reset.Arg1} added on {lastCharacter.Blueprint.Id}");
                                        // try to equip
                                        if (item is IEquipableItem equipable)
                                        {
                                            EquipedItem equipedItem = lastCharacter.SearchEquipmentSlot(equipable, false);
                                            if (equipedItem != null)
                                            {
                                                equipedItem.Item = equipable;
                                                equipable.ChangeContainer(null); // remove from inventory
                                                equipable.ChangeEquipedBy(lastCharacter); // set as equiped by lastCharacter
                                            }
                                            else
                                                Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: E: Item {reset.Arg1} wear location {equipable.WearLocation} doesn't exist on last character");
                                        }
                                        else
                                            Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: E: Item {reset.Arg1} cannot be equiped");
                                    }
                                    else
                                        Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: E: Last character doesn't exist");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: E: Obj {reset.Arg1} (type: {mysteryImporter.Objects.FirstOrDefault(x => x.VNum == reset.Arg1)?.ItemType ?? "unknown"}) not found");
                                break;
                            }
                            // D: set state of door  (not used)
                            // R: randomize room exits
                            // Z: maze at arg3 with size arg1*arg2 and map vnum arg4
                            // TODO: other command  D, R, Z
                    }
                }
            }

            //CharacterNormalBlueprint mob2Blueprint = new CharacterNormalBlueprint
            //{
            //    Id = 2,
            //    Name = "mob2",
            //    ShortDescription = "Second mob (female)",
            //    Description = "Second mob (female) is here",
            //    Sex = Sex.Female,
            //    Level = 10
            //};
            //World.AddCharacterBlueprint(mob2Blueprint);
            //CharacterNormalBlueprint mob3Blueprint = new CharacterNormalBlueprint
            //{
            //    Id = 3,
            //    Name = "mob3",
            //    ShortDescription = "Third mob (male)",
            //    Description = "Third mob (male) is here",
            //    Sex = Sex.Male,
            //    Level = 10
            //};
            //World.AddCharacterBlueprint(mob3Blueprint);
            //CharacterBlueprint mob4Blueprint = new CharacterBlueprint
            //{
            //    Id = 4,
            //    Name = "mob4",
            //    ShortDescription = "Fourth mob (neutral)",
            //    Description = "Fourth mob (neutral) is here",
            //    Sex = Sex.Neutral,
            //    Level = 10
            //};
            //CharacterNormalBlueprint mob5Blueprint = new CharacterNormalBlueprint
            //{
            //    Id = 5,
            //    Name = "mob5",
            //    ShortDescription = "Fifth mob (female)",
            //    Description = "Fifth mob (female) is here",
            //    Sex = Sex.Female,
            //    Level = 10
            //};
            //World.AddCharacterBlueprint(mob5Blueprint);

            //ItemContainerBlueprint item1Blueprint = new ItemContainerBlueprint
            //{
            //    Id = 1,
            //    Name = "item1 first",
            //    ShortDescription = "First item (container)",
            //    Description = "The first item (container) has been left here.",
            //    ItemCount = 10,
            //    WeightMultiplier = 100
            //};
            //World.AddItemBlueprint(item1Blueprint);
            //ItemWeaponBlueprint item2Blueprint = new ItemWeaponBlueprint
            //{
            //    Id = 2,
            //    Name = "item2 second",
            //    ShortDescription = "Second item (weapon)",
            //    Description = "The second item (weapon) has been left here.",
            //    Type = WeaponTypes.Axe1H,
            //    DiceCount = 10,
            //    DiceValue = 20,
            //    DamageType = SchoolTypes.Fire,
            //    WearLocation = WearLocations.Wield
            //};
            //World.AddItemBlueprint(item2Blueprint);
            //ItemArmorBlueprint item3Blueprint = new ItemArmorBlueprint
            //{
            //    Id = 3,
            //    Name = "item3 third",
            //    ShortDescription = "Third item (armor|feet)",
            //    Description = "The third item (armor|feet) has been left here.",
            //    Bash = 100,
            //    Pierce = 110,
            //    Slash = 120,
            //    Exotic = 130,
            //    WearLocation = WearLocations.Feet
            //};
            //World.AddItemBlueprint(item3Blueprint);
            //ItemLightBlueprint item4Blueprint = new ItemLightBlueprint
            //{
            //    Id = 4,
            //    Name = "item4 fourth",
            //    ShortDescription = "Fourth item (light)",
            //    Description = "The fourth item (light) has been left here.",
            //    DurationHours = -1,
            //    WearLocation = WearLocations.Light
            //};
            //World.AddItemBlueprint(item4Blueprint);
            //ItemWeaponBlueprint item5Blueprint = new ItemWeaponBlueprint
            //{
            //    Id = 5,
            //    Name = "item5 fifth",
            //    ShortDescription = "Fifth item (weapon)",
            //    Description = "The fifth item (weapon) has been left here.",
            //    Type = WeaponTypes.Sword1H,
            //    DiceCount = 5,
            //    DiceValue = 40,
            //    DamageType = SchoolTypes.Slash,
            //    WearLocation = WearLocations.Wield
            //};
            //World.AddItemBlueprint(item5Blueprint);
            //ItemWeaponBlueprint item6Blueprint = new ItemWeaponBlueprint
            //{
            //    Id = 6,
            //    Name = "item6 sixth",
            //    ShortDescription = "Sixth item (weapon 2H)",
            //    Description = "The sixth item (weapon 2H) has been left here.",
            //    Type = WeaponTypes.Mace2H,
            //    DiceCount = 10,
            //    DiceValue = 20,
            //    DamageType = SchoolTypes.Holy,
            //    WearLocation = WearLocations.Wield2H
            //};
            //World.AddItemBlueprint(item6Blueprint);
            //ItemShieldBlueprint item7Blueprint = new ItemShieldBlueprint
            //{
            //    Id = 7,
            //    Name = "item7 seventh",
            //    ShortDescription = "Seventh item (shield)",
            //    Description = "The seventh item (shield) has been left here.",
            //    Armor = 1000,
            //    WearLocation = WearLocations.Shield
            //};
            //World.AddItemBlueprint(item7Blueprint);
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
            //ItemDrinkContainerBlueprint item10Blueprint = new ItemDrinkContainerBlueprint
            //{
            //    Id = 10,
            //    Name = "item10 tenth",
            //    ShortDescription = "Tenth item (drink container)",
            //    Description = "The tenth item (drink container) has been left here.",
            //    MaxLiquidAmount = 500,
            //    CurrentLiquidAmount = 100,
            //    LiquidType = "rum",
            //    IsPoisoned = true
            //};
            //World.AddItemBlueprint(item10Blueprint);
            //ItemFoodBlueprint item11Blueprint = new ItemFoodBlueprint
            //{
            //    Id = 11,
            //    Name = "item11 eleventh",
            //    ShortDescription = "Eleventh item (food)",
            //    Description = "The eleventh item (food) has been left here.",
            //    FullHours = 10,
            //    HungerHours = 4,
            //    IsPoisoned = true
            //};
            //World.AddItemBlueprint(item11Blueprint);

            //
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
            {
                Id = DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId,
                Name = "corpse"
            }; // this is mandatory
            World.AddItemBlueprint(corpseBlueprint);

            // Add dummy mobs and items to allow impersonate :)
            IRoom templeOfMota = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
            IRoom templeSquare = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple square");
            IRoom marketSquare = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "market square");
            IRoom commonSquare = World.Rooms.FirstOrDefault(x => x.Name.ToLower() == "the common square");

            ////ICharacter mob1 = World.AddCharacter(Guid.NewGuid(), "mob1", Repository.ClassManager["Druid"], Repository.RaceManager["Insectoid"], Sex.Male, templeOfMota); // playable
            //ICharacter mob2 = World.AddNonPlayableCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
            //ICharacter mob3 = World.AddNonPlayableCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
            ////ICharacter mob4 = World.AddCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
            ////ICharacter mob4 = World.AddCharacter(Guid.NewGuid(), "mob4", Repository.ClassManager["Warrior"], Repository.RaceManager["Dwarf"], Sex.Female, templeSquare); // playable
            //ICharacter mob5 = World.AddNonPlayableCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

            //World.AddItem(Guid.NewGuid(), item1Blueprint, templeOfMota);
            //IItemContainer item1Dup1 = World.AddItem(Guid.NewGuid(), item1Blueprint, mob5) as IItemContainer;
            //World.AddItem(Guid.NewGuid(), item3Blueprint, item1Dup1);
            //IItemWeapon item2 = World.AddItem(Guid.NewGuid(), item2Blueprint, mob2) as IItemWeapon;
            ////World.AddItem(Guid.NewGuid(), item4Blueprint, mob1);
            ////World.AddItem(Guid.NewGuid(), item5Blueprint, mob1);
            ////World.AddItem(Guid.NewGuid(), item1Blueprint, mob1);
            //IItem item3OnMob3 = World.AddItem(Guid.NewGuid(), item3Blueprint, mob3);
            //item3OnMob3.AddBaseItemFlags(ItemFlags.RotDeath);
            ////World.AddItemLight(Guid.NewGuid(), item4Blueprint, mob4);
            //World.AddItem(Guid.NewGuid(), item6Blueprint, templeSquare);
            //World.AddItem(Guid.NewGuid(), item7Blueprint, templeOfMota);
            World.AddItem(Guid.NewGuid(), questItem2Blueprint, templeSquare);
            //World.AddItem(Guid.NewGuid(), item10Blueprint, commonSquare);
            //World.AddItem(Guid.NewGuid(), item11Blueprint, commonSquare);

            //// Equip weapon on mob2
            //mob2.Equipments.First(x => x.Slot == EquipmentSlots.MainHand).Item = item2;
            //item2.ChangeContainer(null);
            //item2.ChangeEquipedBy(mob2);

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

            // Give quest 1 and 2 to mob1
            //IQuest quest1 = new Quest.Quest(questBlueprint1, mob1, mob2);
            //mob1.AddQuest(quest1);
            //IQuest quest2 = new Quest.Quest(questBlueprint2, mob1, mob2);
            //mob1.AddQuest(quest2);

            //// Search extra description
            //foreach(IRoom room inWorld.Rooms.Where(x => x.ExtraDescriptions != null && x.ExtraDescriptions.Any()))
            //    Log.Default.WriteLine(LogLevels.Info, "Room {0} has extra description: {1}", room.DebugName, room.ExtraDescriptions.Keys.Aggregate((n,i) => n+","+i));
            //foreach (IItem item inWorld.Items.Where(x => x.ExtraDescriptions != null && x.ExtraDescriptions.Any()))
            //    Log.Default.WriteLine(LogLevels.Info, "Item {0} has extra description: {1}", item.DebugName, item.ExtraDescriptions.Keys.Aggregate((n, i) => n + "," + i));
        }
    }
}
