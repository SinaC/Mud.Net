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
using Mud.Network.Socket;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
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
            INetworkServer socketServer = new SocketServer(11000);
            Server.Server.Instance.Initialize(false, new List<INetworkServer> { socketServer, this });
            Server.Server.Instance.Start();

            //CreateNewClientWindow();
        }

        private void InputTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SendButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); // http://stackoverflow.com/questions/728432/how-to-programmatically-click-a-button-in-wpf
            else if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
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
                Paragraph paragraph = new Paragraph();
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
                paragraph.Inlines.Add(new Bold(new Run(level + ": "))
                {
                    Foreground = color
                });
                paragraph.Inlines.Add(message);
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
                            World.World.Instance.AddExit(from, to, (ServerOptions.ExitDirections)i, false);
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
            //                    World.World.Instance.AddCharacter(Guid.NewGuid(), mob.Name, room);
            //                break;
            //            case 'O':
            //                ObjectData obj = importer.Objects.FirstOrDefault(x => x.VNum == reset.Arg1);
            //                if (obj != null) // TODO: itemType
            //                    World.World.Instance.AddItemContainer(Guid.NewGuid(), obj.Name, room);
            //                break;
            //            // TODO: other command  P, E, G, D, R, Z
            //        }
            //    }
            //}

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
            CharacterBlueprint mob4Blueprint = new CharacterBlueprint
            {
                Id = 4,
                Name = "mob4",
                ShortDescription = "Fourth mob (neutral)",
                Description = "Fourth mob (neutral) is here",
                Sex = Sex.Neutral,
                Level = 10
            };
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

            //
            ServerOptions.CorpseBlueprint = new ItemCorpseBlueprint();

            // Add dummy mobs and items to allow impersonate :)
            IRoom templeOfMota = World.World.Instance.GetRooms().FirstOrDefault(x => x.Name.ToLower() == "the temple of mota");
            IRoom templeSquare = World.World.Instance.GetRooms().FirstOrDefault(x => x.Name.ToLower() == "the temple square");

            ICharacter mob1 = World.World.Instance.AddCharacter(Guid.NewGuid(), "mob1", templeOfMota); // playable
            ICharacter mob2 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob2Blueprint, templeOfMota);
            ICharacter mob3 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob3Blueprint, templeSquare);
            ICharacter mob4 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob4Blueprint, templeSquare);
            ICharacter mob5 = World.World.Instance.AddCharacter(Guid.NewGuid(), mob5Blueprint, templeSquare);

            // Item1*2 in Room1
            // Item2 in Mob2
            // Item3 in 2.Item1
            // Item4 in Mob1
            // Item1 in Mob1
            IItem item1 = World.World.Instance.AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItem item1Dup1 = World.World.Instance.AddItemContainer(Guid.NewGuid(), item1Blueprint, templeOfMota);
            IItem item2 = World.World.Instance.AddItemWeapon(Guid.NewGuid(), item2Blueprint, mob2);
            IItem item3 = World.World.Instance.AddItemArmor(Guid.NewGuid(), item3Blueprint, item1Dup1 as IContainer);
            IItem item4 = World.World.Instance.AddItemLight(Guid.NewGuid(), item4Blueprint, mob1);
            IItem item1Dup2 = World.World.Instance.AddItemContainer(Guid.NewGuid(), item1Blueprint, mob1);
            IItem item3Dup1 = World.World.Instance.AddItemArmor(Guid.NewGuid(), item3Blueprint, mob3);
            IItem item4Dup1 = World.World.Instance.AddItemLight(Guid.NewGuid(), item4Blueprint, mob4);
        }
    }
}
