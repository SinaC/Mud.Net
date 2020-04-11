using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Mud.Container;
using Mud.Datas;
using Mud.Importer.Mystery;
using Mud.Logger;
using Mud.Network;
using Mud.Network.Telnet;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Constants;
using Mud.Server.Item;
using Mud.Server.Server;

namespace Mud.Server.WPFTestApplication
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window, INetworkServer
    {
        private static ServerWindow _serverWindowInstance;

        public ServerWindow()
        {
            _serverWindowInstance = this;

            DependencyContainer.Instance.Register<IWorld, World.World>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Instance.Register<IServer, Server.Server>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Instance.Register<IAbilityManager, Abilities.AbilityManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Instance.Register<IClassManager, Classes.ClassManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Instance.Register<IRaceManager, Races.RaceManager>(SimpleInjector.Lifestyle.Singleton);

            DependencyContainer.Instance.Register<ILoginManager, Datas.Filesystem.LoginManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Instance.Register<IPlayerManager, Datas.Filesystem.PlayerManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Instance.Register<IAdminManager, Datas.Filesystem.AdminManager>(SimpleInjector.Lifestyle.Singleton);

            InitializeComponent();

            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], "server.log");

            TestLootTable();

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
            INetworkServer telnetServer = new TelnetServer(11000);
            DependencyContainer.Instance.GetInstance<IServer>().Initialize(new List<INetworkServer> { telnetServer, this });
            DependencyContainer.Instance.GetInstance<IServer>().Start();

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
                Container.DependencyContainer.Instance.GetInstance<IServer>().Stop();
                Application.Current.Shutdown();
            }
            else if (input == "alist")
            {
                OutputText("Admins:");
                foreach (IAdmin a in Container.DependencyContainer.Instance.GetInstance<IServer>().Admins)
                    OutputText(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
            }
            else if (input == "plist")
            {
                OutputText("players:");
                foreach (IPlayer p in Container.DependencyContainer.Instance.GetInstance<IServer>().Players)
                    OutputText(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
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
            DependencyContainer.Instance.GetInstance<IWorld>().AddRoomBlueprint(blueprint);
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
            DependencyContainer.Instance.GetInstance<IWorld>().AddRoomBlueprint(blueprint);
            return blueprint;
        }

        private static CharacterBlueprint CreateCharacterBlueprint(Importer.Mystery.MobileData data)
        {
            Sex sex = Sex.Neutral;
            if (data.Sex.ToLower() == "female")
                sex = Sex.Female;
            else if (data.Sex.ToLower() == "male")
                sex = Sex.Male;
            CharacterBlueprint blueprint = new CharacterBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                Level = data.Level,
                LongDescription = data.LongDescr,
                ShortDescription = data.ShortDescr,
                Sex = sex,
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddCharacterBlueprint(blueprint);
            return blueprint;
        }
        private static CharacterBlueprint CreateCharacterBlueprint(Importer.Rom.MobileData data)
        {
            Sex sex = Sex.Neutral;
            if (data.Sex.ToLower() == "female")
                sex = Sex.Female;
            else if (data.Sex.ToLower() == "male")
                sex = Sex.Male;
            CharacterBlueprint blueprint = new CharacterBlueprint
            {
                Id = data.VNum,
                Name = data.Name,
                Description = data.Description,
                Level = data.Level,
                LongDescription = data.LongDescr,
                ShortDescription = data.ShortDescr,
                Sex = sex,
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddCharacterBlueprint(blueprint);
            return blueprint;
        }

        private long ConvertToLong(char c)
        {
            return (long)1 << (c - 48);
        }

        private static ExitFlags ConvertExitInfo(long exitInfo)
        {
            ExitFlags flags = 0;
            if ((exitInfo & MysteryImporter.A) == MysteryImporter.A)
                flags |= ExitFlags.Door;
            if ((exitInfo & MysteryImporter.B) == MysteryImporter.B)
                flags |= ExitFlags.Closed;
            if ((exitInfo & MysteryImporter.B) == MysteryImporter.B)
                flags |= ExitFlags.Locked;
            if ((exitInfo & MysteryImporter.H) == MysteryImporter.H)
                flags |= ExitFlags.Easy;
            if ((exitInfo & MysteryImporter.I) == MysteryImporter.I)
                flags |= ExitFlags.Hard;
            if ((exitInfo & MysteryImporter.M) == MysteryImporter.M)
                flags |= ExitFlags.Hidden;
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
                case MysteryImporter.B: // B finger
                    return WearLocations.Ring;
                case 1 << 2: // C
                    return WearLocations.Amulet;
                case 1 << 3: // D
                    return WearLocations.Chest;
                case 1 << 4: // E
                    return WearLocations.Head;
                case 1 << 5: // F
                    return WearLocations.Legs;
                case 1 << 6: // G
                    return WearLocations.Feet;
                case 1 << 7: // H
                    return WearLocations.Hands;
                case 1 << 8: // I
                    return WearLocations.Arms;
                case 1 << 9: // J
                    return WearLocations.Shield;
                case 1 << 10: // K
                    return WearLocations.Cloak;
                case 1 << 11: // L
                    return WearLocations.Waist;
                case 1 << 12: // M
                    return WearLocations.Wrists;
                case 1 << 13: // N
                    return WearLocations.Wield;
                case 1 << 14: // O
                    return WearLocations.Hold;
                //case 1 << 15: // Q float
                //case 1 << 16: // R ear
                case 1 << 17: // S
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
                case MysteryImporter.B: // B finger
                    return WearLocations.Ring;
                case 1 << 2: // C
                    return WearLocations.Amulet;
                case 1 << 3: // D
                    return WearLocations.Chest;
                case 1 << 4: // E
                    return WearLocations.Head;
                case 1 << 5: // F
                    return WearLocations.Legs;
                case 1 << 6: // G
                    return WearLocations.Feet;
                case 1 << 7: // H
                    return WearLocations.Hands;
                case 1 << 8: // I
                    return WearLocations.Arms;
                case 1 << 9: // J
                    return WearLocations.Shield;
                case 1 << 10: // K
                    return WearLocations.Cloak;
                case 1 << 11: // L
                    return WearLocations.Waist;
                case 1 << 12: // M
                    return WearLocations.Wrists;
                case 1 << 13: // N
                    return WearLocations.Wield;
                case 1 << 14: // O
                    return WearLocations.Hold;
                //case 1 << 15: // Q float
                //case 1 << 16: // R ear
                case 1 << 17: // S
                    return WearLocations.Head; // eyes
                default:
                    return WearLocations.None;
            }
        }

        private static ItemBlueprintBase CreateItemBlueprint(Importer.Mystery.ObjectData data)
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
                    Armor = Convert.ToInt32(data.Values[0]) + Convert.ToInt32(data.Values[1]) + Convert.ToInt32(data.Values[2]) + Convert.ToInt32(data.Values[3]), // TODO
                    ArmorKind = ArmorKinds.Leather // TODO
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
                    ResourceBonus = Convert.ToInt32(data.Values[4])
                };
            }
            else if (data.ItemType == "fountain")
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
                    MaxPeople = 0,
                    MaxWeight = 0,
                    HealBonus = 0,
                    ResourceBonus = 0
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
                    WearLocation = ConvertWearLocation(data)
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
                    WearLocation = ConvertWearLocation(data)
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
                    Destination = Convert.ToInt32(data.Values[3])
                };
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, $"ItemBlueprint cannot be created: [{data.VNum}] [{data.ItemType}] [{data.WearFlags}] : {data.Name}");
                // TODO: other item type
                blueprint = null;
            }
            if (blueprint != null)
                DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(blueprint);
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
                    Armor = Convert.ToInt32(data.Values[0]) + Convert.ToInt32(data.Values[1]) + Convert.ToInt32(data.Values[2]) + Convert.ToInt32(data.Values[3]), // TODO
                    ArmorKind = ArmorKinds.Leather // TODO
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
                    ResourceBonus = Convert.ToInt32(data.Values[4])
                };
            }
            else if (data.ItemType == "fountain")
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
                    MaxPeople = 0,
                    MaxWeight = 0,
                    HealBonus = 0,
                    ResourceBonus = 0
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
                    WearLocation = ConvertWearLocation(data)
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
                    WearLocation = ConvertWearLocation(data)
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
                    Destination = Convert.ToInt32(data.Values[3])
                };
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, $"ItemBlueprint cannot be created: [{data.VNum}] [{data.ItemType}] [{data.WearFlags}] : {data.Name}");
                // TODO: other item type
                blueprint = null;
            }
            if (blueprint != null)
                DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(blueprint);
            return blueprint;
        }

        private static void CreateWorld()
        {
            string path = ConfigurationManager.AppSettings["ImportAreaPath"];

            MysteryImporter mysteryImporter = new MysteryImporter();
            mysteryImporter.Load(System.IO.Path.Combine(path, "midgaard.are"));
            mysteryImporter.Parse();
            mysteryImporter.Load(System.IO.Path.Combine(path, "amazon.are"));
            mysteryImporter.Parse();
            //MysteryImporter importer = new MysteryImporter();
            //string path = @"D:\GitHub\OldMud\area";
            //string fileList = Path.Combine(path, "area.lst");
            //string[] areaFilenames = File.ReadAllLines(fileList);
            //foreach (string areaFilename in areaFilenames.Where(x => !x.Contains("limbo")))
            //{
            //    if (areaFilename.Contains("$"))
            //        break;
            //    string areaFullName = Path.Combine(path, areaFilename);
            //    importer.Load(areaFullName);
            //    importer.Parse();
            //}


            foreach (KeyValuePair<string,int> kv in mysteryImporter.Objects.GroupBy(o => o.ItemType).ToDictionary(g => g.Key, g => g.Count()).OrderBy(x => x.Value))
                Log.Default.WriteLine(LogLevels.Info, "{0} -> {1}", kv.Key, kv.Value);

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
                IArea area = DependencyContainer.Instance.GetInstance<IWorld>().AddArea(Guid.NewGuid(), importedArea.Name, 1, 99, importedArea.Builders, importedArea.Credits);
                areasByVnums.Add(importedArea.VNum, area);
            }

            // Create Rooms
            foreach (Importer.Mystery.RoomData importedRoom in mysteryImporter.Rooms)
            {
                IArea area = areasByVnums[importedRoom.AreaVnum];
                IRoom room = DependencyContainer.Instance.GetInstance<IWorld>().AddRoom(Guid.NewGuid(), DependencyContainer.Instance.GetInstance<IWorld>().GetRoomBlueprint(importedRoom.VNum), area);
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
                            DependencyContainer.Instance.GetInstance<IWorld>().AddExit(from, to, exitBlueprint, (ExitDirections)i);
                        }
                    }
                }
            }

            // Handle resets
            // TODO: handle rom resets
            ICharacter lastCharacter = null;
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
                            CharacterBlueprint blueprint = DependencyContainer.Instance.GetInstance<IWorld>().GetCharacterBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                lastCharacter = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), blueprint, room);
                                Log.Default.WriteLine(LogLevels.Debug, $"Room {importedRoom.VNum}: M: Mob {reset.Arg1} added");
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Warning, $"Room {importedRoom.VNum}: M: Mob {reset.Arg1} not found");
                            break;
                        }
                        case 'O':
                        {
                            ItemBlueprintBase blueprint = DependencyContainer.Instance.GetInstance<IWorld>().GetItemBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                IItem item = DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), blueprint.Id, room);
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
                            //        ItemWeaponBlueprint blueprint = DependencyContainer.Instance.GetInstance<IWorld>().GetItemBlueprint(reset.Arg1) as ItemWeaponBlueprint;
                            //        DependencyContainer.Instance.GetInstance<IWorld>().AddItemWeapon(Guid.NewGuid(), blueprint, room);
                            //        Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: Weapon {reset.Arg1} added on floor");
                            //    }
                            //    else if (obj.ItemType == "container")
                            //    {
                            //        ItemContainerBlueprint blueprint = DependencyContainer.Instance.GetInstance<IWorld>().GetItemBlueprint(reset.Arg1) as ItemContainerBlueprint;
                            //        lastContainer = DependencyContainer.Instance.GetInstance<IWorld>().AddItemContainer(Guid.NewGuid(), blueprint, room);
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
                            ItemBlueprintBase blueprint = DependencyContainer.Instance.GetInstance<IWorld>().GetItemBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                if (lastContainer != null)
                                {
                                    DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), blueprint.Id, lastContainer);
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
                            ItemBlueprintBase blueprint = DependencyContainer.Instance.GetInstance<IWorld>().GetItemBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                if (lastCharacter != null)
                                {
                                        DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
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
                                ItemBlueprintBase blueprint = DependencyContainer.Instance.GetInstance<IWorld>().GetItemBlueprint(reset.Arg1);
                                if (blueprint != null)
                                {
                                    if (lastCharacter != null)
                                    {
                                        DependencyContainer.Instance.GetInstance<IWorld>().AddItem(Guid.NewGuid(), blueprint.Id, lastCharacter);
                                        Log.Default.WriteLine(LogLevels.Debug, $"Room {importedRoom.VNum}: E: Obj {reset.Arg1} added on {lastCharacter.Blueprint.Id}");
                                        // TODO: try to equip
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
                            // TODO: other command  P, E, G, D, R, Z
                    }
                }
            }

            CharacterBlueprint mob2Blueprint = new CharacterBlueprint
            {
                Id = 2,
                Name = "mob2",
                ShortDescription = "Second mob (female)",
                Description = "Second mob (female) is here",
                Sex = Sex.Female,
                Level = 10
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddCharacterBlueprint(mob2Blueprint);
            CharacterBlueprint mob3Blueprint = new CharacterBlueprint
            {
                Id = 3,
                Name = "mob3",
                ShortDescription = "Third mob (male)",
                Description = "Third mob (male) is here",
                Sex = Sex.Male,
                Level = 10
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddCharacterBlueprint(mob3Blueprint);
            //CharacterBlueprint mob4Blueprint = new CharacterBlueprint
            //{
            //    Id = 4,
            //    Name = "mob4",
            //    ShortDescription = "Fourth mob (neutral)",
            //    Description = "Fourth mob (neutral) is here",
            //    Sex = Sex.Neutral,
            //    Level = 10
            //};
            CharacterBlueprint mob5Blueprint = new CharacterBlueprint
            {
                Id = 5,
                Name = "mob5",
                ShortDescription = "Fifth mob (female)",
                Description = "Fifth mob (female) is here",
                Sex = Sex.Female,
                Level = 10
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddCharacterBlueprint(mob5Blueprint);

            ItemContainerBlueprint item1Blueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "item1 first",
                ShortDescription = "First item (container)",
                Description = "The first item (container) has been left here.",
                ItemCount = 10,
                WeightMultiplier = 100
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(item1Blueprint);
            ItemWeaponBlueprint item2Blueprint = new ItemWeaponBlueprint
            {
                Id = 2,
                Name = "item2 second",
                ShortDescription = "Second item (weapon)",
                Description = "The second item (weapon) has been left here.",
                Type = WeaponTypes.Axe1H,
                DiceCount = 10,
                DiceValue = 20,
                DamageType = SchoolTypes.Fire,
                WearLocation = WearLocations.Wield
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(item2Blueprint);
            ItemArmorBlueprint item3Blueprint = new ItemArmorBlueprint
            {
                Id = 3,
                Name = "item3 third",
                ShortDescription = "Third item (armor|feet)",
                Description = "The third item (armor|feet) has been left here.",
                Armor = 100,
                ArmorKind = ArmorKinds.Mail,
                WearLocation = WearLocations.Feet
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(item3Blueprint);
            ItemLightBlueprint item4Blueprint = new ItemLightBlueprint
            {
                Id = 4,
                Name = "item4 fourth",
                ShortDescription = "Fourth item (light)",
                Description = "The fourth item (light) has been left here.",
                DurationHours = -1,
                WearLocation = WearLocations.Light
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(item4Blueprint);
            ItemWeaponBlueprint item5Blueprint = new ItemWeaponBlueprint
            {
                Id = 5,
                Name = "item5 fifth",
                ShortDescription = "Fifth item (weapon)",
                Description = "The fifth item (weapon) has been left here.",
                Type = WeaponTypes.Sword1H,
                DiceCount = 5,
                DiceValue = 40,
                DamageType = SchoolTypes.Physical,
                WearLocation = WearLocations.Wield
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(item5Blueprint);
            ItemWeaponBlueprint item6Blueprint = new ItemWeaponBlueprint
            {
                Id = 6,
                Name = "item6 sixth",
                ShortDescription = "Sixth item (weapon)",
                Description = "The sixth item (weapon) has been left here.",
                Type = WeaponTypes.Mace2H,
                DiceCount = 10,
                DiceValue = 20,
                DamageType = SchoolTypes.Holy,
                WearLocation = WearLocations.Wield
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(item6Blueprint);
            ItemShieldBlueprint item7Blueprint = new ItemShieldBlueprint
            {
                Id = 7,
                Name = "item7 seventh",
                ShortDescription = "Seventh item (shield)",
                Description = "The seventh item (shield) has been left here.",
                Armor = 1000,
                WearLocation = WearLocations.Shield
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(item7Blueprint);
            ItemQuestBlueprint questItem1Blueprint = new ItemQuestBlueprint
            {
                Id = 8,
                Name = "Quest item 1",
                ShortDescription = "Quest item 1",
                Description = "The quest item 1 has been left here."
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(questItem1Blueprint);
            ItemQuestBlueprint questItem2Blueprint = new ItemQuestBlueprint
            {
                Id = 9,
                Name = "Quest item 2",
                ShortDescription = "Quest item 2",
                Description = "The quest item 2 has been left here."
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemBlueprint(questItem2Blueprint);

            //
            ServerOptions.CorpseBlueprint = new ItemCorpseBlueprint
            {
                Name = "corpse"
            }; // this is mandatory

            // Add dummy mobs and items to allow impersonate :)
            IRoom templeOfMota = DependencyContainer.Instance.GetInstance<IWorld>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
            IRoom templeSquare = DependencyContainer.Instance.GetInstance<IWorld>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the temple square");

            //ICharacter mob1 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "mob1", Repository.ClassManager["Druid"], Repository.RaceManager["Insectoid"], Sex.Male, templeOfMota); // playable
            ICharacter mob2 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
            ICharacter mob3 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
            //ICharacter mob4 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
            //ICharacter mob4 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), "mob4", Repository.ClassManager["Warrior"], Repository.RaceManager["Dwarf"], Sex.Female, templeSquare); // playable
            ICharacter mob5 = DependencyContainer.Instance.GetInstance<IWorld>().AddCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

            IItemContainer item1 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItemContainer item1Dup1 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItemWeapon item2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemWeapon(Guid.NewGuid(), item2Blueprint, mob2);
            IItemArmor item3 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemArmor(Guid.NewGuid(), item3Blueprint, item1Dup1);
            //IItemLight item4 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemLight(Guid.NewGuid(), item4Blueprint, mob1);
            //IItemWeapon item5 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemWeapon(Guid.NewGuid(), item5Blueprint, mob1);
            //IItemContainer item1Dup2 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemContainer(Guid.NewGuid(), item1Blueprint, mob1);
            IItemArmor item3Dup1 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemArmor(Guid.NewGuid(), item3Blueprint, mob3);
            //IItemLight item4Dup1 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemLight(Guid.NewGuid(), item4Blueprint, mob4);
            IItemWeapon item6 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemWeapon(Guid.NewGuid(), item6Blueprint, templeSquare);
            IItemShield item7 = DependencyContainer.Instance.GetInstance<IWorld>().AddItemShield(Guid.NewGuid(), item7Blueprint, templeOfMota);
            DependencyContainer.Instance.GetInstance<IWorld>().AddItemQuest(Guid.NewGuid(), questItem2Blueprint, templeSquare);

            // Equip weapon on mob2
            mob2.Equipments.First(x => x.Slot == EquipmentSlots.Wield).Item = item2;
            item2.ChangeContainer(null);
            item2.ChangeEquipedBy(mob2);

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
                KillObjectives = new List<QuestKillObjectiveBlueprint>
                {
                    new QuestKillObjectiveBlueprint
                    {
                        CharacterBlueprintId = 3062, // fido
                        Count = 3
                    }
                },
                ItemObjectives = new List<QuestItemObjectiveBlueprint>
                {
                    new QuestItemObjectiveBlueprint
                    {
                        ItemBlueprintId = questItem2Blueprint.Id,
                        Count = 1
                    },
                    new QuestItemObjectiveBlueprint
                    {
                        ItemBlueprintId = questItem1Blueprint.Id,
                        Count = 2
                    }
                },
                LocationObjectives = new List<QuestLocationObjectiveBlueprint>
                {
                    new QuestLocationObjectiveBlueprint
                    {
                        RoomBlueprintId = templeSquare.Blueprint.Id,
                    }
                },
                KillLootTable = new Dictionary<int, QuestKillLootTable<int>> // when killing mob 3065, we receive quest item 1 (80%)
                {
                    { 3065, quest1KillLoot } // beggar
                }
                // TODO: rewards
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddQuestBlueprint(questBlueprint1);

            QuestBlueprint questBlueprint2 = new QuestBlueprint
            {
                Id = 2,
                Title = "Simple exploration quest",
                Description = "Explore common square",
                Level = 10,
                Experience = 10000,
                Gold = 20,
                LocationObjectives = new List<QuestLocationObjectiveBlueprint>
                {
                    new QuestLocationObjectiveBlueprint
                    {
                        RoomBlueprintId = DependencyContainer.Instance.GetInstance<IWorld>().Rooms.FirstOrDefault(x => x.Name.ToLower() == "the common square")?.Blueprint.Id ?? 0
                    }
                },
                // TODO: rewards
            };
            DependencyContainer.Instance.GetInstance<IWorld>().AddQuestBlueprint(questBlueprint2);

            // Give quest 1 and 2 to mob1
            //IQuest quest1 = new Quest.Quest(questBlueprint1, mob1, mob2);
            //mob1.AddQuest(quest1);
            //IQuest quest2 = new Quest.Quest(questBlueprint2, mob1, mob2);
            //mob1.AddQuest(quest2);

            //// Search extra description
            //foreach(IRoom room in DependencyContainer.Instance.GetInstance<IWorld>().Rooms.Where(x => x.ExtraDescriptions != null && x.ExtraDescriptions.Any()))
            //    Log.Default.WriteLine(LogLevels.Info, "Room {0} has extra description: {1}", room.DebugName, room.ExtraDescriptions.Keys.Aggregate((n,i) => n+","+i));
            //foreach (IItem item in DependencyContainer.Instance.GetInstance<IWorld>().Items.Where(x => x.ExtraDescriptions != null && x.ExtraDescriptions.Any()))
            //    Log.Default.WriteLine(LogLevels.Info, "Item {0} has extra description: {1}", item.DebugName, item.ExtraDescriptions.Keys.Aggregate((n, i) => n + "," + i));
        }
    }
}
