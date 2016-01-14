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
            Server.Server.Instance.Initialize(new List<INetworkServer> { telnetServer, this });
            Server.Server.Instance.Start();

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
                Server.Server.Instance.Stop();
                Application.Current.Shutdown();
            }
            else if (input == "alist")
            {
                OutputText("Admins:");
                foreach (IAdmin a in Server.Server.Instance.GetAdmins())
                    OutputText(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.Name : "") + " " + (a.Incarnating != null ? a.Incarnating.Name : ""));
            }
            else if (input == "plist")
            {
                OutputText("players:");
                foreach (IPlayer p in Server.Server.Instance.GetPlayers())
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
            if (NewClientConnected != null)
                NewClientConnected(window);
            window.Closed += (sender, args) =>
            {
                if (ClientDisconnected != null)
                    ClientDisconnected(window);
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
            _serverWindowInstance.Dispatcher.Invoke(() =>
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

            Dictionary<int, IRoom> roomsByVNums = new Dictionary<int, IRoom>();

            // Create Rooms
            foreach (RoomData importedRoom in importer.Rooms)
            {
                RoomBlueprint roomBlueprint = new RoomBlueprint
                {
                    Id = importedRoom.VNum,
                    Name = importedRoom.Name,
                    Description = importedRoom.Description,
                };
                IRoom room = World.World.Instance.AddRoom(Guid.NewGuid(), roomBlueprint);
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
                            World.World.Instance.AddExit(from, to, null, (ServerOptions.ExitDirections)i);
                        }
                    }
                }
            }
            //// Handle resets
            //Dictionary<string, int> itemTypes = new Dictionary<string, int>();
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
            //                    World.World.Instance.AddCharacter(Guid.NewGuid(), mob.Name, room);
            //                break;
            //            case 'O':
            //                ObjectData obj = importer.Objects.FirstOrDefault(x => x.VNum == reset.Arg1);
            //                if (obj != null)
            //                {
            //                    if (obj.ItemType == "weapon")
            //                    {
            //                        ItemWeaponBlueprint blueprint = new ItemWeaponBlueprint
            //                        {
            //                            Name = obj.Name,
            //                            ShortDescription = obj.ShortDescr,
            //                            Description = obj.Description,
            //                            Cost = Convert.ToInt32(obj.Cost),
            //                            Weight = obj.Weight,
            //                            // TODO: weapon type Values[0]
            //                            DiceCount = Convert.ToInt32(obj.Values[1]),
            //                            DiceValue = Convert.ToInt32(obj.Values[2]),
            //                            // TODO: damage type Values[3]
            //                        };
            //                        World.World.Instance.AddItemWeapon(Guid.NewGuid(), blueprint, room);
            //                    }
            //                    else if (obj.ItemType == "container")
            //                    {
            //                        ItemContainerBlueprint blueprint = new ItemContainerBlueprint
            //                        {
            //                            Name = obj.Name,
            //                            ShortDescription = obj.ShortDescr,
            //                            Description = obj.Description,
            //                            Cost = Convert.ToInt32(obj.Cost),
            //                            Weight = obj.Weight,
            //                            ItemCount = Convert.ToInt32(obj.Values[3]),
            //                            WeightMultiplier = Convert.ToInt32(obj.Values[4]),
            //                        };
            //                        World.World.Instance.AddItemContainer(Guid.NewGuid(), blueprint, room);
            //                    }

            //                    if (!itemTypes.ContainsKey(obj.ItemType))
            //                        itemTypes.Add(obj.ItemType, 1);
            //                    else
            //                        itemTypes[obj.ItemType]++;
            //                }
            //                break;
            //            // TODO: other command  P, E, G, D, R, Z
            //        }
            //    }
            //}
            //foreach(KeyValuePair<string, int> kv in itemTypes)
            //    Log.Default.WriteLine(LogLevels.Info, "{0} -> {1}", kv.Key, kv.Value);

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

            //
            ServerOptions.CorpseBlueprint = new ItemCorpseBlueprint(); // this is mandatory

            // Add dummy mobs and items to allow impersonate :)
            IRoom templeOfMota = World.World.Instance.GetRooms().FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
            IRoom templeSquare = World.World.Instance.GetRooms().FirstOrDefault(x => x.Name.ToLower() == "the temple square");

            ICharacter mob1 = World.World.Instance.AddCharacter(Guid.NewGuid(), "mob1", templeOfMota); // playable
            ICharacter mob2 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
            ICharacter mob3 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
            //ICharacter mob4 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
            ICharacter mob4 = World.World.Instance.AddCharacter(Guid.NewGuid(), "mob4", templeSquare); // playable
            ICharacter mob5 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

            IItemContainer item1 = World.World.Instance.AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItemContainer item1Dup1 = World.World.Instance.AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItemWeapon item2 = World.World.Instance.AddItemWeapon(Guid.NewGuid(), item2Blueprint, mob2);
            IItemArmor item3 = World.World.Instance.AddItemArmor(Guid.NewGuid(), item3Blueprint, item1Dup1);
            IItemLight item4 = World.World.Instance.AddItemLight(Guid.NewGuid(), item4Blueprint, mob1);
            IItemWeapon item5 = World.World.Instance.AddItemWeapon(Guid.NewGuid(), item5Blueprint, mob1);
            IItemContainer item1Dup2 = World.World.Instance.AddItemContainer(Guid.NewGuid(), item1Blueprint, mob1);
            IItemArmor item3Dup1 = World.World.Instance.AddItemArmor(Guid.NewGuid(), item3Blueprint, mob3);
            IItemLight item4Dup1 = World.World.Instance.AddItemLight(Guid.NewGuid(), item4Blueprint, mob4);
            // Equip weapon on mob2
            mob2.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.Wield).Item = item2;
            item2.ChangeContainer(null);
            item2.ChangeEquipedBy(mob2);
        }
    }
}
