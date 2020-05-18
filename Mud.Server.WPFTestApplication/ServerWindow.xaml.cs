﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AutoMapper;
using Mud.Container;
using Mud.Domain;
using Mud.Importer.Mystery;
using Mud.Logger;
using Mud.Network;
using Mud.Network.Telnet;
using Mud.Repository;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Item;
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

        private static RoomBlueprint CreateRoomBlueprint(RoomData data)
        {
            RoomBlueprint blueprint = new RoomBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                ExtraDescriptions = RoomBlueprint.BuildExtraDescriptions(data.ExtraDescr),
                RoomFlags = ConvertRoomFlags(data.Flags),
                SectorType = ConvertSectorTypes(data.Sector),
                HealRate = data.HealRate,
                ResourceRate = data.ManaRate,
                MaxSize = ConvertSize(data.MaxSize),
                // Exits will be done when each room blueprint is created
            };
            World.AddRoomBlueprint(blueprint);
            return blueprint;
        }

        private static ItemBlueprintBase CreateItemBlueprint(ObjectData data)
        {
            (ItemFlags itemFlags, bool noTake) extraFlags = ConvertMysteryItemExtraFlags(data);
            ItemBlueprintBase blueprint;
            if (data.ItemType == "weapon")
            {
                (SchoolTypes damageType, WeaponFlags weaponFlags, string damageNoun) weaponInfo = ConvertWeaponDamageType(data.Values[3], data.Values[4]);
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
                    DamageNoun = weaponInfo.damageNoun,
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
                    MaxWeight = Convert.ToInt32(data.Values[0]),
                    ContainerFlags = ConvertContainerFlags(data),
                    Key = Convert.ToInt32(data.Values[2]),
                    MaxWeightPerItem = Convert.ToInt32(data.Values[3]),
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
            else if (data.ItemType == "jewelry")
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
            else if (data.ItemType == "treasure")
            {
                blueprint = new ItemTreasureBlueprint
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
            else if (data.ItemType == "money")
            {
                blueprint = new ItemMoneyBlueprint
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
                    SilverCoins = Convert.ToInt64(data.Values[0]),
                    GoldCoins = Convert.ToInt64(data.Values[1]),
                    NoTake = extraFlags.noTake,
                };
            }
            else if (data.ItemType == "boat")
            {
                blueprint = new ItemBoatBlueprint
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
                    PortalFlags = ConvertPortalFlags(data),
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
            else if (data.ItemType == "pill")
            {
                blueprint = new ItemPillBlueprint
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
                    SpellLevel = Convert.ToInt32(data.Values[0]),
                    Spell1 = Convert.ToString(data.Values[1]),
                    Spell2 = Convert.ToString(data.Values[2]),
                    Spell3 = Convert.ToString(data.Values[3]),
                    Spell4 = Convert.ToString(data.Values[4]),
                };
            }
            else if (data.ItemType == "potion")
            {
                blueprint = new ItemPotionBlueprint
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
                    SpellLevel = Convert.ToInt32(data.Values[0]),
                    Spell1 = Convert.ToString(data.Values[1]),
                    Spell2 = Convert.ToString(data.Values[2]),
                    Spell3 = Convert.ToString(data.Values[3]),
                    Spell4 = Convert.ToString(data.Values[4]),
                };
            }
            else if (data.ItemType == "scroll")
            {
                blueprint = new ItemScrollBlueprint
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
                    SpellLevel = Convert.ToInt32(data.Values[0]),
                    Spell1 = Convert.ToString(data.Values[1]),
                    Spell2 = Convert.ToString(data.Values[2]),
                    Spell3 = Convert.ToString(data.Values[3]),
                    Spell4 = Convert.ToString(data.Values[4]),
                };
            }
            else if (data.ItemType == "staff")
            {
                blueprint = new ItemStaffBlueprint
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
                    SpellLevel = Convert.ToInt32(data.Values[0]),
                    MaxChargeCount = Convert.ToInt32(data.Values[1]) == 0 ? Convert.ToInt32(data.Values[2]) : Convert.ToInt32(data.Values[1]),
                    CurrentChargeCount = Convert.ToInt32(data.Values[2]),
                    Spell = Convert.ToString(data.Values[3]),
                    AlreadyRecharged = Convert.ToInt32(data.Values[1]) == 0
                };
            }
            else if (data.ItemType == "wand")
            {
                blueprint = new ItemWandBlueprint
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
                    SpellLevel = Convert.ToInt32(data.Values[0]),
                    MaxChargeCount = Convert.ToInt32(data.Values[1]) == 0 ? Convert.ToInt32(data.Values[2]) : Convert.ToInt32(data.Values[1]),
                    CurrentChargeCount = Convert.ToInt32(data.Values[2]),
                    Spell = Convert.ToString(data.Values[3]),
                    AlreadyRecharged = Convert.ToInt32(data.Values[1]) == 0
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

        private static CharacterNormalBlueprint CreateCharacterBlueprint(MobileData data)
        {
            Sex sex = Sex.Neutral;
            if (data.Sex.ToLower() == "female")
                sex = Sex.Female;
            else if (data.Sex.ToLower() == "male")
                sex = Sex.Male;
            SchoolTypes schoolType = SchoolTypes.None;
            string damageNoun = data.DamageType;
            (string name, string noun, SchoolTypes damType) attackTableEntry = AttackTable.FirstOrDefault(x => x.name == data.DamageType);
            if (!attackTableEntry.Equals(default))
            {
                schoolType = attackTableEntry.damType;
                damageNoun = attackTableEntry.noun;
            }
            CharacterNormalBlueprint blueprint = new CharacterNormalBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                Level = data.Level,
                LongDescription = data.LongDescr,
                ShortDescription = data.ShortDescr,
                Sex = sex,
                Wealth = data.Wealth,
                Alignment = data.Alignment,
                DamageNoun = damageNoun,
                DamageType = schoolType,
                DamageDiceCount = data.Damage[0],
                DamageDiceValue = data.Damage[1],
                DamageDiceBonus = data.Damage[2],
                HitPointDiceCount =  data.Hit[0],
                HitPointDiceValue = data.Hit[1],
                HitPointDiceBonus = data.Hit[2],
                ManaDiceCount = data.Mana[0],
                ManaDiceValue = data.Mana[1],
                ManaDiceBonus = data.Mana[2],
                HitRollBonus = data.HitRoll,
                ArmorPierce = data.Armor[0],
                ArmorBash = data.Armor[1],
                ArmorSlash = data.Armor[2],
                ArmorExotic = data.Armor[3],
                CharacterFlags = ConvertMysteryCharacterFlags(data.AffectedBy, data.AffectedBy2),
                ActFlags = ConvertMysteryActFlags(data.Act),
                OffensiveFlags = ConvertMysteryOffensiveFlags(data.OffFlags),
                Immunities = ConvertMysteryIRV(data.ImmFlags),
                Resistances = ConvertMysteryIRV(data.ResFlags),
                Vulnerabilities = ConvertMysteryIRV(data.VulnFlags),
            };
            World.AddCharacterBlueprint(blueprint);
            return blueprint;
        }

        private static bool HasBit(long data, long bit) => (data & bit) == bit;

        private static Domain.Sizes ConvertSize(int size)
        {
            switch ((Importer.Mystery.Sizes)size)
            {
                case Importer.Mystery.Sizes.SIZE_TINY: return Domain.Sizes.Tiny;
                case Importer.Mystery.Sizes.SIZE_SMALL: return Domain.Sizes.Small;
                case Importer.Mystery.Sizes.SIZE_MEDIUM: return Domain.Sizes.Medium;
                case Importer.Mystery.Sizes.SIZE_LARGE: return Domain.Sizes.Large;
                case Importer.Mystery.Sizes.SIZE_HUGE: return Domain.Sizes.Huge;
                case Importer.Mystery.Sizes.SIZE_GIANT: return Domain.Sizes.Giant;
            }
            return Domain.Sizes.Medium;
        }

        private static Domain.SectorTypes ConvertSectorTypes(int sector)
        {
            switch ((Importer.Mystery.SectorTypes)sector)
            {
                case Importer.Mystery.SectorTypes.SECT_INSIDE: return Domain.SectorTypes.Inside;
                case Importer.Mystery.SectorTypes.SECT_CITY: return Domain.SectorTypes.City;
                case Importer.Mystery.SectorTypes.SECT_FIELD: return Domain.SectorTypes.Field;
                case Importer.Mystery.SectorTypes.SECT_FOREST: return Domain.SectorTypes.Forest;
                case Importer.Mystery.SectorTypes.SECT_HILLS: return Domain.SectorTypes.Hills;
                case Importer.Mystery.SectorTypes.SECT_MOUNTAIN: return Domain.SectorTypes.Mountain;
                case Importer.Mystery.SectorTypes.SECT_WATER_SWIM: return Domain.SectorTypes.WaterSwim;
                case Importer.Mystery.SectorTypes.SECT_WATER_NOSWIM: return Domain.SectorTypes.WaterNoSwim;
                case Importer.Mystery.SectorTypes.SECT_BURNING: return Domain.SectorTypes.Burning;
                case Importer.Mystery.SectorTypes.SECT_AIR: return Domain.SectorTypes.Air;
                case Importer.Mystery.SectorTypes.SECT_DESERT: return Domain.SectorTypes.Desert;
                case Importer.Mystery.SectorTypes.SECT_UNDERWATER: return Domain.SectorTypes.Underwater;
                default: return Domain.SectorTypes.Inside;
            }
        }

        private static RoomFlags ConvertRoomFlags(long flag)
        {
            RoomFlags flags = RoomFlags.None;
            if (HasBit(flag, MysteryImporter.A)) flags |= RoomFlags.Dark;
            // B
            if (HasBit(flag, MysteryImporter.C)) flags |= RoomFlags.NoMob;
            if (HasBit(flag, MysteryImporter.D)) flags |= RoomFlags.Indoors;
            if (HasBit(flag, MysteryImporter.E)) flags |= RoomFlags.NoScan;
            // F, G, H, I
            if (HasBit(flag, MysteryImporter.J)) flags |= RoomFlags.Private;
            if (HasBit(flag, MysteryImporter.K)) flags |= RoomFlags.Safe;
            if (HasBit(flag, MysteryImporter.L)) flags |= RoomFlags.Solitary;
            // M
            if (HasBit(flag, MysteryImporter.N)) flags |= RoomFlags.NoRecall;
            if (HasBit(flag, MysteryImporter.O)) flags |= RoomFlags.ImpOnly;
            if (HasBit(flag, MysteryImporter.P)) flags |= RoomFlags.GodsOnly;
            // Q
            if (HasBit(flag, MysteryImporter.R)) flags |= RoomFlags.NewbiesOnly;
            if (HasBit(flag, MysteryImporter.S)) flags |= RoomFlags.Law;
            if (HasBit(flag, MysteryImporter.T)) flags |= RoomFlags.NoWhere;

            return flags;
        }
        
        private static ExitFlags ConvertExitInfo(long exitInfo)
        {
            ExitFlags flags = 0;
            if ((exitInfo & MysteryImporter.A) == MysteryImporter.A)
                flags |= ExitFlags.Door;
            if ((exitInfo & MysteryImporter.B) == MysteryImporter.B)
                flags |= ExitFlags.Closed;
            if ((exitInfo & MysteryImporter.C) == MysteryImporter.C)
                flags |= ExitFlags.Locked;
            if ((exitInfo & MysteryImporter.F) == MysteryImporter.F)
                flags |= ExitFlags.PickProof;
            if ((exitInfo & MysteryImporter.G) == MysteryImporter.G)
                flags |= ExitFlags.NoPass;
            if ((exitInfo & MysteryImporter.H) == MysteryImporter.H)
                flags |= ExitFlags.Easy;
            if ((exitInfo & MysteryImporter.I) == MysteryImporter.I)
                flags |= ExitFlags.Hard;
            if ((exitInfo & MysteryImporter.M) == MysteryImporter.M)
                flags |= ExitFlags.Hidden;
            return flags;
        }

        private static IRVFlags ConvertMysteryIRV(long value)
        {
            IRVFlags flags = 0;
            if ((value & MysteryImporter.A) == MysteryImporter.A) flags |= IRVFlags.Summon;
            if ((value & MysteryImporter.B) == MysteryImporter.B) flags |= IRVFlags.Charm;
            if ((value & MysteryImporter.C) == MysteryImporter.C) flags |= IRVFlags.Magic;
            if ((value & MysteryImporter.D) == MysteryImporter.D) flags |= IRVFlags.Weapon;
            if ((value & MysteryImporter.E) == MysteryImporter.E) flags |= IRVFlags.Bash;
            if ((value & MysteryImporter.F) == MysteryImporter.F) flags |= IRVFlags.Pierce;
            if ((value & MysteryImporter.G) == MysteryImporter.G) flags |= IRVFlags.Slash;
            if ((value & MysteryImporter.H) == MysteryImporter.H) flags |= IRVFlags.Fire;
            if ((value & MysteryImporter.I) == MysteryImporter.I) flags |= IRVFlags.Cold;
            if ((value & MysteryImporter.J) == MysteryImporter.J) flags |= IRVFlags.Lightning;
            if ((value & MysteryImporter.K) == MysteryImporter.K) flags |= IRVFlags.Acid;
            if ((value & MysteryImporter.L) == MysteryImporter.L) flags |= IRVFlags.Poison;
            if ((value & MysteryImporter.M) == MysteryImporter.M) flags |= IRVFlags.Negative;
            if ((value & MysteryImporter.N) == MysteryImporter.N) flags |= IRVFlags.Holy;
            if ((value & MysteryImporter.O) == MysteryImporter.O) flags |= IRVFlags.Energy;
            if ((value & MysteryImporter.P) == MysteryImporter.P) flags |= IRVFlags.Mental;
            if ((value & MysteryImporter.Q) == MysteryImporter.Q) flags |= IRVFlags.Disease;
            if ((value & MysteryImporter.R) == MysteryImporter.R) flags |= IRVFlags.Drowning;
            if ((value & MysteryImporter.S) == MysteryImporter.S) flags |= IRVFlags.Light;
            if ((value & MysteryImporter.T) == MysteryImporter.T) flags |= IRVFlags.Sound;
            // U 
            // V PARALYSIS
            if ((value & MysteryImporter.X) == MysteryImporter.X) flags |= IRVFlags.Wood;
            if ((value & MysteryImporter.Y) == MysteryImporter.Y) flags |= IRVFlags.Silver;
            if ((value & MysteryImporter.Z) == MysteryImporter.Z) flags |= IRVFlags.Iron;

            return flags;
        }

        private static CharacterFlags ConvertMysteryCharacterFlags(long affectedBy, long affectedBy2)
        {
            CharacterFlags flags = CharacterFlags.None;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_BLIND)) flags |= CharacterFlags.Blind;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_INVISIBLE)) flags |= CharacterFlags.Invisible;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_DETECT_EVIL)) flags |= CharacterFlags.DetectEvil;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_DETECT_INVIS)) flags |= CharacterFlags.DetectInvis;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_DETECT_MAGIC)) flags |= CharacterFlags.DetectMagic;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_DETECT_HIDDEN)) flags |= CharacterFlags.DetectHidden;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_DETECT_GOOD)) flags |= CharacterFlags.DetectGood;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_SANCTUARY)) flags |= CharacterFlags.Sanctuary;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_FAERIE_FIRE)) flags |= CharacterFlags.FaerieFire;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_INFRARED)) flags |= CharacterFlags.Infrared;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_CURSE)) flags |= CharacterFlags.Curse;
            //if (HasBit(affectedBy, (long)Importer.Mystery.AffectedBy.AFF_ROOTED)) flags |= CharacterFlags.
            if (HasBit(affectedBy, (long)AffectedBy.AFF_POISON)) flags |= CharacterFlags.Poison;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_PROTECT_EVIL)) flags |= CharacterFlags.ProtectEvil;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_PROTECT_GOOD)) flags |= CharacterFlags.ProtectGood;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_SNEAK)) flags |= CharacterFlags.Sneak;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_HIDE)) flags |= CharacterFlags.Hide;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_SLEEP)) flags |= CharacterFlags.Sleep;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_CHARM)) flags |= CharacterFlags.Charm;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_FLYING)) flags |= CharacterFlags.Flying;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_PASS_DOOR)) flags |= CharacterFlags.PassDoor;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_HASTE)) flags |= CharacterFlags.Haste;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_CALM)) flags |= CharacterFlags.Calm;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_PLAGUE)) flags |= CharacterFlags.Plague;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_WEAKEN)) flags |= CharacterFlags.Weaken;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_DARK_VISION)) flags |= CharacterFlags.DarkVision;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_BERSERK)) flags |= CharacterFlags.Berserk;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_SWIM)) flags |= CharacterFlags.Swim;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_REGENERATION)) flags |= CharacterFlags.Regeneration;
            if (HasBit(affectedBy, (long)AffectedBy.AFF_SLOW)) flags |= CharacterFlags.Slow;
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
            if (HasBit(act, (long)Act.ACT_SENTINEL)) flags |= ActFlags.Sentinel;
            if (HasBit(act, (long)Act.ACT_SCAVENGER)) flags |= ActFlags.Scavenger;
            if (HasBit(act, (long)Act.ACT_AWARE)) flags |= ActFlags.Aware;
            if (HasBit(act, (long)Act.ACT_AGGRESSIVE)) flags |= ActFlags.Aggressive;
            if (HasBit(act, (long)Act.ACT_STAY_AREA)) flags |= ActFlags.StayArea;
            if (HasBit(act, (long)Act.ACT_WIMPY)) flags |= ActFlags.Wimpy;
            if (HasBit(act, (long)Act.ACT_PET)) flags |= ActFlags.Pet;
            if (HasBit(act, (long)Act.ACT_TRAIN)) flags |= ActFlags.Train;
            if (HasBit(act, (long)Act.ACT_PRACTICE)) flags |= ActFlags.Practice;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_FREE_WANDER
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_MOUNTABLE
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_IS_MOUNTED
            if (HasBit(act, (long)Act.ACT_UNDEAD)) flags |= ActFlags.Undead;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_NOSLEEP))
            if (HasBit(act, (long)Act.ACT_CLERIC)) flags |= ActFlags.Cleric;
            if (HasBit(act, (long)Act.ACT_MAGE)) flags |= ActFlags.Mage;
            if (HasBit(act, (long)Act.ACT_THIEF)) flags |= ActFlags.Thief;
            if (HasBit(act, (long)Act.ACT_WARRIOR)) flags |= ActFlags.Warrior;
            if (HasBit(act, (long)Act.ACT_NOALIGN)) flags |= ActFlags.NoAlign;
            if (HasBit(act, (long)Act.ACT_NOPURGE)) flags |= ActFlags.NoPurge;
            if (HasBit(act, (long)Act.ACT_OUTDOORS)) flags |= ActFlags.Outdoors;
            if (HasBit(act, (long)Act.ACT_INDOORS)) flags |= ActFlags.Indoors;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_CREATED
            if (HasBit(act, (long)Act.ACT_IS_HEALER)) flags |= ActFlags.IsHealer;
            if (HasBit(act, (long)Act.ACT_GAIN)) flags |= ActFlags.Gain;
            if (HasBit(act, (long)Act.ACT_UPDATE_ALWAYS)) flags |= ActFlags.UpdateAlways;
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_RESERVED
            //if (HasBit(act, (long)Importer.Mystery.Act.ACT_IS_SAFE
            return flags;
        }

        private static OffensiveFlags ConvertMysteryOffensiveFlags(long off)
        {
            OffensiveFlags flags = OffensiveFlags.None;
            if (HasBit(off, (long)Offensive.OFF_AREA_ATTACK)) flags |= OffensiveFlags.AreaAttack;
            if (HasBit(off, (long)Offensive.OFF_BACKSTAB)) flags |= OffensiveFlags.Backstab;
            if (HasBit(off, (long)Offensive.OFF_BASH)) flags |= OffensiveFlags.Bash;
            if (HasBit(off, (long)Offensive.OFF_BERSERK)) flags |= OffensiveFlags.Berserk;
            if (HasBit(off, (long)Offensive.OFF_DISARM)) flags |= OffensiveFlags.Disarm;
            if (HasBit(off, (long)Offensive.OFF_DODGE)) flags |= OffensiveFlags.Dodge;
            if (HasBit(off, (long)Offensive.OFF_FADE)) flags |= OffensiveFlags.Fade;
            if (HasBit(off, (long)Offensive.OFF_FAST)) flags |= OffensiveFlags.Fast;
            if (HasBit(off, (long)Offensive.OFF_KICK)) flags |= OffensiveFlags.Kick;
            if (HasBit(off, (long)Offensive.OFF_KICK_DIRT)) flags |= OffensiveFlags.DirtKick;
            if (HasBit(off, (long)Offensive.OFF_PARRY)) flags |= OffensiveFlags.Parry;
            if (HasBit(off, (long)Offensive.OFF_RESCUE)) flags |= OffensiveFlags.Rescue;
            if (HasBit(off, (long)Offensive.OFF_TAIL)) flags |= OffensiveFlags.Tail;
            if (HasBit(off, (long)Offensive.OFF_TRIP)) flags |= OffensiveFlags.Trip;
            if (HasBit(off, (long)Offensive.OFF_CRUSH)) flags |= OffensiveFlags.Crush;
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
            if ((flag & MysteryImporter.A) == MysteryImporter.A
                || (flag & MysteryImporter.B) == MysteryImporter.B
                || (flag & MysteryImporter.C) == MysteryImporter.C)
                actions |= FurnitureActions.Stand;
            if ((flag & MysteryImporter.D) == MysteryImporter.D
                || (flag & MysteryImporter.E) == MysteryImporter.E
                || (flag & MysteryImporter.F) == MysteryImporter.F)
                actions |= FurnitureActions.Sit;
            if ((flag & MysteryImporter.G) == MysteryImporter.G
                || (flag & MysteryImporter.H) == MysteryImporter.H
                || (flag & MysteryImporter.I) == MysteryImporter.I)
                actions |= FurnitureActions.Rest;
            if ((flag & MysteryImporter.J) == MysteryImporter.J
                || (flag & MysteryImporter.K) == MysteryImporter.K
                || (flag & MysteryImporter.L) == MysteryImporter.L)
                actions |= FurnitureActions.Sleep;
            return actions;
        }

        private static FurniturePlacePrepositions ConvertFurniturePreposition(object value)
        {
            FurniturePlacePrepositions preposition = FurniturePlacePrepositions.None;

            int flag = value == null ? 0 : Convert.ToInt32(value);
            if ((flag & MysteryImporter.A) == MysteryImporter.A
                || (flag & MysteryImporter.D) == MysteryImporter.D
                || (flag & MysteryImporter.G) == MysteryImporter.G
                || (flag & MysteryImporter.J) == MysteryImporter.J)
                preposition = FurniturePlacePrepositions.At;
            else if ((flag & MysteryImporter.B) == MysteryImporter.B
                || (flag & MysteryImporter.E) == MysteryImporter.E
                || (flag & MysteryImporter.H) == MysteryImporter.H
                || (flag & MysteryImporter.K) == MysteryImporter.K)
                preposition = FurniturePlacePrepositions.On;
            else if ((flag & MysteryImporter.C) == MysteryImporter.C
                || (flag & MysteryImporter.F) == MysteryImporter.F
                || (flag & MysteryImporter.I) == MysteryImporter.I
                || (flag & MysteryImporter.L) == MysteryImporter.L)
                preposition = FurniturePlacePrepositions.In;
            return preposition;
        }

        private static ContainerFlags ConvertContainerFlags(ObjectData data)
        {
            return ContainerFlags.None; // never been used in Rom ?!?!?!?
        }

        private static PortalFlags ConvertPortalFlags(ObjectData data)
        {
            // v1: exit flags
            // v2: gate flags
            PortalFlags flags = PortalFlags.None;
            int v1 = Convert.ToInt32(data.Values[1]);
            if (HasBit(v1, MysteryImporter.A)) flags |= PortalFlags.Closed;
            if (HasBit(v1, MysteryImporter.C)) flags |= PortalFlags.Locked;
            if (HasBit(v1, MysteryImporter.F)) flags |= PortalFlags.PickProof;
            if (HasBit(v1, MysteryImporter.H)) flags |= PortalFlags.Easy;
            if (HasBit(v1, MysteryImporter.I)) flags |= PortalFlags.Hard;
            if (HasBit(v1, MysteryImporter.K)) flags |= PortalFlags.NoClose;
            if (HasBit(v1, MysteryImporter.L)) flags |= PortalFlags.NoLock;
            int v2 = Convert.ToInt32(data.Values[2]);
            if (HasBit(v2, MysteryImporter.B)) flags |= PortalFlags.NoCurse;
            if (HasBit(v2, MysteryImporter.C)) flags |= PortalFlags.GoWith;
            if (HasBit(v2, MysteryImporter.D)) flags |= PortalFlags.Buggy;
            if (HasBit(v2, MysteryImporter.E)) flags |= PortalFlags.Random;
            return flags;
        }

        private static EquipmentSlots ConvertWearLocation(int resetDataWearLocation)
        {
            switch ((ResetDataWearLocation)resetDataWearLocation)
            {
                case ResetDataWearLocation.WEAR_NONE: return EquipmentSlots.None;
                case ResetDataWearLocation.WEAR_LIGHT: return EquipmentSlots.Light;
                case ResetDataWearLocation.WEAR_FINGER_L:
                case ResetDataWearLocation.WEAR_FINGER_R: return EquipmentSlots.Ring;
                case ResetDataWearLocation.WEAR_NECK_1:
                case ResetDataWearLocation.WEAR_NECK_2: return EquipmentSlots.Amulet;
                case ResetDataWearLocation.WEAR_BODY: return EquipmentSlots.Chest;
                case ResetDataWearLocation.WEAR_HEAD: return EquipmentSlots.Head;
                case ResetDataWearLocation.WEAR_LEGS: return EquipmentSlots.Legs;
                case ResetDataWearLocation.WEAR_FEET: return EquipmentSlots.Feet;
                case ResetDataWearLocation.WEAR_HANDS: return EquipmentSlots.Hands;
                case ResetDataWearLocation.WEAR_ARMS: return EquipmentSlots.Arms;
                case ResetDataWearLocation.WEAR_SHIELD: return EquipmentSlots.OffHand;
                case ResetDataWearLocation.WEAR_ABOUT: return EquipmentSlots.Cloak;
                case ResetDataWearLocation.WEAR_WAIST: return EquipmentSlots.Waist;
                case ResetDataWearLocation.WEAR_WRIST_L:
                case ResetDataWearLocation.WEAR_WRIST_R: return EquipmentSlots.Wrists;
                case ResetDataWearLocation.WEAR_WIELD: return EquipmentSlots.MainHand;
                case ResetDataWearLocation.WEAR_HOLD: return EquipmentSlots.OffHand;
                case ResetDataWearLocation.WEAR_FLOAT: return EquipmentSlots.Float;
            }
            return EquipmentSlots.None;
        }

        private static WearLocations ConvertWearLocation(ObjectData data)
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
                case MysteryImporter.B: // B finger
                    return WearLocations.Ring;
                case MysteryImporter.C: // C neck
                    return WearLocations.Amulet;
                case MysteryImporter.D: // D body
                    return WearLocations.Chest;
                case MysteryImporter.E: // E head
                    return WearLocations.Head;
                case MysteryImporter.F: // F legs
                    return WearLocations.Legs;
                case MysteryImporter.G: // G feet
                    return WearLocations.Feet;
                case MysteryImporter.H: // H hands
                    return WearLocations.Hands;
                case MysteryImporter.I: // I arms
                    return WearLocations.Arms;
                case MysteryImporter.J: // J shield
                    return WearLocations.Shield;
                case MysteryImporter.K: // K about
                    return WearLocations.Cloak;
                case MysteryImporter.L: // L waist
                    return WearLocations.Waist;
                case MysteryImporter.M: // M wrist
                    return WearLocations.Wrists;
                case MysteryImporter.N: // N wield
                    if (data.ItemType == "weapon" && HasBit(data.Values[4] == null ? 0 : Convert.ToInt32(data.Values[4]), MysteryImporter.F)) // Two-hands
                        return WearLocations.Wield2H;
                    return WearLocations.Wield;
                case MysteryImporter.O: // O hold
                    return WearLocations.Hold;
                case MysteryImporter.Q: // Q float
                    return WearLocations.Float;
                //case Importer.Mystery.MysteryImporter.R: // R ear
                case MysteryImporter.S: // S eyes
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
                    return WeaponTypes.Exotic;
                case "sword": // Sword
                    return WeaponTypes.Sword;
                case "dagger": // Dagger
                    return WeaponTypes.Dagger;
                case "spear": // Spear
                    return WeaponTypes.Spear;
                case "mace": // Mace
                    return WeaponTypes.Mace;
                case "axe": // Axe
                    return WeaponTypes.Axe;
                case "flail": // Flail
                    return WeaponTypes.Flail;
                case "whip": // Whip
                    return WeaponTypes.Whip;
                case "polearm": // Polearm
                    return WeaponTypes.Polearm;
                case "staff(weapon)": // Staff
                    return WeaponTypes.Staff;
                case "arrow": // Arrow
                    return WeaponTypes.Exotic;
                case "ranged": // Ranged
                    return WeaponTypes.Exotic;
            }
            return WeaponTypes.Exotic;
        }

        private static readonly (string name, string noun, SchoolTypes damType)[] AttackTable =
        {
            (   "none",     "hit",      SchoolTypes.None      ),  /*  0 */
            (   "slice",    "slice",    SchoolTypes.Slash   ),
            (   "stab",     "stab",     SchoolTypes.Pierce  ),
            (   "slash",    "slash",    SchoolTypes.Slash   ),
            (   "whip",     "whip",     SchoolTypes.Slash   ),
            (   "claw",     "claw",     SchoolTypes.Slash   ),  /*  5 */
            (   "blast",    "blast",    SchoolTypes.Bash    ),
            (   "pound",    "pound",    SchoolTypes.Bash    ),
            (   "crush",    "crush",    SchoolTypes.Bash    ),
            (   "grep",     "grep",     SchoolTypes.Slash   ),
            (   "bite",     "bite",     SchoolTypes.Pierce  ),  /* 10 */
            (   "pierce",   "pierce",   SchoolTypes.Pierce  ),
            (   "suction",  "suction",  SchoolTypes.Bash    ),
            (   "beating",  "beating",  SchoolTypes.Bash    ),
            (   "digestion",    "digestion",    SchoolTypes.Acid    ),
            (   "charge",   "charge",   SchoolTypes.Bash    ),  /* 15 */
            (   "slap",     "slap",     SchoolTypes.Bash    ),
            (   "punch",    "punch",    SchoolTypes.Bash    ),
            (   "wrath",    "wrath",    SchoolTypes.Energy  ),
            (   "magic",    "magic",    SchoolTypes.Energy  ),
            (   "divine",   "divine power", SchoolTypes.Holy    ),  /* 20 */
            (   "cleave",   "cleave",   SchoolTypes.Slash   ),
            (   "scratch",  "scratch",  SchoolTypes.Pierce  ),
            (   "peck",     "peck",     SchoolTypes.Pierce  ),
            (   "peckb",    "peck",     SchoolTypes.Bash    ),
            (   "chop",     "chop",     SchoolTypes.Slash   ),  /* 25 */
            (   "sting",    "sting",    SchoolTypes.Pierce  ),
            (   "smash",     "smash",   SchoolTypes.Bash    ),
            (   "shbite",   "shocking bite",SchoolTypes.Lightning   ),
            (   "flbite",   "flaming bite", SchoolTypes.Fire    ),
            (   "frbite",   "freezing bite", SchoolTypes.Cold   ),  /* 30 */
            (   "acbite",   "acidic bite",  SchoolTypes.Acid    ),
            (   "chomp",    "chomp",    SchoolTypes.Pierce  ),
            (   "drain",    "life drain",   SchoolTypes.Negative    ),
            (   "thrust",   "thrust",   SchoolTypes.Pierce  ),
            (   "slime",    "slime",    SchoolTypes.Acid    ),
            (   "shock",    "shock",    SchoolTypes.Lightning   ),
            (   "thwack",   "thwack",   SchoolTypes.Bash    ),
            (   "flame",    "flame",    SchoolTypes.Fire    ),
            (   "chill",    "chill",    SchoolTypes.Cold    ),
        };

        private static (SchoolTypes schoolType, WeaponFlags weaponFlags, string damageNoun) ConvertWeaponDamageType(object attackTableValue, object weaponType2Value)
        {
            string attackTable = (string)attackTableValue;
            SchoolTypes schoolType = SchoolTypes.None;
            string damageNoun = attackTable;
            (string name, string noun, SchoolTypes damType) attackTableEntry = AttackTable.FirstOrDefault(x => x.name == attackTable);
            if (!attackTableEntry.Equals(default))
            {
                schoolType = attackTableEntry.damType;
                damageNoun = attackTableEntry.noun;
            }

            int weaponType2 = weaponType2Value == null ? 0 : Convert.ToInt32(weaponType2Value);
            WeaponFlags weaponFlags = WeaponFlags.None;
            if ((weaponType2 & MysteryImporter.A) == MysteryImporter.A) // Flaming
                weaponFlags |= WeaponFlags.Flaming;
            if ((weaponType2 & MysteryImporter.B) == MysteryImporter.B) // Frost
                weaponFlags |= WeaponFlags.Frost;
            if ((weaponType2 & MysteryImporter.C) == MysteryImporter.C) // Vampiric
                weaponFlags |= WeaponFlags.Vampiric;
            if ((weaponType2 & MysteryImporter.D) == MysteryImporter.D) // Sharp
                weaponFlags |= WeaponFlags.Sharp;
            if ((weaponType2 & MysteryImporter.E) == MysteryImporter.E) // Vorpal
                weaponFlags |= WeaponFlags.Vorpal;
            if ((weaponType2 & MysteryImporter.F) == MysteryImporter.F) // Two-hands
                weaponFlags |= WeaponFlags.TwoHands;
            if ((weaponType2 & MysteryImporter.G) == MysteryImporter.G) // Shocking
                weaponFlags |= WeaponFlags.Shocking;
            if ((weaponType2 & MysteryImporter.H) == MysteryImporter.H) // Poison
                weaponFlags |= WeaponFlags.Poison;
            if ((weaponType2 & MysteryImporter.I) == MysteryImporter.I) // Holy
                weaponFlags |= WeaponFlags.Holy;
            // J: Weighted
            // K: Necrotism

            //
            return (schoolType, weaponFlags, damageNoun);
        }

        private static (ItemFlags, bool) ConvertMysteryItemExtraFlags(ObjectData data)
        {
            ItemFlags itemFlags = ItemFlags.None;

            bool noTake = false;
            if ((data.WearFlags & MysteryImporter.A) != MysteryImporter.A) noTake = true; // WearFlags A means TAKE

            if ((data.ExtraFlags & MysteryImporter.A) == MysteryImporter.A) itemFlags |= ItemFlags.Glowing; // A ITEM_GLOW
            if ((data.ExtraFlags & MysteryImporter.B) == MysteryImporter.B) itemFlags |= ItemFlags.Humming; // B ITEM_HUM
            if ((data.ExtraFlags & MysteryImporter.C) == MysteryImporter.C) itemFlags |= ItemFlags.Dark; // C ITEM_DARK
            if ((data.ExtraFlags & MysteryImporter.D) == MysteryImporter.D) itemFlags |= ItemFlags.Lock; // D ITEM_LOCK
            if ((data.ExtraFlags & MysteryImporter.E) == MysteryImporter.E) itemFlags |= ItemFlags.Evil; // E ITEM_EVIL
            if ((data.ExtraFlags & MysteryImporter.F) == MysteryImporter.F) itemFlags |= ItemFlags.Invis; // F ITEM_INVIS
            if ((data.ExtraFlags & MysteryImporter.G) == MysteryImporter.G) itemFlags |= ItemFlags.Magic; // G ITEM_MAGIC
            if ((data.ExtraFlags & MysteryImporter.H) == MysteryImporter.H) itemFlags |= ItemFlags.NoDrop; // H ITEM_NODROP
            if ((data.ExtraFlags & MysteryImporter.I) == MysteryImporter.I) itemFlags |= ItemFlags.Bless; // I ITEM_BLESS
            if ((data.ExtraFlags & MysteryImporter.J) == MysteryImporter.J) itemFlags |= ItemFlags.AntiGood; // J ITEM_ANTI_GOOD
            if ((data.ExtraFlags & MysteryImporter.K) == MysteryImporter.K) itemFlags |= ItemFlags.AntiEvil; // K ITEM_ANTI_EVIL
            if ((data.ExtraFlags & MysteryImporter.L) == MysteryImporter.L) itemFlags |= ItemFlags.AntiNeutral; // L ITEM_ANTI_NEUTRAL
            if ((data.ExtraFlags & MysteryImporter.M) == MysteryImporter.M) itemFlags |= ItemFlags.NoRemove; // M ITEM_NOREMOVE
            if ((data.ExtraFlags & MysteryImporter.N) == MysteryImporter.N) itemFlags |= ItemFlags.Inventory; // N ITEM_INVENTORY
            if ((data.ExtraFlags & MysteryImporter.O) == MysteryImporter.O) itemFlags |= ItemFlags.NoPurge; // O ITEM_NOPURGE
            if ((data.ExtraFlags & MysteryImporter.P) == MysteryImporter.P) itemFlags |= ItemFlags.RotDeath; // P ITEM_ROT_DEATH
            if ((data.ExtraFlags & MysteryImporter.Q) == MysteryImporter.Q) itemFlags |= ItemFlags.VisibleDeath; // Q ITEM_VIS_DEATH
            //if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.R) == Importer.Mystery.MysteryImporter.R) itemFlags |= ItemFlags.; // R ITEM_DONATED
            if ((data.ExtraFlags & MysteryImporter.S) == MysteryImporter.S) itemFlags |= ItemFlags.NonMetal; // S NONMETAL
            if ((data.ExtraFlags & MysteryImporter.T) == MysteryImporter.T) itemFlags |= ItemFlags.NoLocate; // T ITEM_NOLOCATE
            if ((data.ExtraFlags & MysteryImporter.U) == MysteryImporter.U) itemFlags |= ItemFlags.MeltOnDrop; // U ITEM_MELT_DROP
            if ((data.ExtraFlags & MysteryImporter.V) == MysteryImporter.V) itemFlags |= ItemFlags.HadTimer; // V ITEM_HAD_TIMER
            if ((data.ExtraFlags & MysteryImporter.W) == MysteryImporter.W) itemFlags |= ItemFlags.SellExtract; // W ITEM_SELL_EXTRACT
            // X ITEM_NOSAC
            if ((data.ExtraFlags & MysteryImporter.Y) == MysteryImporter.Y) itemFlags |= ItemFlags.BurnProof; // Y ITEM_BURN_PROOF
            if ((data.ExtraFlags & MysteryImporter.Z) == MysteryImporter.Z) itemFlags |= ItemFlags.NoUncurse; // Z ITEM_NOUNCURSE
            // aa ITEM_NOIDENT
            //if ((data.ExtraFlags & Importer.Mystery.MysteryImporter.bb) == Importer.Mystery.MysteryImporter.bb) itemFlags |= ItemFlags.Indestructible; // bb ITEM_NOCOND

            return (itemFlags, noTake);
        }

        private static void CreateWorld()
        {
            string path =  DependencyContainer.Current.GetInstance<ISettings>().ImportAreaPath;

            MysteryImporter mysteryImporter = new MysteryImporter();
            mysteryImporter.Load(System.IO.Path.Combine(path, "limbo.are"));
            mysteryImporter.Parse();
            mysteryImporter.Load(System.IO.Path.Combine(path, "midgaard.are"));
            mysteryImporter.Parse();
            mysteryImporter.Load(System.IO.Path.Combine(path, "hitower.are"));
            mysteryImporter.Parse();
            //mysteryImporter.Load(System.IO.Path.Combine(path, "amazon.are"));
            //mysteryImporter.Parse();

            //string fileList = System.IO.Path.Combine(path, "area.lst");
            //string[] areaFilenames = System.IO.File.ReadAllLines(fileList);
            //foreach (string areaFilename in areaFilenames)//.Where(x => !x.Contains("limbo")))
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
            foreach (RoomData importedRoom in mysteryImporter.Rooms)
                CreateRoomBlueprint(importedRoom);
            // Create Character blueprints
            foreach (MobileData mobile in mysteryImporter.Mobiles)
                CreateCharacterBlueprint(mobile);
            // Create Item blueprints
            foreach (ObjectData obj in mysteryImporter.Objects)
                CreateItemBlueprint(obj);
            // Create Areas
            foreach (AreaData importedArea in mysteryImporter.Areas)
            {
                // TODO: levels
                IArea area = World.AddArea(Guid.NewGuid(), importedArea.Name, 1, 99, importedArea.Builders, importedArea.Credits);
                areasByVnums.Add(importedArea.VNum, area);
            }

            // Create Rooms
            foreach (RoomData importedRoom in mysteryImporter.Rooms)
            {
                IArea area = areasByVnums[importedRoom.AreaVnum];
                IRoom room = World.AddRoom(Guid.NewGuid(), World.GetRoomBlueprint(importedRoom.VNum), area);
                roomsByVNums.Add(importedRoom.VNum, room);
            }

            // Create Exits
            foreach (RoomData importedRoom in mysteryImporter.Rooms)
            {
                for (int i = 0; i < RoomData.MaxExits - 1; i++)
                {
                    ExitData exit = importedRoom.Exits[i];
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

            // Create Resets
            foreach (RoomData importedRoom in mysteryImporter.Rooms.Where(x => x.Resets.Any()))
            {
                RoomBlueprint room = World.GetRoomBlueprint(importedRoom.VNum);
                foreach (ResetData reset in importedRoom.Resets)
                {
                    switch (reset.Command)
                    {
                        case 'M':
                            Debug.Assert(reset.Arg3 == room.Id, $"Reset arg3 '{reset.Arg3}' should be equal to room id '{room.Id}'.");
                            room.Resets.Add(new CharacterReset
                            {
                                RoomBlueprintId = room.Id,
                                CharacterId = reset.Arg1,
                                GlobalLimit = reset.Arg2,
                                LocalLimit = reset.Arg4
                            });
                            break;
                        case 'O':
                            Debug.Assert(reset.Arg3 == room.Id, $"Reset arg3 '{reset.Arg3}' should be equal to room id '{room.Id}'.");
                            room.Resets.Add(new ItemInRoomReset
                            {
                                RoomBlueprintId = room.Id,
                                ItemId = reset.Arg1,
                                GlobalLimit = reset.Arg2,
                                LocalLimit = reset.Arg4,
                            });
                            break;
                        case 'P':
                            room.Resets.Add(new ItemInItemReset 
                            {
                                RoomBlueprintId = room.Id,
                                ItemId = reset.Arg1,
                                ContainerId = reset.Arg3,
                                GlobalLimit = reset.Arg2,
                                LocalLimit = reset.Arg4,
                            });
                            break;
                        case 'G':
                            room.Resets.Add(new ItemInCharacterReset 
                            {
                                RoomBlueprintId = room.Id,
                                ItemId = reset.Arg1,
                                GlobalLimit = reset.Arg2,
                            });
                            break;
                        case 'E':
                            room.Resets.Add(new ItemInEquipmentReset
                            {
                                RoomBlueprintId = room.Id,
                                ItemId = reset.Arg1,
                                EquipmentSlot = ConvertWearLocation(reset.Arg3), // bypass bug in Mystery area
                                GlobalLimit = reset.Arg2,
                            });
                            break;
                    }
                    // TODO: D, R, Z
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

            // MANDATORY ITEM
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint
            {
                Id = DependencyContainer.Current.GetInstance<ISettings>().CorpseBlueprintId,
                NoTake = true,
                Name = "corpse"
            }; // this is mandatory
            World.AddItemBlueprint(corpseBlueprint);
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
            //item2.ChangeEquippedBy(mob2);

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
