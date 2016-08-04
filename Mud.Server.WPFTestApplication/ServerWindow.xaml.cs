using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Mud.Importer.Mystery;
using Mud.Logger;
using Mud.Network;
using Mud.Network.Telnet;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Item;
using Mud.Server.Server;
using System.IO;

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

            InitializeComponent();

            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], "server.log");

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            CreateMidgaard();

            //
            INetworkServer telnetServer = new TelnetServer(11000);
            Repository.Server.Initialize(new List<INetworkServer> { telnetServer, this });
            Repository.Server.Start();

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
                Repository.Server.Stop();
                Application.Current.Shutdown();
            }
            else if (input == "alist")
            {
                OutputText("Admins:");
                foreach (IAdmin a in Repository.Server.GetAdmins())
                    OutputText(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.Name : "") + " " + (a.Incarnating != null ? a.Incarnating.Name : ""));
            }
            else if (input == "plist")
            {
                OutputText("players:");
                foreach (IPlayer p in Repository.Server.GetPlayers())
                    OutputText(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.Name : ""));
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
            };
            Repository.World.AddRoomBlueprint(blueprint);
            return blueprint;
        }

        private static CharacterBlueprint CreateCharacterBlueprint(MobileData data)
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
            Repository.World.AddCharacterBlueprint(blueprint);
            return blueprint;
        }

        private long ConvertToLong(char c)
        {
            return (long)1 << (c - 48);
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
                //case 1 << 1: // B finger
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

        private static ItemBlueprintBase CreateItemBlueprint(ObjectData data)
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
                    ExtraDescriptions = data.ExtraDescr,
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
                    ExtraDescriptions = data.ExtraDescr,
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
                    ExtraDescriptions = data.ExtraDescr,
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
                    ExtraDescriptions = data.ExtraDescr,
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
                    ExtraDescriptions = data.ExtraDescr,
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    MaxPeople = Convert.ToInt32(data.Values[0]),
                    MaxWeight = Convert.ToInt32(data.Values[1]),
                    // TODO: flags
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
                    ExtraDescriptions = data.ExtraDescr,
                    Cost = Convert.ToInt32(data.Cost),
                    Weight = data.Weight,
                    WearLocation = ConvertWearLocation(data),
                    MaxPeople = 0,
                    MaxWeight = 0,
                    HealBonus = 0,
                    ResourceBonus = 0
                };
            }
            else
            // TODO: other item type
                blueprint = null;
            if (blueprint != null)
                Repository.World.AddItemBlueprint(blueprint);
            return blueprint;
        }

        public static IItem CreateItem(ItemBlueprintBase blueprint, IContainer container)
        {
            if (blueprint is ItemWeaponBlueprint)
                return Repository.World.AddItemWeapon(Guid.NewGuid(), (ItemWeaponBlueprint) blueprint, container);
            if (blueprint is ItemContainerBlueprint)
                return Repository.World.AddItemContainer(Guid.NewGuid(),(ItemContainerBlueprint) blueprint, container);
            if (blueprint is ItemArmorBlueprint)
                return Repository.World.AddItemArmor(Guid.NewGuid(), (ItemArmorBlueprint) blueprint, container);
            if (blueprint is ItemLightBlueprint)
                return Repository.World.AddItemLight(Guid.NewGuid(), (ItemLightBlueprint) blueprint, container);
            if (blueprint is ItemFurnitureBlueprint)
                return Repository.World.AddItemFurniture(Guid.NewGuid(), (ItemFurnitureBlueprint) blueprint, container);
            // TODO: other blueprint
            return null;
        }

        // tables.C:601 type_flags
        //"light" OK
        //"scroll"
        //"wand"
        //"staff"
        //"weapon" OK
        //"treasure" -> not mapped
        //"armor" OK
        //"potion"
        //"furniture" OK
        //"trash" -> not mapped
        //"container" OK
        //"drinkcontainer" -> not mapped
        //"key"
        //"food"
        //"money"
        //"boat"
        //"npccorpse" -> not mapped
        //"pccorpse" -> not mapped
        //"fountain" -> mapped into furniture
        //"pill" -> not mapped
        //"map"
        //"portal"
        //"warpstone" -> not mapped
        //"component"
        //"gem"
        //"jewelry"
        //"instrument" -> not mapped
        //"clothing"
        //"window" -> not mapped
        //"template" -> not mapped
        //"saddle" -> not mapped
        //"rope" -> not mapped

        private static void CreateMidgaard()
        {
            MysteryImporter importer = new MysteryImporter();
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

            foreach (KeyValuePair<string,int> kv in importer.Objects.GroupBy(o => o.ItemType).ToDictionary(g => g.Key, g => g.Count()).OrderBy(x => x.Value))
                Log.Default.WriteLine(LogLevels.Info, "{0} -> {1}", kv.Key, kv.Value);

            Dictionary<int, IRoom> roomsByVNums = new Dictionary<int, IRoom>();

            // Create Rooms blueprints
            foreach (RoomData importedRoom in importer.Rooms)
                CreateRoomBlueprint(importedRoom);
            // Create Character blueprints
            foreach (MobileData mobile in importer.Mobiles)
                CreateCharacterBlueprint(mobile);
            // Create Item blueprints
            foreach (ObjectData obj in importer.Objects)
                CreateItemBlueprint(obj);
            
            // Create Rooms
            foreach (RoomData importedRoom in importer.Rooms)
            {
                IRoom room = Repository.World.AddRoom(Guid.NewGuid(), Repository.World.GetRoomBlueprint(importedRoom.VNum));
                roomsByVNums.Add(importedRoom.VNum, room);
            }

            // Create Exits
            foreach (RoomData importedRoom in importer.Rooms)
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
                            Log.Default.WriteLine(LogLevels.Error, "Origin room not found for vnum {0}", importedRoom.VNum);
                        else if (to == null)
                            Log.Default.WriteLine(LogLevels.Error, "Destination room not found for vnum {0}", importedRoom.VNum);
                        else
                        {
                            ExitBlueprint exitBlueprint = new ExitBlueprint
                            {
                                Destination = exit.DestinationVNum,
                                Description = exit.Description,
                                Key = exit.Key,
                                Keyword = exit.Keyword
                            };
                            Repository.World.AddExit(from, to, exitBlueprint, (ExitDirections)i);
                        }
                    }
                }
            }

            // Handle resets
            ICharacter lastCharacter = null;
            IItemContainer lastContainer = null;
            Dictionary<string, int> itemTypes = new Dictionary<string, int>();
            foreach (RoomData importedRoom in importer.Rooms.Where(x => x.Resets.Any()))
            {
                IRoom room;
                roomsByVNums.TryGetValue(importedRoom.VNum, out room);
                foreach (ResetData reset in importedRoom.Resets)
                {
                    switch (reset.Command)
                    {
                        case 'M':
                        {
                            CharacterBlueprint blueprint = Repository.World.GetCharacterBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                lastCharacter = Repository.World.AddCharacter(Guid.NewGuid(), blueprint, room);
                                Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: M: Mob {reset.Arg1} added");
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: M: Mob {reset.Arg1} not found");
                            break;
                        }
                        case 'O':
                        {
                            ItemBlueprintBase blueprint = Repository.World.GetItemBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                IItem item = CreateItem(blueprint, room);
                                if (item != null)
                                    Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: O: Obj {reset.Arg1} added room");
                                else
                                    Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: O: Obj {reset.Arg1} not created");
                                lastContainer = item as IItemContainer; // even if item is not a container, we have to convert it
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: O: Obj {reset.Arg1} not found");
                            //ObjectData obj = importer.Objects.FirstOrDefault(x => x.VNum == reset.Arg1);
                            //if (obj != null)
                            //{
                            //    if (obj.ItemType == "weapon")
                            //    {
                            //        ItemWeaponBlueprint blueprint = Repository.World.GetItemBlueprint(reset.Arg1) as ItemWeaponBlueprint;
                            //        Repository.World.AddItemWeapon(Guid.NewGuid(), blueprint, room);
                            //        Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: Weapon {reset.Arg1} added on floor");
                            //    }
                            //    else if (obj.ItemType == "container")
                            //    {
                            //        ItemContainerBlueprint blueprint = Repository.World.GetItemBlueprint(reset.Arg1) as ItemContainerBlueprint;
                            //        lastContainer = Repository.World.AddItemContainer(Guid.NewGuid(), blueprint, room);
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
                            ItemBlueprintBase blueprint = Repository.World.GetItemBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                if (lastContainer != null)
                                {
                                    CreateItem(blueprint, lastContainer);
                                    Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: P: Obj {reset.Arg1} added in {lastContainer.Blueprint.Id}");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: P: Last item was not a container");
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: P: Obj {reset.Arg1} not found");
                            break;
                        }
                        // G: give object arg1 to mobile 
                        case 'G':
                        {
                            ItemBlueprintBase blueprint = Repository.World.GetItemBlueprint(reset.Arg1);
                            if (blueprint != null)
                            {
                                if (lastCharacter != null)
                                {
                                    CreateItem(blueprint, lastCharacter);
                                    Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: G: Obj {reset.Arg1} added on {lastCharacter.Blueprint.Id}");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: G: Last character doesn't exist");
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: G: Obj {reset.Arg1} not found");
                            break;
                        }
                        // E: equip object arg1 to mobile
                        case 'E':
                            {
                                ItemBlueprintBase blueprint = Repository.World.GetItemBlueprint(reset.Arg1);
                                if (blueprint != null)
                                {
                                    if (lastCharacter != null)
                                    {
                                        CreateItem(blueprint, lastCharacter);
                                        Log.Default.WriteLine(LogLevels.Info, $"Room {importedRoom.VNum}: E: Obj {reset.Arg1} added on {lastCharacter.Blueprint.Id}");
                                        // TODO: try to equip
                                    }
                                    else
                                        Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: E: Last character doesn't exist");
                                }
                                else
                                    Log.Default.WriteLine(LogLevels.Error, $"Room {importedRoom.VNum}: E: Obj {reset.Arg1} not found");
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
            CharacterBlueprint mob3Blueprint = new CharacterBlueprint
            {
                Id = 3,
                Name = "mob3",
                ShortDescription = "Third mob (male)",
                Description = "Third mob (male) is here",
                Sex = Sex.Male,
                Level = 10
            };
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
            ItemContainerBlueprint item1Blueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "item1",
                ShortDescription = "First item (container)",
                Description = "The first item (container) has been left here.",
                ItemCount = 10,
                WeightMultiplier = 100
            };
            ItemWeaponBlueprint item2Blueprint = new ItemWeaponBlueprint
            {
                Id = 2,
                Name = "item2",
                ShortDescription = "Second item (weapon)",
                Description = "The second item (weapon) has been left here.",
                Type = WeaponTypes.Axe1H,
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
                Armor = 100,
                ArmorKind = ArmorKinds.Mail,
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
                Type = WeaponTypes.Sword1H,
                DiceCount = 5,
                DiceValue = 40,
                DamageType = SchoolTypes.Physical,
                WearLocation = WearLocations.Wield
            };
            ItemWeaponBlueprint item6Blueprint = new ItemWeaponBlueprint
            {
                Id = 6,
                Name = "item6",
                ShortDescription = "Sixth item (weapon)",
                Description = "The sixth item (weapon) has been left here.",
                Type = WeaponTypes.Mace2H,
                DiceCount = 10,
                DiceValue = 20,
                DamageType = SchoolTypes.Holy,
                WearLocation = WearLocations.Wield
            };
            ItemShieldBlueprint item7Blueprint = new ItemShieldBlueprint
            {
                Id = 7,
                Name = "item7",
                ShortDescription = "Seventh item (shield)",
                Description = "The seventh item (shield) has been left here.",
                Armor = 1000,
                WearLocation = WearLocations.Shield
            };

            //
            ServerOptions.CorpseBlueprint = new ItemCorpseBlueprint(); // this is mandatory

            // Add dummy mobs and items to allow impersonate :)
            IRoom templeOfMota = Repository.World.GetRooms().FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
            IRoom templeSquare = Repository.World.GetRooms().FirstOrDefault(x => x.Name.ToLower() == "the temple square");

            ICharacter mob1 = Repository.World.AddCharacter(Guid.NewGuid(), "mob1", Repository.ClassManager["Mage"], Repository.RaceManager["Troll"], Sex.Male, templeOfMota); // playable
            ICharacter mob2 = Repository.World.AddCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
            ICharacter mob3 = Repository.World.AddCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
            //ICharacter mob4 = Repository.World.AddCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
            ICharacter mob4 = Repository.World.AddCharacter(Guid.NewGuid(), "mob4", Repository.ClassManager["Warrior"], Repository.RaceManager["Dwarf"], Sex.Female, templeSquare); // playable
            ICharacter mob5 = Repository.World.AddCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

            IItemContainer item1 = Repository.World.AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItemContainer item1Dup1 = Repository.World.AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItemWeapon item2 = Repository.World.AddItemWeapon(Guid.NewGuid(), item2Blueprint, mob2);
            IItemArmor item3 = Repository.World.AddItemArmor(Guid.NewGuid(), item3Blueprint, item1Dup1);
            IItemLight item4 = Repository.World.AddItemLight(Guid.NewGuid(), item4Blueprint, mob1);
            IItemWeapon item5 = Repository.World.AddItemWeapon(Guid.NewGuid(), item5Blueprint, mob1);
            IItemContainer item1Dup2 = Repository.World.AddItemContainer(Guid.NewGuid(), item1Blueprint, mob1);
            IItemArmor item3Dup1 = Repository.World.AddItemArmor(Guid.NewGuid(), item3Blueprint, mob3);
            IItemLight item4Dup1 = Repository.World.AddItemLight(Guid.NewGuid(), item4Blueprint, mob4);
            IItemWeapon item6 = Repository.World.AddItemWeapon(Guid.NewGuid(), item6Blueprint, templeSquare);
            IItemShield item7 = Repository.World.AddItemShield(Guid.NewGuid(), item7Blueprint, templeOfMota);
            // Equip weapon on mob2
            mob2.Equipments.First(x => x.Slot == EquipmentSlots.Wield).Item = item2;
            item2.ChangeContainer(null);
            item2.ChangeEquipedBy(mob2);
        }
    }
}
